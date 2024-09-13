using Briefly.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Briefly.Services.Publishing;

public class PublishingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublishingService> _logger;

    public PublishingService(IServiceProvider serviceProvider, ILogger<PublishingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BrieflyContext>();
                var publisherService = scope.ServiceProvider.GetRequiredService<IPublisher>();

                // Retrieve the last published timestamp
                DateTime? lastPublishedTimestamp = await dbContext.PublishedMessages
                    .OrderByDescending(pm => pm.PublishedAt)
                    .Select(pm => (DateTime?)pm.PublishedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                // Determine next run time
                DateTime nextRunTime;

                if (lastPublishedTimestamp.HasValue)
                {
                    // Schedule for 24 hours after the last published time
                    nextRunTime = lastPublishedTimestamp.Value.AddDays(1).Date.AddHours(9);
                }
                else
                {
                    // No previous messages published, schedule for the next occurrence at 9 AM
                    nextRunTime = DateTime.Today.AddHours(9);
                    if (DateTime.Now > nextRunTime)
                    {
                        nextRunTime = nextRunTime.AddDays(1);
                    }
                }

                // Calculate delay
                TimeSpan delay = nextRunTime - DateTime.Now;

                if (delay.TotalMilliseconds > 0)
                {
                    _logger.LogInformation($"Waiting {delay} until next run at {nextRunTime}.");
                    await Task.Delay(delay, stoppingToken);
                }

                // Proceed to publish
                var blogPost = await dbContext.BlogPost
                    .FirstOrDefaultAsync(bp => bp.IsApprovedSummary && !bp.IsPublished, stoppingToken);

                if (blogPost != null)
                {
                    var message = $"{blogPost.Summary}\n\n{blogPost.Url}";

                    await publisherService.SendMessageAsync(message);

                    // Mark the blog post as published
                    blogPost.IsPublished = true;
                    dbContext.BlogPost.Update(blogPost);

                    // Record the published message
                    var publishedMessage = new PublishedMessage
                    {
                        Content = message,
                        PublishedAt = DateTime.Now,
                        Media = "Telegram",
                        MediaIdentifier = publisherService.MediaIdentifier
                    };
                    dbContext.PublishedMessages.Add(publishedMessage);

                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation($"Published blog post ID {blogPost.Id}");
                }
                else
                {
                    _logger.LogInformation("No approved and unpublished blog posts found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while publishing the blog post.");
            }
        }
    }
}
