using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Models;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

public sealed class ComicsRepository
{
    private readonly IEventStore _eventStore;

    public ComicsRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Comic?> GetComic(string id, CancellationToken cancellationToken)
    {
        var events = _eventStore.Read(id, cancellationToken);
        return await Comic.Create(events);
    }

    public async Task Insert(string streamId, IEvent @event, CancellationToken cancellationToken)
    {
        await _eventStore.Insert(streamId, @event, cancellationToken);
    }
}
