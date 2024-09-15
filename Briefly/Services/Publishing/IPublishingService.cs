namespace Briefly.Services.Publishing;

public interface IPublishingService
{
    Task PublishAsync(BlogPost blogToPublish, CancellationToken stoppingToken);
}
