using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IGiftCodeService
{
    Task<string> AddAsync(
    string code,
    Func<string, Task>? notify = null);

    Task<List<GiftCode>> GetAllAsync();

    Task<bool> DeleteAsync(string code);
}