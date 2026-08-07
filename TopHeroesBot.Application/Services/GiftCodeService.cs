using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Services;

public class GiftCodeService : IGiftCodeService
{
    private readonly IRewardService _rewardService;
    private readonly ITopHeroesExecutor _executor;
    private readonly IGiftCodeRepository _giftRepository;
    private readonly IAccountRepository _accountRepository;

    public GiftCodeService(
     IGiftCodeRepository giftRepository,
     IAccountRepository accountRepository,
     ITopHeroesExecutor executor,
     IRewardService rewardService)
    {
        _giftRepository = giftRepository;
        _accountRepository = accountRepository;
        _executor = executor;
        _rewardService = rewardService;
    }
    public async Task<string> AddAsync(
    string code,
    Func<string, Task>? notify = null)
    {
        code = code.Trim().ToUpper();

        var exists = await _giftRepository.GetByCodeAsync(code);

        if (exists != null)
            return "❌ GiftCode đã tồn tại.";

        await _giftRepository.AddAsync(new GiftCode
        {
            Code = code
        });

        var accounts = await _accountRepository.GetAllAsync();

        foreach (var account in accounts)
        {
            var context = await _executor.ExecuteAsync(
    account.Uid,
    ctx => _rewardService.RedeemGiftAndNotify(ctx, code),
    notify);
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
}