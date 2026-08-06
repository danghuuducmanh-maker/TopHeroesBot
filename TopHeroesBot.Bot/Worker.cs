namespace TopHeroesBot.Bot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TopHeroesBot started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO:
            // - Kiểm tra lịch chạy Daily
            // - Gọi DailyService khi đến giờ

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}