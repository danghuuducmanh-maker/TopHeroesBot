using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IScheduleService
{
    Task<ScheduleSetting> GetAsync();

    Task SetAsync(int hour, int minute);

    Task EnableAsync();

    Task DisableAsync();

    Task RunNowAsync();
}