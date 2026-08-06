using Microsoft.EntityFrameworkCore;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<GiftCode> GiftCodes => Set<GiftCode>();

    public DbSet<DailyHistory> DailyHistories => Set<DailyHistory>();

    public DbSet<ScheduleSetting> ScheduleSettings => Set<ScheduleSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .HasKey(x => x.Uid);

        modelBuilder.Entity<GiftCode>()
            .HasKey(x => x.Code);

        modelBuilder.Entity<DailyHistory>()
            .HasKey(x => new { x.Uid, x.Date });

        modelBuilder.Entity<ScheduleSetting>()
            .HasKey(x => x.Id);
    }
}