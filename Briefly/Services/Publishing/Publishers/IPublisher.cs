namespace Briefly.Services.Publishing.Publishers;

public interface IPublisher
{
    string MediaIdentifier { get; }
    Task PublishMessageAsync(string message);
}
