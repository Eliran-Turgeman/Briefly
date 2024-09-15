namespace Briefly.Services.Summarization.ContentFetchers.Strategies;

public interface IContentFetchStrategy
{
    Task<string> FetchContentAsync(string url);
}
