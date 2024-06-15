using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

namespace MyWeirdStuff.ApiService.Infrastructure;

public sealed class EventStore : IEventStore
{
    private static readonly Dictionary<string, List<IEvent>> _events = new();
    private readonly ILogger<EventStore> _logger;

    public EventStore(ILogger<EventStore> logger)
    {
        _logger = logger;
    }
    
    public Task Insert(string streamId, IEvent @event)
    {
        if (_events.TryGetValue(streamId, out var events))
        {
            events.Add(@event);
        }
        else
        {
            _events.Add(streamId, new List<IEvent> { @event });
        }
        _logger.LogInformation("Event {Event} added to stream {StreamId}", @event, streamId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IEvent>> Read(string streamId)
    {
        _logger.LogInformation("Reading events from stream {StreamId}", streamId);
        if (!_events.TryGetValue(streamId, out var events))
        {
            return Task.FromResult(Enumerable.Empty<IEvent>());
        }
        return Task.FromResult(events.AsEnumerable());
    }
}
