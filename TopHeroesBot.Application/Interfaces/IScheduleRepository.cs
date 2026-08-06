using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IScheduleRepository
{
    Task<ScheduleSetting?> GetAsync();

    Task SaveAsync(ScheduleSetting setting);
}