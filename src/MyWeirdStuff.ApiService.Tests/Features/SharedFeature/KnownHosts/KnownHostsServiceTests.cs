using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Tests.Features.SharedFeature.KnownHosts;

public class KnownHostsServiceTests
{
    private readonly KnownHostsService _sut = new();

    [Theory]
    [InlineData("xkcd.com")]
    public void ShouldReturnKnownHost(string host)
    {
        // Given
        var url = $"https://{host}/";

        // When
        var knownHost = _sut.GetKnownHost(url);

        // Then
        Assert.NotNull(knownHost);
    }

    [Fact]
    public void ShouldReturnNullWhenHostIsUnknown()
    {
        // Given
        var url = "https://unknown.com/";

        // When
        var knownHost = _sut.GetKnownHost(url);

        // Then
        Assert.Null(knownHost);
    }
}
