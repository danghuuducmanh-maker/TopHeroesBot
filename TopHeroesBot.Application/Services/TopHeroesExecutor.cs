using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;
using static TopHeroesBot.Application.Services.AccountService;

namespace TopHeroesBot.Application.Services;

public class TopHeroesExecutor : ITopHeroesExecutor
{
    private readonly ITopHeroesClient _topHeroesClient;

    public async Task<NotifyContext?> CreateContextAsync(
    string uid,
    Func<string, Task>? notify = null)
    {
        await _topHeroesClient.CreatePageAsync();

        try
        {
            await _topHeroesClient.LoginAsync(uid);

            var profile = await _topHeroesClient.GetPlayerProfileAsync();

            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };

            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {profile.Name} ({profile.Server}): Đăng nhập thành công.");
            }

            return context;
        }
        catch
        {
            await _topHeroesClient.CloseAsync();
            return null;
        }
    }

    public Task ExecuteAsync(
        string uid,
        Func<NotifyContext, Task> action,
        Func<string, Task>? notify = null)
    {
        throw new NotImplementedException();
    }
}