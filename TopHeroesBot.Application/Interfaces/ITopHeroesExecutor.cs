using TopHeroesBot.Application.DTOs;

namespace TopHeroesBot.Application.Interfaces;

public interface ITopHeroesExecutor
{
    Task<NotifyContext?> ExecuteAsync(
    string uid,
    Func<NotifyContext, Task> action,
    Func<string, Task>? notify = null);
    Task<GiftRedeemStatus> ExecuteAsync(
    string uid,
    Func<NotifyContext, Task<GiftRedeemStatus>> action,
    Func<string, Task>? notify = null);
}