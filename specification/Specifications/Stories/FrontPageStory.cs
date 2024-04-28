namespace Specifications.Stories;

public sealed class FrontPageStory
{
    private readonly SutDriver _sutDriver;

    public FrontPageStory()
    {
        _sutDriver = new SutDriver(x => x.Chromium);
    }

    [Fact]
    public async Task ShouldShowFrontPage()
    {
        await _sutDriver.Initialize();
    }
}
