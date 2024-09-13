namespace Briefly.Services.Summarization;

public interface ITextSummaryProvider
{
    Task<string> GenerateSummaryAsync(string content);
}
