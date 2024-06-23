using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;
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

    public async Task<ComicDto> AddComic(AddComicRequest request, CancellationToken cancellationToken)
    {
        var uri = new Uri(request.Url);
        if (uri.AbsolutePath.Length < 2)
        {
            throw new ValidationException("Path segment is required to identify the comic");
        }

        var knownHost = _knownHostsService.GetKnownHost(request.Url);
        if (knownHost is null)
        {
            throw new ValidationException("Host is not supported");
        }

        var streamId = knownHost.GenerateStreamId(request.Url);
        var existings = _eventStore.Read(streamId, cancellationToken);
        var anyExistings = false;
        await foreach (var _ in existings)
        {
            anyExistings = true;
        }
        if (anyExistings)
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
        await _eventStore.Insert(streamId, @event, cancellationToken);

        var dto = new ComicDto
        {
            Url = @event.Url,
        };
        return dto;
    }
}
