using Microsoft.AspNetCore.Http.HttpResults;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Net;
using System.Text.RegularExpressions;

namespace Briefly.Services.Summarization.SummarizationProviders;

public class OpenAiTextSummaryProvider : ITextSummaryProvider
{
    private readonly OpenAIAPI _openAiApi;
    private readonly ILogger<OpenAiTextSummaryProvider> _logger;

    public OpenAiTextSummaryProvider(string apiKey, ILogger<OpenAiTextSummaryProvider> logger)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "OpenAI API key must be provided.");

        _openAiApi = new OpenAIAPI(apiKey);
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(string content)
    {
        _logger.LogInformation("Generating summary for content.");
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content to summarize must be provided.", nameof(content));

        var prompt = $"""
        Summarize the following blog post in 2-5 sentences using simple, direct language.
        Focus on the main idea of the post, and include an important quote from the content if relevant.
        Keep it raw and avoid fancy words or unnecessary details.

        The summary should adhere to the following requirements:
        - Be precise, don't mislead and don't be clickbaity.
        - Don't say words that will give the impression you are an LLM - like "delve" for example.

        Your output should be the summary itself ONLY

        Please summarize the following blog post:\n {content}
        """;

        var chatRequest = new ChatRequest()
        {
            Model = OpenAI_API.Models.Model.ChatGPTTurbo,
            Temperature = 0.7,
            MaxTokens = 150,
            Messages = new ChatMessage[]
            {
                new ChatMessage(ChatMessageRole.System, "You are an assistant that provides concise summaries."),
                new ChatMessage(ChatMessageRole.User, prompt)
            }
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
