using OpenAI_API;
using OpenAI_API.Chat;

namespace Briefly.Services.Summarization;

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
        You  will help me generate a summary for a blog post.
        The summary should adhere to the following requirements:
        - summary should contain 2-3 sentences only.
        - summary should be in a simple language
        - summary can include some "wrapping" around the summary itself like "Interesting read, this post is about... " or "Just read about ... I think you will enjoy it" something like that - make it human.

        Your output should be the summary itself ONLY

        Please summarize the following blog post:\n {content}
        """;

        // Create a chat completion request
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

        var chatResult = await _openAiApi.Chat.CreateChatCompletionAsync(chatRequest);

        var summary = chatResult.Choices[0].Message.TextContent.Trim();

        return summary;
    }
}
