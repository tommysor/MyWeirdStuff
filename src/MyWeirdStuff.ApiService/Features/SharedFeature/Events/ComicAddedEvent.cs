using Azure;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Events;

public sealed class ComicAddedEvent : IEvent
{
    public required string Url { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
