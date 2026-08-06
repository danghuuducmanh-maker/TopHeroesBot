namespace TopHeroesBot.Domain.Entities;

public class ScheduleSetting
{
    public int Id { get; set; }

    public int Hour { get; set; }

    public int Minute { get; set; }

    public bool Enabled { get; set; } = true;
}