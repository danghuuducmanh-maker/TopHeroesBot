using TopHeroesBot.Application.Enums;
using TopHeroesBot.Application.Interfaces;
using Discord;
using Discord.WebSocket;

namespace TopHeroesBot.Bot.Services;


public class SchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordSocketClient _client;
    public SchedulerService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
    {
        _scopeFactory = scopeFactory;
        _client = client;
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

            ulong channelId = 1425855503145369715; // ID channel log của bạn

            Func<string, Task> notify = async message =>
            {
                var channel =
                    _client.GetChannel(channelId) as IMessageChannel;
                Console.WriteLine($"ConnectionState = {_client.ConnectionState}");
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
                else
                    Console.WriteLine(channel == null ? "Channel null" : channel.Name);
            };

            Console.WriteLine("Scheduler bắt đầu chạy");

            await notify("🚀 Scheduler bắt đầu.");

            Console.WriteLine("Đã gửi tin nhắn bắt đầu");

            await scheduleService.RunNowAsync(
                notify,
                RunAction.Daily,
                RunAction.Gold);

            await notify("✅ Scheduler hoàn thành.");
        }
    }
}
