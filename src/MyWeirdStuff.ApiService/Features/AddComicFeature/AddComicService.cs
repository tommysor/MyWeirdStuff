using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Helpers;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public sealed class AddComicService
{
    private readonly IKnownHostsService _knownHostsService;
    private readonly IEventStore _eventStore;
    private readonly TimeProvider _timeProvider;

    public AddComicService(
        IKnownHostsService knownHostsService,
        IEventStore eventStore,
        TimeProvider timeProvider)
    {
        _knownHostsService = knownHostsService;
        _eventStore = eventStore;
        _timeProvider = timeProvider;
    }

    public async Task<ComicDto> AddComic(AddComicRequest request)
    {
        var uri = new Uri(request.Url);
        if (uri.AbsolutePath.Length < 2)
        {
            throw new InvalidOperationException("Path segment is required to identify the comic");
        }

        var knownHost = _knownHostsService.GetKnownHost(request.Url);
        if (knownHost is null)
        {
            throw new InvalidOperationException("Host is not supported");
        }

        var streamId = StreamIdHelper.GenerateStreamId(request.Url);
        var existings = await _eventStore.Read(streamId);
        if (existings.Any())
        {
            throw new InvalidOperationException("Comic already exists");
        }

        var now = _timeProvider.GetUtcNow();
        var rowKeyTimePart = now.ToString("yyyyMMddHHmmssfff");

        var @event = new ComicAddedEvent
        {
            Url = request.Url,
            PartitionKey = streamId,
            RowKey = rowKeyTimePart + "-" + Guid.NewGuid().ToString(),
        };
        await _eventStore.Insert(streamId, @event);

        var dto = new ComicDto
        {
            Url = @event.Url,
        };
        return dto;
    }
}
