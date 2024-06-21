using MyWeirdStuff.ApiService.Features.SharedFeature.Events;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

public interface IEventStore
{
    Task Insert(string streamId, IEvent @event);
    IAsyncEnumerable<IEvent> Read(string streamId);
}
