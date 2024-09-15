using TL;
using WTelegram;

namespace Briefly.Services.Publishing.Publishers;

public class TelegramPublisher : IPublisher, IDisposable
{
    private readonly Client _client;
    private readonly long _channelId;
    private readonly string _apiId;
    private readonly string _apiHash;
    private readonly string _phoneNumber;
    private InputPeerChannel? _channelPeer = null;
    private readonly ILogger<TelegramPublisher> _logger;

    public string MediaIdentifier => _channelId.ToString();

    public TelegramPublisher(string apiId, string apiHash, string phoneNumber, long channelId, ILogger<TelegramPublisher> logger)
    {
        _apiId = apiId;
        _apiHash = apiHash;
        _phoneNumber = phoneNumber;
        _logger = logger;
        _channelId = channelId;

        _client = new Client(Config);
    }

    private async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Telegram publisher");
        await _client.LoginUserIfNeeded();

        var dialogs = await _client.Messages_GetAllDialogs();
        var chat = dialogs.chats.Values
            .OfType<Channel>()
            .FirstOrDefault(c => c.ID == _channelId);

        if (chat == null)
        {
            throw new Exception($"Channel with ID {_channelId} not found.");
        }

        _channelPeer = new InputPeerChannel(chat.ID, chat.access_hash);
    }

    private string Config(string what)
    {
        switch (what)
        {
            case "api_id": return _apiId;
            case "api_hash": return _apiHash;
            case "phone_number": return _phoneNumber;
            case "verification_code":
                Console.Write("Enter the code you received: ");
                return Console.ReadLine();
            case "password":
                Console.Write("Enter your 2FA password: ");
                return Console.ReadLine();
            default: return null;
        }
    }

    public async Task PublishMessageAsync(string message)
    {
        _logger.LogInformation($"Publishing message to Telegram channel {_channelId}");
        await InitializeAsync();
        await _client.SendMessageAsync(_channelPeer, message);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
