using TopHeroesBot.Application.DTOs;

namespace TopHeroesBot.Application.Interfaces;

public interface ITopHeroesExecutor
{
    Task<NotifyContext?> ExecuteAsync(
        string uid,
        Func<NotifyContext, Task> action,
        Func<string, Task>? notify = null);
    Task<T> ExecuteAsync<T>(
    string uid,
    Func<NotifyContext, Task<T>> action,
    Func<string, Task>? notify = null);
}