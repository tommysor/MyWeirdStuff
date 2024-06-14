using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Tests.Features.SharedFeature.KnownHosts;

public class IKnownHostTests
{
    private class KnownHostTestClass : IKnownHost
    {
        public string GenerateStreamIdPartFromPath(string path)
        {
            return path;
        }
    }

    private readonly IKnownHost _sut = new KnownHostTestClass();

    private static string GetHostPartFromStreamId(string streamId)
    {
        return streamId.Split('-')[0];
    }

    [Fact]
    public void ShouldGenerateStreamIdWithSameStartWhenHostIsEqual()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://a.b/d");

        // Then
        var streamId1Start = GetHostPartFromStreamId(streamId1);
        var streamId2Start = GetHostPartFromStreamId(streamId2);
        Assert.Equal(streamId1Start, streamId2Start);
    }

    [Fact]
    public void ShouldGenerateStreamIdWithDifferentStartWhenHostIsDifferent()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://x.y/z");

        // Then
        var streamId1Start = GetHostPartFromStreamId(streamId1);
        var streamId2Start = GetHostPartFromStreamId(streamId2);
        Assert.NotEqual(streamId1Start, streamId2Start);
    }

    [Fact]
    public void ShouldGenerateStreamIdIgnoringWwwPart()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://a.b/c");
        var streamId2 = _sut.GenerateStreamId("https://www.a.b/d");

        // Then
        var streamId1Start = GetHostPartFromStreamId(streamId1);
        var streamId2Start = GetHostPartFromStreamId(streamId2);
        Assert.Equal(streamId1Start, streamId2Start);
    }

    [Fact]
    public void ShouldGenerateStreamIdIgnoringCase()
    {
        // When
        var streamId1 = _sut.GenerateStreamId("https://A.B/c");
        var streamId2 = _sut.GenerateStreamId("https://a.b/d");

        // Then
        var streamId1Start = GetHostPartFromStreamId(streamId1);
        var streamId2Start = GetHostPartFromStreamId(streamId2);
        Assert.Equal(streamId1Start, streamId2Start);
    }
}
