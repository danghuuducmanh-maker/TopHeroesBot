using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Application.Services;

public class RewardService : IRewardService
{
    private readonly ITopHeroesClient _topHeroesClient;
    private readonly IGiftCodeRepository _giftCodeRepository;

    public RewardService(
        ITopHeroesClient topHeroesClient,
        IGiftCodeRepository giftCodeRepository)
    {
        _topHeroesClient = topHeroesClient;
        _giftCodeRepository = giftCodeRepository;
    }


    public async Task<ClaimStatus> ClaimDailyAndNotify(NotifyContext context)
    {
        var daily = await _topHeroesClient.DailyAsync();

        if (context.Notify != null)
        {
            await context.Notify(
            $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): [Daily] " +
            (daily.Status == ClaimStatus.Success
                ? "Nhận thưởng thành công."
                : "Hôm nay đã nhận thưởng"));
        }


        return daily.Status;
    }
    public async Task<EventStatus> ClaimGoldAndNotify(
    NotifyContext context)
    {
        var gold = await _topHeroesClient.GoldAsync();

        if (gold.Status == EventStatus.NotAvailable)
        {
            return gold.Status;
        }

        string message = gold.Status switch
        {
            EventStatus.Success => "Nhận thưởng thành công.",
            EventStatus.AlreadyClaimed => "Hôm nay đã nhận thưởng.",
            _ => "Lỗi."
        };

        if (context.Notify != null)
        {
            await context.Notify(
                $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): [Gold] {message}");
        }

        return gold.Status;
    }
    public async Task RedeemGiftAndNotify(
    NotifyContext context,
    string code)
    {
        var result = await _topHeroesClient.RedeemGiftAsync(code);

        switch (result.ResultCode)
        {
            case 1:

                await context.Notify?.Invoke(
                    $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {code}: Thành công.");

                break;

            case 80006:

                await context.Notify?.Invoke(
                    $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {code}: Đã sử dụng.");

                break;

            case 80004:
            case 10015:
            case 10017:

                await context.Notify?.Invoke(
                    $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {code}: Không hợp lệ.");

                await _giftCodeRepository.DeleteAsync(code);

                break;

            default:

                await context.Notify?.Invoke(
                    $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {code}: Lỗi {result.ResultCode}");

                break;
        }
    }

    public async Task RedeemAllGiftAndNotify(
    NotifyContext context)
    {
        var giftCodes = await _giftCodeRepository.GetAllAsync();

        foreach (var gift in giftCodes)
        {
            await RedeemGiftAndNotify(
                context,
                gift.Code);
        }
    }
}