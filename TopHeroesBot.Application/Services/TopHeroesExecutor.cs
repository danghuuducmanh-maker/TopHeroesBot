using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Application.Services;

public class TopHeroesExecutor : ITopHeroesExecutor
{
    private readonly ITopHeroesClient _topHeroesClient;
    public TopHeroesExecutor(ITopHeroesClient topHeroesClient)
    {
        _topHeroesClient = topHeroesClient;
    }
    public async Task<NotifyContext?> ExecuteAsync(
    string uid,
    Func<NotifyContext, Task> action,
    Func<string, Task>? notify = null)
    {
        await _topHeroesClient.CreatePageAsync();

        try
        {
            var profile = await LoginAndNotify(uid, notify);

            if (profile == null)
                return null;
            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };
            await action(context);

            return context;
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
    }
    private async Task<PlayerProfile?> LoginAndNotify(
    string uid,
    Func<string, Task>? notify)
    {
        try
        {
            await _topHeroesClient.LoginAsync(uid);

            var profile = await _topHeroesClient.GetPlayerProfileAsync();

            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {profile.Name} ({profile.Server}): Đăng nhập thành công.");
            }

            return profile;
        }
        catch
        {
            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {uid}: Đăng nhập thất bại.");
            }

            return null;
        }
    }
    public async Task<T> ExecuteAsync<T>(
    string uid,
    Func<NotifyContext, Task<T>> action,
    Func<string, Task>? notify = null)
    {
        await _topHeroesClient.CreatePageAsync();

        try
        {
            var profile = await LoginAndNotify(uid, notify);

            if (profile == null)
                return default!;
            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };
            await action(context);

            return await action(context);
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
    }
}