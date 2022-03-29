using Telegram.Bot;

namespace TgToVkPoster;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private TgToVkConfiguration _configuration;
    public Worker(TgToVkConfiguration config, ILogger<Worker> logger)
    {
        _logger = logger;
        _configuration = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(_configuration.TgbotToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
