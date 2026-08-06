using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IDailyHistoryRepository
{
    Task<bool> ExistsAsync(string uid, DateOnly date);

    Task AddAsync(DailyHistory history);

    Task DeleteOlderThanAsync(DateOnly date);
}