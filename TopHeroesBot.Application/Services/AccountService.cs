using System.Security.Principal;
using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace TopHeroesBot.Application.Services;

public class AccountService : IAccountService
{
    private readonly IGiftCodeRepository _giftCodeRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITopHeroesClient _topHeroesClient;

    public AccountService(
    IAccountRepository accountRepository,
    IGiftCodeRepository giftCodeRepository,
    ITopHeroesClient topHeroesClient)
    {
        _accountRepository = accountRepository;
        _giftCodeRepository = giftCodeRepository;
        _topHeroesClient = topHeroesClient;
    }
    
    private async Task<PlayerProfile?> LoginAndNotify(
    string uid,
    Func<string, Task>? notify)
    {
        try
        {
            await _topHeroesClient.LoginAsync(uid);

            var profile = await _topHeroesClient.GetPlayerProfileAsync();

            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {profile.Name} ({profile.Server}): Đăng nhập thành công.");
            }

            return profile;
        }
        catch
        {
            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {uid}: Đăng nhập thất bại.");
            }

            return null;
        }
    }
    private async Task<ClaimStatus> ClaimDailyAndNotify(NotifyContext context)
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
    private async Task<EventStatus> ClaimGoldAndNotify(
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
    private async Task RedeemGiftAndNotify(NotifyContext context)
    {
        var giftCodes = await _giftCodeRepository.GetAllAsync();

        foreach (var gift in giftCodes)
        {
            var result = await _topHeroesClient.RedeemGiftAsync(gift.Code);

            switch (result.ResultCode)
            {
                case 1:

                    await context.Notify?.Invoke(
                        $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {gift.Code}: Thành công.");

                    break;

                case 80006:

                    await context.Notify?.Invoke(
                        $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {gift.Code}: Đã sử dụng.");

                    break;

                case 80004:
                case 10015:
                case 10017:

                    await context.Notify?.Invoke(
                        $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {gift.Code}: Không hợp lệ.");

                    await _giftCodeRepository.DeleteAsync(gift.Code);

                    break;

                default:

                    await context.Notify?.Invoke(
                        $"[{DateTime.Now:HH:mm:ss}] {context.Uid} {context.Profile.Name} ({context.Profile.Server}): {gift.Code}: Lỗi {result.ResultCode}");

                    break;
            }
        }
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
        var exists = await _accountRepository.GetByUidAsync(uid);

        if (exists != null)
        {
            return new AddAccountResult
            {
                Success = false,
                Message = "UID đã tồn tại."
            };
        }

        await _topHeroesClient.CreatePageAsync();

        try
        {
            var profile = await LoginAndNotify(uid, notify);
            if (profile == null)
            {
                return new AddAccountResult
                {
                    Success = false,
                    Message = "Đăng nhập thất bại."
                };
            }
            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };

            await ClaimDailyAndNotify(context);

            await ClaimGoldAndNotify(context);

            await RedeemGiftAndNotify(context);

            await SaveAccount(profile, uid);

            return new AddAccountResult
            {
                Success = true,
                Message = "Thêm tài khoản thành công."
            };
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
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
    public async Task RunDailyAsync(Func<string, Task>? notify = null)
    {
        var accounts = await _accountRepository.GetAllAsync();


        foreach (var account in accounts)
        {
            await _topHeroesClient.CreatePageAsync();

            try
            {
                var profile = await LoginAndNotify(account.Uid, notify);

                var context = new NotifyContext
                {
                    Uid = account.Uid,
                    Profile = profile,
                    Notify = notify
                };

                await ClaimDailyAndNotify(context);
                await ClaimGoldAndNotify(context);
            }
            catch (Exception)
            {
                // Bỏ qua account lỗi
            }
            finally
            {
                await _topHeroesClient.CloseAsync();
            }

        }
    }
    public async Task RunGoldAsync(Func<string, Task>? notify = null)
    {
        var accounts = await _accountRepository.GetAllAsync();


        foreach (var account in accounts)
        {
            await _topHeroesClient.CreatePageAsync();

            try
            {
                var profile = await LoginAndNotify(account.Uid, notify);

                var context = new NotifyContext
                {
                    Uid = account.Uid,
                    Profile = profile,
                    Notify = notify
                };

                await ClaimGoldAndNotify(context);

            }
            catch (Exception)
            {
                // Bỏ qua account lỗi
            }
            finally
            {
                await _topHeroesClient.CloseAsync();
            }

        }
    }
    public async Task<bool> RunOneDailyAsync(
    string uid,
    Func<string, Task>? notify = null)
    {
        var account = await _accountRepository.GetByUidAsync(uid);

        if (account == null)
            return false;
        await _topHeroesClient.CreatePageAsync();

        try
        {
            var profile = await LoginAndNotify(uid, notify);

            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };

            await ClaimDailyAndNotify(context);
            return true;
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
    }
    public async Task<bool> RunOneGoldAsync(
   string uid,
   Func<string, Task>? notify = null)
    {
        var account = await _accountRepository.GetByUidAsync(uid);

        if (account == null)
            return false;
        await _topHeroesClient.CreatePageAsync();

        try
        {
            var profile = await LoginAndNotify(uid, notify);

            var context = new NotifyContext
            {
                Uid = uid,
                Profile = profile,
                Notify = notify
            };

            await ClaimGoldAndNotify(context);
            return true;
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
    }
    public async Task<int> DeleteAllAsync()
    {
        return await _accountRepository.DeleteAllAsync();
    }
}