using MyWeirdStuff.ApiService.Features.SharedFeature.Events;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Models;

public sealed class Comic
{
    public string Id { get; private set; }
    public Uri Url { get; private set; }
    public string? Description { get; private set; }

    private Comic(string id, Uri url)
    {
        Id = id;
        Url = url;
    }

    public static async Task<Comic?> Create(IAsyncEnumerable<IEvent> events)
    {
        Comic? comic = null;
        await foreach (var @event in events)
        {
            if (@event is ComicAddedEvent comicAddedEvent)
            {
                if (comic is null)
                {
                    comic = new Comic(comicAddedEvent.PartitionKey, new Uri(comicAddedEvent.Url));
                }
            }
        }
        return comic;
    }
}
