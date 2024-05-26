using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Helpers;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure.Models;

namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public sealed class AddComicService
{
    private readonly IBlobStore _blobStore;

    public AddComicService(IBlobStore blobStore)
    {
        _blobStore = blobStore;
    }

    public async Task<ComicDto> AddComic(AddComicRequest request)
    {
        var uri = new Uri(request.Url);
        var entity = new ComicEntity
        {
            Url = request.Url,
        };
        var blobName = uri.AbsolutePath[1..];
        var blobData = BlobConvertHelper.ConvertToStream(entity);
        await _blobStore.Save(uri.Host, blobName, blobData);

        var dto = new ComicDto
        {
            Url = entity.Url,
        };
        return dto;
    }
}
