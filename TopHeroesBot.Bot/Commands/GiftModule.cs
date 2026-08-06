using Discord;
using Discord.Interactions;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Bot.Commands;

[Group("gift", "Quản lý GiftCode")]
public class GiftModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGiftCodeService _giftService;

    public GiftModule(IGiftCodeService giftService)
    {
        _giftService = giftService;
    }

    [SlashCommand("add", "Thêm GiftCode")]
    public async Task Add(string code)
    {
        await DeferAsync();

        var message = await _giftService.AddAsync(code);

        var embed = new EmbedBuilder()
            .WithTitle("🎁 Kết quả thêm GiftCode")
            .WithColor(Color.Green)
            .WithDescription(message)
            .Build();

        await FollowupAsync(embed: embed);
    }

    [SlashCommand("list", "Danh sách GiftCode")]
    public async Task List()
    {
        var gifts = await _giftService.GetAllAsync();

        if (gifts.Count == 0)
        {
            await RespondAsync("❌ Chưa có GiftCode.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("🎁 Danh sách GiftCode")
            .WithColor(Color.Blue);

        foreach (var gift in gifts)
        {
            embed.AddField("GiftCode", gift.Code, false);
        }

        embed.WithFooter($"Tổng: {gifts.Count} GiftCode");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("remove", "Xóa GiftCode")]
    public async Task Remove(string code)
    {
        var success = await _giftService.DeleteAsync(code);

        if (!success)
        {
            await RespondAsync(
                "❌ Không tìm thấy GiftCode.",
                ephemeral: true);

            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("🗑️ Đã xóa GiftCode")
            .WithColor(Color.Red)
            .AddField("GiftCode", code.ToUpper(), false)
            .Build();

        await RespondAsync(embed: embed);
    }
}