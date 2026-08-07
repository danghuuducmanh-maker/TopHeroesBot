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
        await DeferAsync(ephemeral: true);

        try
        {
            var result = await _giftService.AddAsync(
            code,
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });


            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = "✅ Hoàn thành.";
            });
        }
        catch (Exception ex)
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"❌ {ex.Message}";
            });
        }
        await Context.Channel.SendMessageAsync("🏁 Đã chạy xong tất cả tài khoản.");

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