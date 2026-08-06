namespace TopHeroesBot.Domain.Entities;

public class DailyHistory
{
    public string Uid { get; set; } = string.Empty;

    public DateOnly Date { get; set; }
}