using Microsoft.Extensions.Caching.Hybrid;
using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public sealed class AddComicService
{
    private readonly IKnownHostsService _knownHostsService;
    private readonly ComicsRepository _comicsRepository;
    private readonly TimeProvider _timeProvider;
    private readonly HybridCache _cache;

    public AddComicService(
        IKnownHostsService knownHostsService,
        ComicsRepository comicsRepository,
        TimeProvider timeProvider,
        HybridCache cache)
    {
        _knownHostsService = knownHostsService;
        _comicsRepository = comicsRepository;
        _timeProvider = timeProvider;
        _cache = cache;
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
        var dto = await _cache.GetOrCreateAsync(
            $"AddComic-{streamId}",
            async ct => await AddComicCore(request, streamId, ct),
            options: new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            token: cancellationToken);
        return dto;
    }

    internal async Task<ComicDto> AddComicCore(AddComicRequest request, string streamId, CancellationToken cancellationToken)
    {
        SharedFeature.Models.Comic? comic = await _comicsRepository.GetComic(streamId, cancellationToken);

        if (comic is not null)
        {
            throw new InvalidOperationException("Comic already exists");
        }

        var now = _timeProvider.GetUtcNow();
        var rowKey = now.ToString("yyyyMMddHHmmssfff");

        var @event = new ComicAddedEvent
        {
            Url = request.Url,
            PartitionKey = streamId,
            RowKey = rowKey,
        };
        await _comicsRepository.Insert(streamId, @event, cancellationToken);

        var dto = new ComicDto
        {
            Id = streamId,
            Url = @event.Url,
        };
        return dto;
    }
}
