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

    private int GenerateRandomXkcdComicNumber()
        => _random.Next(1, 2500);

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var id = GenerateRandomXkcdComicNumber();

        // When
        await _sutDriver.AddComic($"https://xkcd.com/{id}/");

        // Then
        await _sutDriver.ThenSavedUrlIs($"https://xkcd.com/{id}/");
    }

    [Fact]
    public async Task ShouldReturnComicId()
    {
        // Given
        var number = GenerateRandomXkcdComicNumber();

        // When
        await _sutDriver.AddComic($"https://xkcd.com/{number}/");

        // Then
        await _sutDriver.ThenSavedComicIdIs($"xkcd.com-{number}");
    }

    [Fact]
    public async Task ShouldExplainWhenPathIsMissing()
    {
        // When
        await _sutDriver.AddComic("https://xkcd.com");

        // Then
        await _sutDriver.ThenErrorMessageContains("required to identify the comic");
    }
}
