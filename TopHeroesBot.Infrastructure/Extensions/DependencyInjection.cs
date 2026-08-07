using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Application.Services;
using TopHeroesBot.Infrastructure.Data;
using TopHeroesBot.Infrastructure.Client;
using TopHeroesBot.Infrastructure.Repositories;

namespace TopHeroesBot.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IGiftCodeRepository, GiftCodeRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        
        
        // Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IGiftCodeService, GiftCodeService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ITopHeroesExecutor, TopHeroesExecutor>();
        services.AddScoped<IRewardService, RewardService>();
        // Playwright Client
        services.AddScoped<ITopHeroesClient, TopHeroesClient>();
        return services;
    }
}