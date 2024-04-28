namespace MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

public interface IBlobStore
{
    Task Save(string container, string blobName, Stream blobData);
}
