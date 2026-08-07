using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Bot.Services;

public class SchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SchedulerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeZoneInfo vietnamTimeZone;

        try
        {
            // Windows
            vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            // Linux
            vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var scheduleService =
                scope.ServiceProvider.GetRequiredService<IScheduleService>();

            var setting = await scheduleService.GetAsync();

            if (!setting.Enabled)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            // Luôn dùng giờ Việt Nam (GMT+7)
            var now = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                vietnamTimeZone);

            var nextRun = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                setting.Hour,
                setting.Minute,
                0);

            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            Console.WriteLine($"Now     : {now}");
            Console.WriteLine($"NextRun : {nextRun}");
            Console.WriteLine($"Delay   : {delay}");

            await Task.Delay(delay, stoppingToken);

            await scheduleService.RunNowAsync();
        }
    }
}
