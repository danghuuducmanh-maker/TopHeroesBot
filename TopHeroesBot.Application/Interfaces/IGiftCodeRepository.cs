using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IGiftCodeRepository
{
    Task<List<GiftCode>> GetAllAsync();

    Task<GiftCode?> GetByCodeAsync(string code);

    Task AddAsync(GiftCode giftCode);

    Task DeleteAsync(string code);
}