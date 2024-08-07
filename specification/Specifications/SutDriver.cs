using Microsoft.Playwright;

namespace Specifications;

public sealed class SutDriver
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private readonly string _baseAddress;
    private readonly Func<IPlaywright, IBrowserType> _selectBrowserType;

    public SutDriver(Func<IPlaywright, IBrowserType> selectBrowserType)
    {
        _baseAddress = GetBaseAddress();
        _selectBrowserType = selectBrowserType;
    }

    public async Task Initialize()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _selectBrowserType(_playwright)
            .LaunchAsync(new() { Headless = true });

        _page = await _browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = _baseAddress,
        });
        await _page.GotoAsync("/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        // Don't know how to account for startup Blazor SignalR connection (Is not enough: 'WaitUntilState.NetworkIdle')
        await Task.Delay(500);
        await Assertions.Expect(_page).ToHaveTitleAsync("My Weird Stuff");
    }

    private static string GetBaseAddress()
    {
        var baseAddress = Environment.GetEnvironmentVariable("SPECIFICATIONS_BASEADDRESS")
            ?? throw new InvalidOperationException("SPECIFICATIONS_BASEADDRESS not found");
        baseAddress = AddSchemaIfNotPresent(baseAddress);
        return baseAddress;
    }

    private static string AddSchemaIfNotPresent(string baseAddress)
    {
        var isContainsSchema = baseAddress.StartsWith("http://")
            || baseAddress.StartsWith("https://");
        if (!isContainsSchema)
        {
            baseAddress = $"https://{baseAddress}";
        }

        return baseAddress;
    }

    internal async Task AddComic(string url)
    {
        await _page.GetByText("Add comic").ClickAsync();
        
        var input = _page.GetByLabel("Add comic");
        await input.FillAsync(url);

        var submit = _page.GetByRole(AriaRole.Button, new() { Name = "Save" });
        await submit.ClickAsync();
    }

    internal async Task ThenSavedUrlIs(string expectedUrl)
    {
        var actualUrl = _page.GetByLabel("Saved url");
        await Assertions.Expect(actualUrl).ToHaveTextAsync(expectedUrl);
    }

    internal async Task ThenSavedComicIdIs(string expectedComicId)
    {
        var actualId = _page.GetByLabel("Saved comic id");
        await Assertions.Expect(actualId).ToHaveTextAsync(expectedComicId);
    }

    internal async Task ThenErrorMessageContains(string expectedErrorContains)
    {
        var status = _page.GetByLabel("status");
        await Assertions.Expect(status).ToContainTextAsync(expectedErrorContains, new() { Timeout = 3000 });
    }
}
