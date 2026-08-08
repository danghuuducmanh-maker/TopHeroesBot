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
    private IBrowserContext? _context;

    public async Task CreateBrowserAsync()
    {
       
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true,

                Args =
                [
                    "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-extensions",
                "--disable-background-networking",
                "--disable-sync",
                "--disable-default-apps",
                "--mute-audio",
                "--no-first-run",
                "--no-default-browser-check"
                ]
            });
        Console.WriteLine(
   $"CREATE BROWSER - Client = {GetHashCode()}");
    }
    public async Task CreatePageAsync()
    {
        if (_browser == null)
            throw new InvalidOperationException("Browser chưa được tạo.");

        _context = await _browser.NewContextAsync();

        _page = await _context.NewPageAsync();

        await _page.RouteAsync("**/*", async route =>
        {
            var type = route.Request.ResourceType;

            if (type == "image" ||
                type == "font" ||
                type == "media")
            {
                await route.AbortAsync();
            }
            else
            {
                await route.ContinueAsync();
            }
        });

        await _page.GotoAsync(
            "https://topheroes.pay-store.rivergame.net/en");
        Console.WriteLine(
    $"CREATE PAGE - Client = {GetHashCode()}, Browser = {_browser != null}");
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

        try
        {
            await textbox.FillAsync(code);
        }
        catch (TimeoutException)
        {
            await ClosePopupAsync();

            await textbox.FillAsync(code);
        }


        // Đợi API trả về
        var responseTask = _page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/v2/store/redemption/redeem") &&
            r.Request.Method == "POST");

        // Click Confirm
        try
        {
            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Confirm" })
            .ClickAsync();
        }
        catch (TimeoutException)
        {
            await ClosePopupAsync();

            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Confirm" })
            .ClickAsync();
        }
        

        var response = await responseTask;

        string json = await response.TextAsync();
        Console.WriteLine($"Gift [{code}] Response:");
        Console.WriteLine(json);
        var result = JsonSerializer.Deserialize<GiftCodeResponse>(json);

        if (result?.Code == 1)
        {
            await ClosePopupAsync();
        }
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
                try
                {
                    await button.ClickAsync();
                }
                catch (TimeoutException)
                {
                    await ClosePopupAsync();

                    await button.ClickAsync();
                }

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
                try
                {
                    await button.ClickAsync();
                }
                catch (TimeoutException)
                {
                    await ClosePopupAsync();

                    await button.ClickAsync();
                }

                return EventStatus.Success;
            }
        }

        return EventStatus.AlreadyClaimed;
    }
    public async Task CloseAsync()
    {
        if (_context != null)
            await _context.CloseAsync();

        _page = null;
        _context = null;
    }
    public async Task CloseBrowserAsync()
    {
        if (_browser != null)
            await _browser.CloseAsync();

        _playwright?.Dispose();

        _browser = null;
        _playwright = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
    private async Task ClosePopupAsync()
    {
        if (_page == null)
            return;

        try
        {
            var popup = _page.Locator(".dialog-collection");

            await popup.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 2000
            });

            // Đợi animation hoàn tất
            

            // Click ra ngoài popup
            await _page.Mouse.ClickAsync(20, 20);

            // Chờ popup biến mất
            await popup.WaitForAsync(new()
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 500
            });
        }
        catch (TimeoutException)
        {
            // Không có popup hoặc popup không đóng được
        }
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