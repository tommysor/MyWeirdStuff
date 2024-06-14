using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Tests.Features.SharedFeature.KnownHosts;

public class IKnownHostTests
{
    private class KnownHostTestClass : IKnownHost {}

    private readonly IKnownHost _sut = new KnownHostTestClass();

    [Fact]
    public void ShouldGenerateStreamIdWithSameStartWhenHostIsEqual()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://a.b/d");

        // Then
        Assert.Equal(streamId1[..8], streamId2[..8]);
    }

    [Fact]
    public void ShouldGenerateStreamIdWithDifferentStartWhenHostIsDifferent()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://x.y/z");

        // Then
        Assert.NotEqual(streamId1[..8], streamId2[..8]);
    }

    [Fact]
    public void ShouldGenerateStreamIdIgnoringWwwPart()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://www.a.b/d");

        // Then
        Assert.Equal(streamId1[..8], streamId2[..8]);
    }

    [Fact]
    public void ShouldGenerateStreamIdIgnoringCase()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://A.B/c");
        var streamId2 = _sut.GenerateStreamId("https://a.b/d");

        // Then
        Assert.Equal(streamId1[..8], streamId2[..8]);
    }
}
