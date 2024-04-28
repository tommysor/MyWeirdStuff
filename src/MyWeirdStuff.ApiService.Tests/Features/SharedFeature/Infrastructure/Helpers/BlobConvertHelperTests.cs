using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Helpers;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Models;

namespace MyWeirdStuff.ApiService.Tests.Features.SharedFeature.Infrastructure.Helpers;

public sealed class BlobConvertHelperTests
{
    [Fact]
    public void ShouldRoundtripComicEntityUrl()
    {
        // Given
        var input = new ComicEntity { Url = "https://abc.de" };

        // When
        var blobData = BlobConvertHelper.ConvertToStream(input);
        var actual = BlobConvertHelper.ConvertToEntity<ComicEntity>(blobData);

        // Then
        Assert.Equal(input.Url, actual.Url);
    }
}
