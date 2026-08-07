namespace TopHeroesBot.Application.DTOs;

public record NotifyContext
{
    public string Uid { get; init; } = "";

    public PlayerProfile Profile { get; init; } = null!;

    public Func<string, Task>? Notify { get; init; }
}