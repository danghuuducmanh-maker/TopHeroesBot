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

    public async Task<string> AddAsync(string code)
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

        var message = $"🎁 **GiftCode:** `{code}`\n\n";

        foreach (var account in accounts)
        {
            await _topHeroesClient.CreatePageAsync();

            try
            {
                await _topHeroesClient.LoginAsync(account.Uid);

                var result = await _topHeroesClient.RedeemGiftAsync(code);

                switch (result.ResultCode)
                {
                    case 1:
                        message += $"✅ **{account.Name}** (`{account.Uid}`)\n";
                        break;

                    case 80006:
                        message += $"⏩ **{account.Name}** (`{account.Uid}`) - Đã nhận\n";
                        break;

                    case 80004:
                    case 10015:
                    case 10017:

                        await _giftRepository.DeleteAsync(code);

                        message += $"❌ GiftCode không hợp lệ.\n";

                        return message;

                    default:

                        message += $"⚠️ **{account.Name}** (`{account.Uid}`) - Lỗi {result.ResultCode}\n";
                        break;
                }
            }
            catch (Exception ex)
            {
                message += $"❌ **{account.Name}** (`{account.Uid}`) - {ex.Message}\n";
            }
            finally
            {
                await _topHeroesClient.CloseAsync();
            }
        }

        return message;
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