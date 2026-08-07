using System.Security.Principal;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Services;

public class GiftCodeService : IGiftCodeService
{
    private readonly IGiftCodeRepository _giftRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITopHeroesClient _topHeroesClient;

    public GiftCodeService(IGiftCodeRepository giftRepository)
    {
        _giftRepository = giftRepository;
    }
    private async Task RedeemGiftForAccountAsync(
    Account account,
    string code,
    Func<string, Task>? notify)
    {
        await _topHeroesClient.CreatePageAsync();

        try
        {
            await _topHeroesClient.LoginAsync(account.Uid);

            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {account.Name} ({account.Server}): Đăng nhập thành công.");
            }

            var result = await _topHeroesClient.RedeemGiftAsync(code);

            switch (result.ResultCode)
            {
                case 1:

                    if (notify != null)
                    {
                        await notify(
                            $"[{DateTime.Now:HH:mm:ss}] {account.Name} ({account.Server}): {code}: Thành công.");
                    }

                    break;

                case 80006:

                    if (notify != null)
                    {
                        await notify(
                            $"[{DateTime.Now:HH:mm:ss}] {account.Name} ({account.Server}): {code}: Đã nhận.");
                    }

                    break;

                case 80004:
                case 10015:
                case 10017:

                    await _giftRepository.DeleteAsync(code);

                    if (notify != null)
                    {
                        await notify(
                            $"[{DateTime.Now:HH:mm:ss}] {code}: GiftCode không hợp lệ. Đã xóa khỏi danh sách.");
                    }

                    break;

                default:

                    if (notify != null)
                    {
                        await notify(
                            $"[{DateTime.Now:HH:mm:ss}] {account.Name} ({account.Server}): {code}: Lỗi {result.ResultCode}");
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            if (notify != null)
            {
                await notify(
                    $"[{DateTime.Now:HH:mm:ss}] {account.Name} ({account.Server}): {ex.Message}");
            }
        }
        finally
        {
            await _topHeroesClient.CloseAsync();
        }
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
            await RedeemGiftForAccountAsync(
                account,
                code,
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