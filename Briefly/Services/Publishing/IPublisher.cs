namespace Briefly.Services.Publishing;

public interface IPublisher
{
    string MediaIdentifier { get; }
    Task SendMessageAsync(string message);
}
