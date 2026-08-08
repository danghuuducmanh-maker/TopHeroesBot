namespace TopHeroesBot.Domain.Entities;

public class Account
{
    public string Uid { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Server { get; set; } = string.Empty;

    public int Order { get; set; }
}