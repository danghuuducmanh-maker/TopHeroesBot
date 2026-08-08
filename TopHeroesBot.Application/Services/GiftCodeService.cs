using System;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Services;

public class GiftCodeService : IGiftCodeService
{
    private readonly IRewardService _rewardService;
    private readonly ITopHeroesExecutor _executor;
    private readonly IGiftCodeRepository _giftRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITopHeroesClient _topHeroesClient;
    public GiftCodeService(
     IGiftCodeRepository giftRepository,
     IAccountRepository accountRepository,
     ITopHeroesExecutor executor,
     IRewardService rewardService,
     ITopHeroesClient topHeroesClient)
    {
        _giftRepository = giftRepository;
        _accountRepository = accountRepository;
        _executor = executor;
        _rewardService = rewardService;
        _topHeroesClient = topHeroesClient;
    }
    public async Task<string> AddAsync(
     string code,
     Func<string, Task>? notify = null)
    {
        code = code.Trim();

        var exists = await _giftRepository.GetByCodeAsync(code);

        if (exists != null)
            return "❌ GiftCode đã tồn tại.";

        await _giftRepository.AddAsync(new GiftCode
        {
            Code = code
        });

        var accounts = await _accountRepository.GetAllAsync();

        await _topHeroesClient.CreateBrowserAsync();

        try
        {
            int consecutive10017 = 0;
            int retryIndex = -1;

            for (int i = 0; i < accounts.Count; i++)
            {
                var account = accounts[i];

                var status = await _executor.ExecuteAsync(
                    account.Uid,
                    ctx => _rewardService.RedeemGiftAndNotify(ctx, code),
                    notify);

                if (status == GiftRedeemStatus.TooManyRequests)
                {
                    if (consecutive10017 == 0)
                        retryIndex = i;

                    consecutive10017++;

                    if (consecutive10017 >= 5)
                    {
                        await notify?.Invoke(
                            "⏳ 5 tài khoản liên tiếp bị giới hạn IP (10017). Chờ 15 phút...");

                        await Task.Delay(TimeSpan.FromMinutes(15));

                        i = retryIndex - 1;

                        consecutive10017 = 0;
                        retryIndex = -1;
                    }
                }
                else
                {
                    consecutive10017 = 0;
                    retryIndex = -1;
                }
            }
        }
        finally
        {
            await _topHeroesClient.CloseBrowserAsync();
        }

        return $"🎁 GiftCode `{code}` đã xử lý xong.";
    }
    public async Task<List<GiftCode>> GetAllAsync()
    {
        return await _giftRepository.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var gift = await _giftRepository.GetByCodeAsync(code);

        if (gift == null)
            return false;

        await _giftRepository.DeleteAsync(code);

        return true;
    }
    public async Task<string> RemoveAllAsync()
    {
        await _giftRepository.RemoveAllAsync();

        return "🗑️ Đã xóa toàn bộ GiftCode.";
    }
}