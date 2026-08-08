using Microsoft.EntityFrameworkCore;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;
using TopHeroesBot.Infrastructure.Data;

namespace TopHeroesBot.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{

    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account)
    {
        var nextOrder = await _context.Accounts.AnyAsync()
    ? await _context.Accounts.MaxAsync(x => x.Order) + 1
    : 1;

        account.Order = nextOrder;
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(string uid)
    {
        var account = await GetByUidAsync(uid);

        if (account == null)
            return;

        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts
    .OrderBy(x => x.Order)
    .ToListAsync();
    }

    public async Task<Account?> GetByUidAsync(string uid)
    {
        return await _context.Accounts.FindAsync(uid);
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }
    public async Task<int> DeleteAllAsync()
    {
        var accounts = await _context.Accounts.ToListAsync();

        int count = accounts.Count;

        _context.Accounts.RemoveRange(accounts);

        await _context.SaveChangesAsync();

        return count;
    }
}