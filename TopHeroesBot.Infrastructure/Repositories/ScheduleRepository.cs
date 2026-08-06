using Microsoft.EntityFrameworkCore;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;
using TopHeroesBot.Infrastructure.Data;

namespace TopHeroesBot.Infrastructure.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDbContext _context;

    public ScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleSetting?> GetAsync()
    {
        return await _context.ScheduleSettings.FirstOrDefaultAsync();
    }

    public async Task SaveAsync(ScheduleSetting setting)
    {
        if (setting.Id == 0)
        {
            _context.ScheduleSettings.Add(setting);
        }
        else
        {
            _context.ScheduleSettings.Update(setting);
        }

        await _context.SaveChangesAsync();
    }
}