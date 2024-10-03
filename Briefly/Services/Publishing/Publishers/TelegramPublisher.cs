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

    public TelegramPublisher(string apiId, string apiHash, string phoneNumber,
        long channelId, ILogger<TelegramPublisher> logger)
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
            case "verification_code":return fetchVerificationCode();
            case "password":
                Console.Write("Enter your 2FA password: ");
                return Console.ReadLine()!;
            default: return null;
        }
    }

    /// <summary>
    /// Try to read verification code from file /app/data/telegram_verification_code.txt (path from container)
    /// This is a workaround to avoid manual input of verification code.
    /// Once the service is running, the admin should copy the verification code to the file, using the container's shell.
    /// i.e docker-compose exec app sh, then echo $VERIFICATION_CODE > /app/data/telegram_verification_code.txt
    /// This method will allow exceptions to be thrown if the file is not found, to let the admin enough time to copy the code.
    /// </summary>
    /// <returns></returns>
    private string fetchVerificationCode()
    {
        string filePath = "/app/data/telegram_verification_code.txt";
        int retries = 5;
        int delayBetweenRetries = 10000;  // 10 seconds

        for (int i = 0; i < retries; i++)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string verificationCode = reader.ReadToEnd();
                        return verificationCode.Trim();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading verification code file: " + ex.Message);
                }
            }

            _logger.LogWarning($"Verification code file not found. Retrying in {delayBetweenRetries / 1000} seconds...");
            Thread.Sleep(delayBetweenRetries);
        }

        throw new FileNotFoundException($"Verification code file was not found after {retries} attempts.");
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
