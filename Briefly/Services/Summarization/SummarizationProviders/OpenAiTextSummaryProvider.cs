using Briefly.Services.Summarization.Prompts;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text.RegularExpressions;

namespace Briefly.Services.Summarization.SummarizationProviders;

public class OpenAiTextSummaryProvider : ITextSummaryProvider
{
    private readonly OpenAIAPI _openAiApi;
    private readonly ILogger<OpenAiTextSummaryProvider> _logger;
    private readonly IPromptProvider _promptProvider;

    public OpenAiTextSummaryProvider(string apiKey,
        ILogger<OpenAiTextSummaryProvider> logger, IPromptProvider promptProvider)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "OpenAI API key must be provided.");

        _openAiApi = new OpenAIAPI(apiKey);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _promptProvider = promptProvider ?? throw new ArgumentNullException(nameof(promptProvider));
    }

    public async Task<string> GenerateSummaryAsync(string content)
    {
        _logger.LogInformation("Generating summary for content.");
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content to summarize must be provided.", nameof(content));

        var prompt = $"{_promptProvider.GetPrompt()}\n{content}";

        var chatRequest = new ChatRequest()
        {
            Model = OpenAI_API.Models.Model.ChatGPTTurbo,
            Temperature = 0.7,
            MaxTokens = 150,
            Messages =
            [
                new ChatMessage(ChatMessageRole.System, "You are an assistant that provides concise summaries."),
                new ChatMessage(ChatMessageRole.User, prompt)
            ]
        };

        try
        {
            var chatResult = await _openAiApi.Chat.CreateChatCompletionAsync(chatRequest);
            var summary = chatResult.Choices[0].Message.TextContent.Trim();

            return summary;
        }
        catch (Exception ex) when (ex.Message.Contains("context_length_exceeded"))
        {
            _logger.LogError(ex, "Error generating summary for content.");
            var overflowRate = ExtractTokenOverflowRate(ex.Message);
            chatRequest.Messages[1].TextContent = TrimPromptByRate(prompt, overflowRate);
            var chatResult = await _openAiApi.Chat.CreateChatCompletionAsync(chatRequest);
            _logger.LogInformation("Summary generated successfully after trimming prompt.");
            var summary = chatResult.Choices[0].Message.TextContent.Trim();

            return summary;
        }
    }

    /// <summary>
    /// Extract the rate between the token limit and the actual token count that caused the overflow.
    /// The result will be rounded up to the nearest integer.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    private int ExtractTokenOverflowRate(string errorMessage)
    {
        var maxTokensMatch = Regex.Match(errorMessage, @"This model's maximum context length is (\d+) tokens");
        var actualTokensMatch = Regex.Match(errorMessage, @"your messages resulted in (\d+) tokens");

        if (!maxTokensMatch.Success || !actualTokensMatch.Success)
        {
            return 0;
        }

        var maxTokens = double.Parse(maxTokensMatch.Groups[1].Value);
        var actualTokens = double.Parse(actualTokensMatch.Groups[1].Value);

        return (int)Math.Ceiling(actualTokens / maxTokens);
    }

    /// <summary>
    /// Trim the prompt by the rate between the token limit and the actual token count that caused the overflow.
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    private string TrimPromptByRate(string prompt, int rate)
    {
        var newPromptLength = (int)Math.Ceiling(prompt.Length / (double)rate);
        return prompt.Substring(0, newPromptLength);
    }
}
