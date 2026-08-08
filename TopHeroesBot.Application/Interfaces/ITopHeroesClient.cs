using TopHeroesBot.Application.DTOs;

namespace TopHeroesBot.Application.Interfaces;

public interface ITopHeroesClient
{
    Task CreateBrowserAsync();
    Task CreatePageAsync();

    Task LoginAsync(string uid);

    Task<PlayerProfile> GetPlayerProfileAsync();

    Task<DailyResult> DailyAsync();

    Task<GiftResult> RedeemGiftAsync(string code);

    Task<EventResult> GoldAsync();

    Task CloseAsync();

    Task CloseBrowserAsync();
}