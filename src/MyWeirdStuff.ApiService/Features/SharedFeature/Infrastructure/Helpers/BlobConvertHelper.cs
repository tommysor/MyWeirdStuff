using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Models;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Helpers;

public static class BlobConvertHelper
{
    public static T ConvertToEntity<T>(Stream blobData)
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(blobData);
        return result!;
    }

    public static Stream ConvertToStream(ComicEntity entity)
    {
        var result = new MemoryStream();
        System.Text.Json.JsonSerializer.Serialize(result, entity);
        result.Position = 0;
        return result;
    }
}
