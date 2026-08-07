using TopHeroesBot.Application.DTOs;

namespace TopHeroesBot.Application.Interfaces;

public interface IRewardService
{
    Task<ClaimStatus> ClaimDailyAndNotify(NotifyContext context);

    Task<EventStatus> ClaimGoldAndNotify(NotifyContext context);

    Task RedeemGiftAndNotify(
        NotifyContext context,
        string code);
    Task RedeemAllGiftAndNotify(NotifyContext context);
}