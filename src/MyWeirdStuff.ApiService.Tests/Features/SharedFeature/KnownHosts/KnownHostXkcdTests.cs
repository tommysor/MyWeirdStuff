using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Tests.Features.SharedFeature.KnownHosts;

public class KnownHostXkcdTests
{
    private readonly IKnownHost _sut = new KnownHostXkcd();

    [Fact]
    public void ShouldGenerateStreamIdSameWhenPathIsEqual()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://xkcd.com/1/");
        var streamId2 = _sut.GenerateStreamId("https://xkcd.com/1/");

        // Then
        Assert.Equal(streamId1, streamId2);
    }

    [Fact]
    public void ShouldGenerateStreamIdDifferentWhenPathIsDifferent()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://xkcd.com/1/");
        var streamId2 = _sut.GenerateStreamId("https://xkcd.com/2/");

        // Then
        Assert.NotEqual(streamId1, streamId2);
    }

    [Theory]
    [InlineData("https://xkcd.com/1/")]
    public void ShouldGenerateStreamIdWithoutIllegalCharacters(string url)
    {
        // When
        var streamId = _sut.GenerateStreamId(url);

        // Then
        // Taken from https://learn.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#characters-disallowed-in-key-fields
        /*
The forward slash (/) character

The backslash (\) character

The number sign (#) character

The question mark (?) character

Control characters from U+0000 to U+001F, including:

The horizontal tab (\t) character
The linefeed (\n) character
The carriage return (\r) character
Control characters from U+007F to U+009F
        */
        Assert.DoesNotContain(streamId, c => c == '/');
        Assert.DoesNotContain(streamId, c => c == '\\');
        Assert.DoesNotContain(streamId, c => c == '#');
        Assert.DoesNotContain(streamId, c => c == '?');
        Assert.DoesNotContain(streamId, c => c is >= '\u0000' and <= '\u001F');
        Assert.DoesNotContain(streamId, c => c is >= '\u007F' and <= '\u009F');
    }
}
