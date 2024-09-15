using SmartReader;

namespace Briefly.Services.Summarization.ContentFetchers.Strategies;

public class SmartReaderFetchStrategy : IContentFetchStrategy
{
    private readonly ILogger<SmartReaderFetchStrategy> _logger;

    public SmartReaderFetchStrategy(ILogger<SmartReaderFetchStrategy> logger)
    {
        _logger = logger;
    }

    public Task<string> FetchContentAsync(string url)
    {
        var article = new Reader(url).GetArticle();

        if (!article.IsReadable)
        {
            _logger.LogWarning($"Failed to extract readable content from {url}. Switching to fallback.");
            throw new InvalidOperationException("Content not readable");
        }

        return Task.FromResult(article.TextContent);
    }
}
