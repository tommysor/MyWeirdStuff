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

    public async Task Insert(string streamId, IEvent @event)
    {
        var addEntityResponse = await _tableClient.AddEntityAsync(@event);
        if (addEntityResponse.IsError)
        {
            var exceptionMessage = $"{addEntityResponse.Status}:{addEntityResponse.ReasonPhrase}:{addEntityResponse.ClientRequestId}";
            throw new InvalidOperationException(exceptionMessage);
        }
        _logger.LogInformation("Event {Event} added to stream {StreamId}", @event, streamId);
    }

    public async IAsyncEnumerable<IEvent> Read(string streamId)
    {
        _logger.LogInformation("Reading events from stream {StreamId}", streamId);
        var pageable = _tableClient.QueryAsync<ComicAddedEvent>(e => e.PartitionKey == streamId);
        await foreach (var @event in pageable)
        {
            yield return @event;
        }
    }
}
