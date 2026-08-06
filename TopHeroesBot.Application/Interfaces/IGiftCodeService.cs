using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IGiftCodeService
{
    Task<string> AddAsync(string code);

    Task<List<GiftCode>> GetAllAsync();

    Task<bool> DeleteAsync(string code);
}