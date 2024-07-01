using System.Runtime.CompilerServices;
using Azure.Data.Tables;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

namespace MyWeirdStuff.ApiService.Infrastructure;

public sealed class EventStore : IEventStore
{
    private const string _tableName = "comics";
    private readonly ILogger<EventStore> _logger;
    private readonly TableServiceClient _tableServiceClient;
    private readonly TableClient _tableClient;

    public EventStore(ILogger<EventStore> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
        _tableClient = _tableServiceClient.GetTableClient(_tableName);
    }

    public async Task Insert(string streamId, IEvent @event, CancellationToken cancellationToken)
    {
        var addEntityResponse = await _tableClient.AddEntityAsync(@event, cancellationToken: cancellationToken);
        if (addEntityResponse.IsError)
        {
            var exceptionMessage = $"{addEntityResponse.Status}:{addEntityResponse.ReasonPhrase}:{addEntityResponse.ClientRequestId}";
            throw new InvalidOperationException(exceptionMessage);
        }
        _logger.LogInformation("Event {Event} added to stream {StreamId}", @event, streamId);
    }

    public async IAsyncEnumerable<IEvent> Read(string streamId, [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reading events from stream {StreamId}", streamId);
        var pageable = _tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == streamId, cancellationToken: cancellationToken);
        await foreach (var @event in pageable.WithCancellation(cancellationToken))
        {
            var eventType = @event.GetString("EventType");
            var eventTypeVersion = @event.GetInt32("EventTypeVersion");
            IEvent mapped = eventType switch
            {
                null or "" or "ComicAddedEvent" =>
                    eventTypeVersion switch
                    {
                        null or 0 => new ComicAddedEvent{
                            PartitionKey = @event.PartitionKey,
                            RowKey = @event.RowKey,
                            Timestamp = @event.Timestamp,
                            ETag = @event.ETag,
                            Url = @event.GetString("Url"),
                        },
                        _ => throw new InvalidOperationException($"Unknown event type version {eventTypeVersion} for event type {eventType}"),
                    },
                _ => throw new InvalidOperationException($"Unknown event type {eventType}"),
            };
            yield return mapped;
        }
    }
}
