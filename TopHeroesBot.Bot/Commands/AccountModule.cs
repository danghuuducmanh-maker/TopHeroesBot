using Discord;
using Discord.Interactions;
using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Bot.Commands;

[Group("account", "Quản lý tài khoản")]
public class AccountModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAccountService _accountService;

    public AccountModule(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [SlashCommand("add", "Thêm tài khoản")]
    //public async Task Add(string uid)
    //{
    //    Console.WriteLine("ENTER");

    //    await RespondAsync("OK");
    //}
    public async Task Add(string uid)
    {
        await DeferAsync(ephemeral: true);
        await Context.Channel.SendMessageAsync("Bắt đầu chạy:");
        var result = await _accountService.AddAccountAsync(
            uid,
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });

        if (!result.Success)
        {
            await FollowupAsync($"❌ {result.Message}");
            return;
        }

        await Context.Channel.SendMessageAsync(" Thêm tài khoản thành công.");
        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });

    }

    //[SlashCommand("list", "Danh sách")]
    //public async Task List()
    //{
    //    Console.WriteLine("LIST");
    //    await RespondAsync("OK");
    //}
    [SlashCommand("list", "Danh sách tài khoản")]
    public async Task List()
    {
        var accounts = await _accountService.GetAllAsync();

        if (accounts.Count == 0)
        {
            await RespondAsync("Chưa có tài khoản.");
            return;
        }

        var text = string.Join(
            Environment.NewLine,
            accounts.Select((x, i) =>
                $"{i + 1}. {x.Uid}: {x.Name} ({x.Server}) "));

        await RespondAsync($"""
            ## 📋 Danh sách tài khoảnn
            {text}
            """);
    }

    [SlashCommand("remove", "Xóa tài khoản")]
    public async Task Remove(string uid)
    {
        bool deleted = await _accountService.DeleteAsync(uid);

        if (!deleted)
        {
            await RespondAsync("❌ Không tìm thấy UID.");
            return;
        }

        await RespondAsync($"🗑️ Đã xóa UID **{uid}**");
    }
    [SlashCommand("runall", "Chạy Daily tất cả tài khoản")]
    public async Task Run()
    {
        await Context.Channel.SendMessageAsync("Bắt đầu chạy:");
        await DeferAsync(ephemeral: true);
        await _accountService.RunDailyAsync(
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });

        await Context.Channel.SendMessageAsync("🏁 Đã chạy xong tất cả tài khoản.");
        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("run", "Chạy 1 tài khoản")]
    public async Task RunOne(string uid)
    {
        await Context.Channel.SendMessageAsync("Bắt đầu chạy:");
        await DeferAsync(ephemeral: true);

        bool found = await _accountService.RunOneDailyAsync(
            uid,
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });

        if (!found)
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = "❌ Không tìm thấy UID.";
            });

            return;
        }

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("goldall", "Chạy Gold tất cả tài khoản")]
    public async Task RunGold()
    {
        await Context.Channel.SendMessageAsync("Bắt đầu chạy:");
        await DeferAsync(ephemeral: true);
        await _accountService.RunGoldAsync(
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });

        await Context.Channel.SendMessageAsync("🏁 Đã chạy xong tất cả tài khoản.");
        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("gold", "Chạy Gold 1 tài khoản")]
    public async Task RunOneGold(string uid)
    {
        await Context.Channel.SendMessageAsync("Bắt đầu chạy:");
        await DeferAsync(ephemeral: true);

        bool found = await _accountService.RunOneGoldAsync(
            uid,
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);
            });

        if (!found)
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = "❌ Không tìm thấy UID.";
            });

            return;
        }

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("addimport", "Import tài khoản từ file txt")]
    public async Task AddImport(IAttachment file)
    {
        int success = 0;
        int failed = 0;
        await DeferAsync(ephemeral: true);

        if (!file.Filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = "❌ Chỉ hỗ trợ file .txt";
            });

            return;
        }

        await Context.Channel.SendMessageAsync("🚀 Bắt đầu import tài khoản...");

        using var httpClient = new HttpClient();

        var content = await httpClient.GetStringAsync(file.Url);

        var uidList = content
            .Split(new[] { '\r', '\n', ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !x.StartsWith("#"))
            .Distinct();

        foreach (var uid in uidList)
        {
            var result = await _accountService.AddAccountAsync(
                uid,
                async message =>
                {
                    await Context.Channel.SendMessageAsync(message);
                });

            if (!result.Success)
            {
                await Context.Channel.SendMessageAsync(
                    $"❌ {uid}: {result.Message}");
                failed++;
            }
            else
                success++;
        }

        await Context.Channel.SendMessageAsync(
    $"🏁 Import hoàn thành.\n" +
    $"✅ Thành công: {success}\n" +
    $"❌ Thất bại: {failed}");

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("addlist", "Thêm nhiều tài khoản")]
    public async Task AddList(string uids)
    {
        await DeferAsync(ephemeral: true);

        await Context.Channel.SendMessageAsync("🚀 Bắt đầu thêm tài khoản...");

        var uidList = uids
            .Split(new[] { '\r', '\n', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Distinct();

        foreach (var uid in uidList)
        {
            var result = await _accountService.AddAccountAsync(
                uid,
                async message =>
                {
                    await Context.Channel.SendMessageAsync(message);
                });

            if (!result.Success)
            {
                await Context.Channel.SendMessageAsync(
                    $"❌ {uid}: {result.Message}");
            }
        }

        await Context.Channel.SendMessageAsync("🏁 Đã thêm xong tất cả tài khoản.");

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
    [SlashCommand("deleteall", "Xóa toàn bộ tài khoản")]
    public async Task DeleteAll(
    [Summary("confirm", "Xác nhận xóa toàn bộ")] bool confirm)
    {
        if (!confirm)
        {
            await RespondAsync(
                "❌ Bạn phải chọn **confirm = true** để xóa toàn bộ.",
                ephemeral: true);

            return;
        }

        await DeferAsync(ephemeral: true);

        int count = await _accountService.DeleteAllAsync();

        await Context.Channel.SendMessageAsync(
            $"🗑️ Đã xóa {count} tài khoản.");

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "✅ Hoàn thành";
        });
    }
}