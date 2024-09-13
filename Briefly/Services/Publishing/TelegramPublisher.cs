using TL;
using WTelegram;

namespace Briefly.Services.Publishing;

public class TelegramPublisher : IPublisher, IDisposable
{
    private readonly Client _client;
    private readonly long _channelId;
    private readonly string _apiId;
    private readonly string _apiHash;
    private readonly string _phoneNumber;
    private InputPeerChannel? _channelPeer = null;

    public string MediaIdentifier => _channelId.ToString();

    public TelegramPublisher(string apiId, string apiHash, string phoneNumber, long channelId)
    {
        _apiId = apiId;
        _apiHash = apiHash;
        _phoneNumber = phoneNumber;
        _client = new Client(Config);
        _channelId = channelId;

        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
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

    public async Task SendMessageAsync(string message)
    {
        await _client.SendMessageAsync(_channelPeer, message);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
