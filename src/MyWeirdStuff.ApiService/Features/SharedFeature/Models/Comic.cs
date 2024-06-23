using MyWeirdStuff.ApiService.Features.SharedFeature.Events;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Models;

public sealed class Comic
{
    public string Id { get; set; }
    public Uri Url { get; set; }
    public string? Description { get; set; }

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
