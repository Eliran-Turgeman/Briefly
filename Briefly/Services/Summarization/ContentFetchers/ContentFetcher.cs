using Briefly.Services.Summarization.ContentFetchers.Strategies;

namespace Briefly.Services.Summarization.ContentFetchers;

public class ContentFetcher : IContentFetcher
{
    private readonly ILogger<ContentFetcher> _logger;
    private readonly IEnumerable<IContentFetchStrategy> _fetchStrategies;

    public ContentFetcher(ILogger<ContentFetcher> logger, IEnumerable<IContentFetchStrategy> fetchStrategies)
    {
        _logger = logger;
        _fetchStrategies = fetchStrategies;
    }

    public async Task<string> FetchContentAsync(string url)
    {
        foreach (var strategy in _fetchStrategies)
        {
            try
            {
                return await strategy.FetchContentAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Fetching content using {strategy.GetType().Name} failed for {url}");
            }
        }

        throw new InvalidOperationException("All content fetching strategies failed");
    }
}

