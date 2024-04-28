namespace Specifications.Stories;

/// <summary>
/// As a user
/// I should be able to see the front page
/// So that I can then do something useful
/// </summary>
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
