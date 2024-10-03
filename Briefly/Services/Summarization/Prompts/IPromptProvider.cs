namespace Briefly.Services.Summarization.Prompts;

public interface IPromptProvider
{
    string GetPrompt();

    void setPromptType(string promptType);
}
