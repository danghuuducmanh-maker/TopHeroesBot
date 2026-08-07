using TopHeroesBot.Application.DTOs;
using static TopHeroesBot.Application.Services.AccountService;

namespace TopHeroesBot.Application.Interfaces;

public interface ITopHeroesExecutor
{
    Task<NotifyContext?> CreateContextAsync(
        string uid,
        Func<string, Task>? notify = null);

    Task ExecuteAsync(
        string uid,
        Func<NotifyContext, Task> action,
        Func<string, Task>? notify = null);
}