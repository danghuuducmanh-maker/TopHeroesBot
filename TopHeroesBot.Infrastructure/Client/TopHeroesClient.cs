using Microsoft.Playwright;
using System.Text.Json;
using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Interfaces;

namespace TopHeroesBot.Infrastructure.Client;

public class TopHeroesClient : ITopHeroesClient
{
    private string _playerName = string.Empty;
    private string _playerServer = string.Empty;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public async Task CreatePageAsync()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            });

        _page = await _browser.NewPageAsync();

        await _page.GotoAsync("https://topheroes.pay-store.rivergame.net/en");
    }

    public async Task LoginAsync(string uid)
    {
        if (_page == null)
            throw new InvalidOperationException("Page chưa được tạo.");
        var popup = _page.Locator(".dialog-collection");

        try
        {
            await popup.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 4000
            });

            // Cho animation chạy xong
            await _page.WaitForTimeoutAsync(500);

            await _page.Mouse.ClickAsync(20, 20);

            // Đợi một chút để popup đóng
            await _page.WaitForTimeoutAsync(300);
        }
        catch (TimeoutException)
        {
            // Không có popup -> bỏ qua
        }
        // LOGIN
        await _page.Locator("#site-widget-2121094971520928")
            .GetByText("LOGIN")
            .ClickAsync();

        // UID
        await _page.GetByRole(
                AriaRole.Textbox,
                new() { Name = "Enter your UID..." })
            .FillAsync(uid);

        // CHECK
        await _page.Locator("button.check-btn")
            .ClickAsync();

        // Đợi hiện Account
        await _page
            .Locator(".user-info-item")
            .Filter(new() { HasText = "Account" })
            .WaitForAsync();
        _playerName = (await _page
    .Locator(".user-info-item")
    .Filter(new() { HasText = "Account" })
    .Locator("span")
    .InnerTextAsync())
    .Trim();

        _playerServer = (await _page
            .Locator(".user-info-item")
            .Filter(new() { HasText = "Server" })
            .Locator("span")
            .InnerTextAsync())
            .Trim();
        // CONFIRM
        await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Confirm" })
            .First
            .ClickAsync();


        try
        {
            await popup.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 4000
            });

            // Cho animation chạy xong
            await _page.WaitForTimeoutAsync(500);

            await _page.Mouse.ClickAsync(20, 20);

            // Đợi một chút để popup đóng
            await _page.WaitForTimeoutAsync(300);
        }
        catch (TimeoutException)
        {
            // Không có popup -> bỏ qua
        }
    }

    public Task<PlayerProfile> GetPlayerProfileAsync()
    {
        return Task.FromResult(new PlayerProfile
        {
            Name = _playerName,
            Server = _playerServer
        });
    }



    public async Task<DailyResult> DailyAsync()
    {
        return new DailyResult
        {
            Status = await ClaimRewardAsync("#site-widget-1035124126946440")
        };
    }

    public async Task<EventResult> GoldAsync()
    {
        const string selector = "#site-widget-6160508834407800";

        if (!await ElementExistsAsync(selector))
        {
            return new EventResult
            {
                Status = EventStatus.NotAvailable
            };
        }

        return new EventResult
        {
            Status = await ClaimEventRewardAsync(selector)
        };
    }

    public async Task<GiftResult> RedeemGiftAsync(string code)
    {
        if (_page == null)
            throw new InvalidOperationException("Chưa đăng nhập.");

        var textbox = _page.GetByRole(
            AriaRole.Textbox,
            new() { Name = "Enter the code" });

        await textbox.FillAsync(code);

        // Đợi API trả về
        var responseTask = _page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/v2/store/redemption/redeem") &&
            r.Request.Method == "POST");

        // Click Confirm
        await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Confirm" })
            .ClickAsync();

        var response = await responseTask;

        string json = await response.TextAsync();

        var result = JsonSerializer.Deserialize<GiftCodeResponse>(json);

        return new GiftResult
        {
            Code = code,
            ResultCode = result?.Code ?? -1
        };
    }
    private async Task<ClaimStatus> ClaimRewardAsync(string widgetId)
    {
        if (_page == null)
            throw new InvalidOperationException("Chưa đăng nhập.");

        var buttons = _page
            .Locator(widgetId)   // <-- dùng widgetId, không hardcode
            .Locator(".handle");

        int count = await buttons.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var button = buttons.Nth(i);

            string text = (await button.Locator("span").InnerTextAsync()).Trim();

            bool isLocked = await button.EvaluateAsync<bool>(
                "e => e.classList.contains('unsign')");

            if (text == "Sign in" && !isLocked)
            {
                await button.ClickAsync();

                return ClaimStatus.Success;
            }
        }

        return ClaimStatus.AlreadyClaimed;
    }
    private async Task<EventStatus> ClaimEventRewardAsync(string widgetId)
    {
        if (_page == null)
            throw new InvalidOperationException("Chưa đăng nhập.");

        var buttons = _page
            .Locator(widgetId)   // <-- dùng widgetId, không hardcode
            .Locator(".handle");

        int count = await buttons.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var button = buttons.Nth(i);

            string text = (await button.Locator("span").InnerTextAsync()).Trim();

            bool isLocked = await button.EvaluateAsync<bool>(
                "e => e.classList.contains('unsign')");

            if (text == "Sign in" && !isLocked)
            {
                await button.ClickAsync();

                return EventStatus.Success;
            }
        }

        return EventStatus.AlreadyClaimed;
    }
    public async Task CloseAsync()
    {
        if (_page != null)
            await _page.CloseAsync();

        if (_browser != null)
            await _browser.CloseAsync();

        _playwright?.Dispose();

        _page = null;
        _browser = null;
        _playwright = null;
    }
    
    
    public async Task<bool> ElementExistsAsync(string selector)
    {
        try
        {
            await _page.WaitForSelectorAsync(selector, new()
            {
                Timeout = 2000
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}