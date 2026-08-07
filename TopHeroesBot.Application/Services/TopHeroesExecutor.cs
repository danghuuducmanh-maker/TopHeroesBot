using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;
using static TopHeroesBot.Application.Services.AccountService;

namespace TopHeroesBot.Application.Services;

public class TopHeroesExecutor : ITopHeroesExecutor
{
    private readonly ITopHeroesClient _topHeroesClient;

    public TopHeroesExecutor(
        ITopHeroesClient topHeroesClient)
    {
        _topHeroesClient = topHeroesClient;
    }

    public async Task ExecuteAsync(
        string uid,
        Func<NotifyContext, Task> action,
        Func<string, Task>? notify = null)
    {

    }
}