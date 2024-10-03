using Briefly.Data;
using Briefly.Services.Summarization.ContentFetchers;
using Briefly.Services.Summarization.SummarizationProviders;

namespace Briefly.Services.Summarization;

public class SummarizationService : ISummarizationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SummarizationService> _logger;
    private readonly IContentFetcher _contentFetcher;

    public SummarizationService(IServiceScopeFactory scopeFactory,
        ILogger<SummarizationService> logger,
        IContentFetcher contentFetcher)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _contentFetcher = contentFetcher;
    }

    public async Task SummarizeAsync(BlogPost postToSummarize, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Summarizing blog post {postToSummarize.Id}");
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrieflyContext>();
        var llmProvider = scope.ServiceProvider.GetRequiredService<ITextSummaryProvider>();

        try
        {
            var content = await _contentFetcher.FetchContentAsync(postToSummarize.Url!);
            var summary = await llmProvider.GenerateSummaryAsync(content);

            postToSummarize.Summary = summary;

            dbContext.Update(postToSummarize);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error summarizing blog post {postToSummarize.Id}: {ex.ToString()}");
        }
    }

    public async Task SummarizeAsync(int blogPostId, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrieflyContext>();
        var llmProvider = scope.ServiceProvider.GetRequiredService<ITextSummaryProvider>();

        var blogPost = await dbContext.BlogPost.FindAsync(blogPostId);

        if (blogPost == null)
        {
            _logger.LogWarning($"Blog post with ID {blogPostId} not found.");
            return;
        }

        await SummarizeAsync(blogPost, stoppingToken);
    }
}
