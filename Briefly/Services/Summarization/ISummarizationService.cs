namespace Briefly.Services.Summarization;

public interface ISummarizationService
{
    Task SummarizeAsync(BlogPost post, CancellationToken stoppingToken);
}
