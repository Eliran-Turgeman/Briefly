using Briefly.Data;
using Briefly.Services.Publishing.Publishers;

namespace Briefly.Services.Publishing;

public class PublishingService : IPublishingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublishingService> _logger;

    public PublishingService(IServiceProvider serviceProvider, ILogger<PublishingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync(BlogPost blogToPublish, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrieflyContext>();

        var message = $"{blogToPublish.Summary}\n\n{blogToPublish.Url}";

        await publisher.PublishMessageAsync(message);

        // Mark the blog post as published
        blogToPublish.IsPublished = true;
        blogToPublish.PublishedAt = DateTime.Now;
        dbContext.BlogPost.Update(blogToPublish);

        // Record the published message
        var publishedMessage = new PublishedMessage
        {
            Content = message,
            PublishedAt = DateTime.Now,
            Media = "Telegram",
            MediaIdentifier = publisher.MediaIdentifier
        };
        dbContext.PublishedMessages.Add(publishedMessage);

        await dbContext.SaveChangesAsync(stoppingToken);

        _logger.LogInformation($"Published blog post ID {blogToPublish.Id}");
    }
}
