using Briefly.Data;
using Microsoft.EntityFrameworkCore;
using SmartReader;

namespace Briefly.Services.Summarization;

public class SummarizationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SummarizationService> _logger;

    public SummarizationService(IServiceScopeFactory scopeFactory, ILogger<SummarizationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BrieflyContext>();
                var llmProvider = scope.ServiceProvider.GetRequiredService<ITextSummaryProvider>();

                var blogPosts = await dbContext.BlogPost
                    .Where(bp => string.IsNullOrEmpty(bp.Summary))
                    .Where(bp => !bp.IsPublished)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation($"Found {blogPosts.Count} blog posts to summarize.");

                foreach (var blogPost in blogPosts)
                {
                    try
                    {
                        var content = await FetchContentAsync(blogPost.Url!);
                        var summary = await llmProvider.GenerateSummaryAsync(content);

                        blogPost.Summary = summary;

                        dbContext.Update(blogPost);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical($"Error summarizing blog post {blogPost.Id}: {ex.Message}");
                    }
                }
            }

            // Wait before checking again
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task<string> FetchContentAsync(string url)
    {
        var article = new Reader(url).GetArticle();

        if (!article.IsReadable)
        {
            _logger.LogWarning($"Failed to extract readable content from {url}. Falling back to HTTP request.");
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        return article.TextContent;
    }
}

