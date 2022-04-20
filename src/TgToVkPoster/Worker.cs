using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VkNet.Model.Attachments;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

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
        if (!Directory.Exists("photos"))
            Directory.CreateDirectory("photos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(_configuration.TgbotToken!);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        var api = new VkApi();

        var updateReceiver = new QueuedUpdateReceiver(bot, receiverOptions);
        api.Authorize(new ApiAuthParams
        {
            AccessToken = _configuration.VkAccessToken
        });
        await foreach (var update in updateReceiver.WithCancellation(stoppingToken))
        {
            _logger.LogDebug("Update type: {update}", update.Type);
            Telegram.Bot.Types.Message? message = update.Type switch
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
            var vkMessage = new WallPostParams
            {
                OwnerId = _configuration.OwnerId,
                Message = (!string.IsNullOrEmpty(message.Text) || !string.IsNullOrEmpty(message.Caption)) ? message.Text ?? message.Caption : null,
                FromGroup = _configuration.OwnerId < 0
            };
            var result = api.Wall.PostAsync(vkMessage);
            _logger.LogDebug("{result}", await result);

        }
    }
}
