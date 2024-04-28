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
}
