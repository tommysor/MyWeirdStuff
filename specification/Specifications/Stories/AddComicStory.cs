namespace Specifications.Stories;

/// <summary>
/// As a user
/// I should be able to add a comic
/// So that I can expand the collection
/// </summary>
public sealed class AddComicStory : IAsyncLifetime
{
    private readonly SutDriver _sutDriver;
    private readonly Random _random = new();

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

    private int GenerateRandomXkcdComicId()
        => _random.Next(1, 2500);

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var id = GenerateRandomXkcdComicId();

        // When
        await _sutDriver.AddComic($"https://xkcd.com/{id}/");

        // Then
        await _sutDriver.AssertAddComicResponse($"https://xkcd.com/{id}/");
    }
}
