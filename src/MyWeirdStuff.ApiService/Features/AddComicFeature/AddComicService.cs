using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

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
        throw new NotImplementedException();
    }
}
