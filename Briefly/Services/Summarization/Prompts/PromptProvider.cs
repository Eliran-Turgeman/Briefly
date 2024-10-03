namespace Briefly.Services.Summarization.Prompts;

public class PromptProvider : IPromptProvider
{
    private readonly ILogger<PromptProvider> _logger;

    public PromptProvider(ILogger<PromptProvider> logger)
    {
        _logger = logger;
    }

    public string GetPrompt()
    {
        var promptType = fetchPromptType();
        return promptType switch
        {
            "SummarizeContent" => Prompts.SummarizeContentPrompt,
            "QuoteFromContent" => Prompts.QuoteFromContentPrompt,
            _ => Prompts.SummarizeContentPrompt
        };
    }

    /// <summary>
    /// Reads the selected prompt type from the file located at /app/data/summarizer_prompt_type.txt.
    /// If the file doesn't exist, returns a default prompt type ("SummarizeContent").
    /// </summary>
    /// <returns>The prompt type as a string.</returns>
    private string fetchPromptType()
    {
        var filePath = "/app/data/summarizer_prompt_type.txt";
        var defaultPromptType = "SummarizeContent";
        try
        {
            if (File.Exists(filePath))
            {
                var promptType = File.ReadAllText(filePath).Trim();
                _logger.LogInformation($"Prompt type file found. Using prompt type: {promptType}");
                return promptType;
            }
            else
            {
                _logger.LogWarning("Prompt type file not found. Using default.");
                return defaultPromptType;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading prompt type file: {ex.Message}. Using default.");
            return defaultPromptType;
        }
    }

}
