namespace Specifications.Stories;

/// <summary>
/// As a user
/// I should be able to add a comic
/// So that I can expand the collection
/// </summary>
public sealed class AddComicStory : IAsyncLifetime
{
    private readonly SutDriver _sutDriver;

    public AddComicStory()
    {
        _sutDriver = new(x => x.Chromium);
    }

    public Task DisposeAsync()
        => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await _sutDriver.Initialize();
    }

    [Fact(Skip = "Not implemented")]
    public async Task ShouldAddUrl()
    {
        // When
        await _sutDriver.AddComic("https://someurl.aa");

        // Then
        await _sutDriver.AssertAddComicResponse("https://someurl.aa");
    }
}
