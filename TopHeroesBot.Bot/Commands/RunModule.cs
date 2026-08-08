using Discord.Interactions;
using TopHeroesBot.Application.Enums;
using TopHeroesBot.Application.Helpers;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Bot.Commands;

[Group("run", "Chạy các chức năng")]
public class RunModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAccountService _accountService;

    public RunModule(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [SlashCommand("uid", "Chạy một tài khoản")]
    public async Task RunUid(
        string uid,
        RunAction action1,
        RunAction? action2 = null,
        RunAction? action3 = null)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            var success = await _accountService.RunAsync(
            uid,
            RunActionHelper.Build(action1, action2, action3),
            async message =>
            {
                await Context.Channel.SendMessageAsync(message);

            });

            if (!success)
            {
                await Context.Channel.SendMessageAsync("❌ Không tìm thấy UID.");
                return;
            }
            await Context.Channel.SendMessageAsync("🏁 Đã chạy xong tài khoản.");

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
        
       
    }

    [SlashCommand("all", "Chạy tất cả tài khoản")]
    public async Task RunAll(
        RunAction action1,
        RunAction? action2 = null,
        RunAction? action3 = null)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            await _accountService.RunAllAsync(
           RunActionHelper.Build(action1, action2, action3),
           async message =>
           {
               Console.WriteLine($"Send Discord ({message.Length}): {message}");

               await Context.Channel.SendMessageAsync(message);
           });

            await Context.Channel.SendMessageAsync("🏁 Đã chạy xong tất cả tài khoản.");
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
        
    }
}