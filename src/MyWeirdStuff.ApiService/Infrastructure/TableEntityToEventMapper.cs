using Azure.Data.Tables;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;

namespace MyWeirdStuff.ApiService.Infrastructure;

public static class TableEntityToEventMapper
{
    public static IEvent Map(TableEntity tableEntity)
    {
        var eventType = tableEntity.GetString("EventType");
        var eventTypeVersion = tableEntity.GetInt32("EventTypeVersion");
        IEvent mapped = eventType switch
        {
            null or "" or "ComicAddedEvent" =>
                eventTypeVersion switch
                {
                    null or 0 => new ComicAddedEvent{
                        PartitionKey = tableEntity.PartitionKey,
                        RowKey = tableEntity.RowKey,
                        Timestamp = tableEntity.Timestamp,
                        ETag = tableEntity.ETag,
                        Url = tableEntity.GetString("Url"),
                    },
                    _ => throw new InvalidOperationException($"Unknown event type version {eventTypeVersion} for event type {eventType}"),
                },
            _ => throw new InvalidOperationException($"Unknown event type {eventType}"),
        };
        return mapped;
    }
}
