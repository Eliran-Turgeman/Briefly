using Briefly.Data;
using Microsoft.EntityFrameworkCore;
using SmartReader;

namespace Briefly.Services.Summarization;

public class SummarizationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SummarizationBackgroundService> _logger;
    private readonly ISummarizationService _summarizationService;

    public SummarizationBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<SummarizationBackgroundService> logger,
        ISummarizationService summarizationService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _summarizationService = summarizationService;
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
                        // Sumarizes the blog post and updates the summary in the database.
                        await _summarizationService.SummarizeAsync(blogPost, stoppingToken);
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
}

