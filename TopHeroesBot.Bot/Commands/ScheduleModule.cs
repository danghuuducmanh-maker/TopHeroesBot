using Discord;
using Discord.Interactions;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Bot.Commands;

[Group("schedule", "Quản lý lịch chạy")]
public class ScheduleModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IScheduleService _scheduleService;

    public ScheduleModule(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [SlashCommand("status", "Xem lịch chạy")]
    public async Task Status()
    {
        var setting = await _scheduleService.GetAsync();

        var embed = new EmbedBuilder()
            .WithTitle("⏰ Scheduler")
            .WithColor(Color.Blue)
            .AddField("Enabled", setting.Enabled ? "✅ Bật" : "❌ Tắt", true)
            .AddField("Time", $"{setting.Hour:D2}:{setting.Minute:D2}", true)
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("set", "Đổi giờ chạy")]
    public async Task Set(
        int hour,
        int minute)
    {
        if (hour < 0 || hour > 23)
        {
            await RespondAsync("❌ Hour phải từ 0-23.", ephemeral: true);
            return;
        }

        if (minute < 0 || minute > 59)
        {
            await RespondAsync("❌ Minute phải từ 0-59.", ephemeral: true);
            return;
        }

        await _scheduleService.SetAsync(hour, minute);

        var embed = new EmbedBuilder()
            .WithTitle("✅ Đã cập nhật lịch")
            .WithColor(Color.Green)
            .AddField("Thời gian", $"{hour:D2}:{minute:D2}")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("enable", "Bật Scheduler")]
    public async Task Enable()
    {
        await _scheduleService.EnableAsync();

        await RespondAsync("✅ Scheduler đã được bật.");
    }

    [SlashCommand("disable", "Tắt Scheduler")]
    public async Task Disable()
    {
        await _scheduleService.DisableAsync();

        await RespondAsync("🛑 Scheduler đã được tắt.");
    }
}