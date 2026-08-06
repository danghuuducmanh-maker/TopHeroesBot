using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IAccountRepository
{
    Task<List<Account>> GetAllAsync();

    Task<Account?> GetByUidAsync(string uid);

    Task AddAsync(Account account);

    Task UpdateAsync(Account account);

    Task DeleteAsync(string uid);
    Task<int> DeleteAllAsync();
}