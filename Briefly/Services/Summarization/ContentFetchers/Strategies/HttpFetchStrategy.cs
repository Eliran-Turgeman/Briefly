namespace Briefly.Services.Summarization.ContentFetchers.Strategies;

public class HttpFetchStrategy : IContentFetchStrategy
{
    public async Task<string> FetchContentAsync(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}

