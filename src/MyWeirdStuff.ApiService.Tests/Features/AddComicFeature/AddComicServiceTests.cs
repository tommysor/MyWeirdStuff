using MyWeirdStuff.ApiService.Features.AddComicFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using NSubstitute;

namespace MyWeirdStuff.ApiService.Tests.Features.AddComicFeature;

public sealed class AddComicServiceTests
{
    private readonly AddComicService _sut;
    private readonly IBlobStore _blobStoreMock;

    public AddComicServiceTests()
    {
        _blobStoreMock = Substitute.For<IBlobStore>();
        _sut = new(_blobStoreMock);
    }

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://url.ab",
        };

        // When
        var actual = await _sut.AddComic(request);

        // Then
        Assert.Equal("https://url.ab", actual.Url);
    }

    [Fact]
    public async Task ShouldSetContainerName()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://a.b/c",
        };

        // When
        _ = await _sut.AddComic(request);

        // Then
        await _blobStoreMock
            .Received(1)
            .Save(
                Arg.Is<string>(actual => actual == "a.b"),
                Arg.Any<string>(),
                Arg.Any<Stream>()
            );
    }

    [Fact]
    public async Task ShouldSetBlobName()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://a.b/c",
        };

        // When
        _ = await _sut.AddComic(request);

        // Then
        await _blobStoreMock
            .Received(1)
            .Save(
                Arg.Any<string>(),
                Arg.Is<string>(actual => actual == "c"),
                Arg.Any<Stream>()
            );
    }
}
