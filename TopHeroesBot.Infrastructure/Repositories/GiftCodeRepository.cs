using Microsoft.EntityFrameworkCore;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;
using TopHeroesBot.Infrastructure.Data;

namespace TopHeroesBot.Infrastructure.Repositories;

public class GiftCodeRepository : IGiftCodeRepository
{
    private readonly AppDbContext _context;

    public GiftCodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GiftCode>> GetAllAsync()
    {
        return await _context.GiftCodes.ToListAsync();
    }

    public async Task<GiftCode?> GetByCodeAsync(string code)
    {
        return await _context.GiftCodes
            .FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task AddAsync(GiftCode giftCode)
    {
        _context.GiftCodes.Add(giftCode);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string code)
    {
        var gift = await GetByCodeAsync(code);

        if (gift == null)
            return;

        _context.GiftCodes.Remove(gift);

        await _context.SaveChangesAsync();
    }
    public async Task RemoveAllAsync()
    {
        _context.GiftCodes.RemoveRange(_context.GiftCodes);

        await _context.SaveChangesAsync();
    }
}