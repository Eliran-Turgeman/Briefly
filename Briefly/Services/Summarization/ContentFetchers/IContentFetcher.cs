namespace Briefly.Services.Summarization.ContentFetchers;

public interface IContentFetcher
{
    Task<string> FetchContentAsync(string url);
}

