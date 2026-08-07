using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Enums;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Services;

public class AccountService : IAccountService
{
    private readonly IRewardService _rewardService;
    private readonly IAccountRepository _accountRepository;
    private readonly ITopHeroesExecutor _executor;
    private readonly IGiftCodeRepository _giftCodeRepository;

    public AccountService(
     IAccountRepository accountRepository,
     IGiftCodeRepository giftRepository,
     ITopHeroesExecutor executor,
     IRewardService rewardService)
    {
        _accountRepository = accountRepository;
        _giftCodeRepository = giftRepository;
        _executor = executor;
        _rewardService = rewardService;
    }

    private async Task SaveAccount(
    PlayerProfile profile,
    string uid)
    {
        var account = new Account
        {
            Uid = uid,
            Name = profile.Name,
            Server = profile.Server
        };

        await _accountRepository.AddAsync(account);
    }
    public async Task<AddAccountResult> AddAccountAsync(
    string uid,
    Func<string, Task>? notify = null)
    {
        var context = await _executor.ExecuteAsync(
            uid,
            async ctx =>
            {
                await RunActions(
                        ctx,
                        RunAction.Daily,
                        RunAction.Gold);

                await SaveAccount(ctx.Profile, uid);
            },
    notify);

        if (context == null)
        {
            return new AddAccountResult
            {
                Success = false,
                Message = "Đăng nhập thất bại."
            };
        }

        return new AddAccountResult
        {
            Success = true,
            Message = "Thêm tài khoản thành công."
        };
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _accountRepository.GetAllAsync();
    }
    public async Task<bool> DeleteAsync(string uid)
    {
        var account = await _accountRepository.GetByUidAsync(uid);

        if (account == null)
            return false;

        await _accountRepository.DeleteAsync(uid);

        return true;
    }

    public async Task<int> DeleteAllAsync()
    {
        return await _accountRepository.DeleteAllAsync();
    }
   
    public async Task<bool> RunAsync(
     string uid,
     RunAction[] actions,
     Func<string, Task>? notify = null)
    {
        var account = await _accountRepository.GetByUidAsync(uid);

        if (account == null)
            return false;

        await _executor.ExecuteAsync(
            uid,
            ctx => RunActions(ctx, actions),
            notify);

        return true;
    }

    public async Task RunAllAsync(
        RunAction[] actions,
        Func<string, Task>? notify = null)
    {
        var accounts = await _accountRepository.GetAllAsync();

        foreach (var account in accounts)
        {
            await RunAsync(
                account.Uid,
                actions,
                notify);
        }
    }
   private async Task RunActions(
   NotifyContext context,
   params RunAction[] actions)
    {
        if (actions.Contains(RunAction.All))
        {
            actions =
            [
                RunAction.Daily,
                RunAction.Gold,
                RunAction.Gift
            ];
        }
        foreach (var action in actions)
        {
            switch (action)
            {
                case RunAction.Daily:
                    await _rewardService.ClaimDailyAndNotify(context);
                    break;

                case RunAction.Gold:
                    await _rewardService.ClaimGoldAndNotify(context);
                    break;

                case RunAction.Gift:
                    await _rewardService.RedeemAllGiftAndNotify(context);
                    break;
            }
        }
    }
}