using System.Collections.Specialized;
using System.Text;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace TgToVkPoster;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TgToVkConfiguration _configuration;
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    public Worker(TgToVkConfiguration config, HttpClient client, ILogger<Worker> logger)
    {
        _logger = logger;
        if (config.TgbotToken is null)
            throw new NullReferenceException($"{nameof(config.TgbotToken)} not set");
        _configuration = config;
        _client = client;
        _baseUrl = $"https://api.vk.com/method/wall.post?v=5.131&access_token={_configuration.VkAccessToken}&owner_id={_configuration.OwnerId}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(_configuration.TgbotToken!);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        var updateReceiver = new QueuedUpdateReceiver(bot, receiverOptions);

        await foreach (var update in updateReceiver.WithCancellation(stoppingToken))
        {
            _logger.LogDebug("Update type: {update}", update.Type);
            Message? message = update.Type switch
            {
                UpdateType.ChannelPost => update.ChannelPost,
                UpdateType.Message => update.Message,
                _ => null,
            };
            if (message is null)
            {
                _logger.LogError("unsupported {Type} ", update.Type);
                continue;
            }
            var uriBuider = new StringBuilder(_baseUrl);
            if (!string.IsNullOrEmpty(message.Text))
                uriBuider.Append($"&message={message.Text}");
            if (_configuration.OwnerId < 0)
                uriBuider.Append("&from_group=1");
            var result = await _client.GetAsync(uriBuider.ToString(), stoppingToken);
            _logger.LogDebug("{response}", await result.Content.ReadAsStringAsync(stoppingToken));

        }
    }
}
