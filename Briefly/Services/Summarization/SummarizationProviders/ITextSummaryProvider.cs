namespace Briefly.Services.Summarization.SummarizationProviders;

public interface ITextSummaryProvider
{
    Task<string> GenerateSummaryAsync(string content);
}
