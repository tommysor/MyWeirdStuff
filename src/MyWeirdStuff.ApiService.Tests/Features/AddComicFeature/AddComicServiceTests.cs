using Microsoft.Extensions.Time.Testing;
using MyWeirdStuff.ApiService.Features.AddComicFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;
using NSubstitute;

namespace MyWeirdStuff.ApiService.Tests.Features.AddComicFeature;

public sealed class AddComicServiceTests
{
    private readonly AddComicService _sut;
    private readonly IEventStore _eventStoreMock;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public AddComicServiceTests()
    {
        _eventStoreMock = Substitute.For<IEventStore>();
        var repository = new ComicsRepository(_eventStoreMock);
        _fakeTimeProvider = new();
        _sut = new(new KnownHostsService(), repository, _fakeTimeProvider);
    }

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://xkcd.com/1",
        };

        // When
        var actual = await _sut.AddComic(request, CancellationToken.None);

        // Then
        Assert.Equal("https://xkcd.com/1", actual.Url);
    }

    [Fact]
    public async Task ShouldInsertEvent()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://xkcd.com/1",
        };

        // When
        await _sut.AddComic(request, CancellationToken.None);

        // Then
        await _eventStoreMock
            .ReceivedWithAnyArgs(1)
            .Insert(default!, default!, default);
    }

    [Fact]
    public async Task ShouldReturnComicId()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://xkcd.com/1",
        };

        // When
        var actual = await _sut.AddComic(request, CancellationToken.None);

        // Then
        Assert.Equal("xkcd.com-1", actual.Id);
    }

    [Fact]
    public async Task ShouldNotAddComicIfAlreadyExists()
    {
        // Given
        async IAsyncEnumerable<IEvent> GetEvents()
        {
            await Task.CompletedTask;
            yield return new ComicAddedEvent
            {
                Url = "https://xkcd.com/1",
                PartitionKey = "a",
                RowKey = "b",
            };
        };
        _eventStoreMock
            .Read(default!, default)
            .ReturnsForAnyArgs(GetEvents());

        var request = new AddComicRequest
        {
            Url = "https://xkcd.com/1",
        };

        // When
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(()=> _sut.AddComic(request, CancellationToken.None));

        // Then
        Assert.Contains("Comic already exists", ex.Message);
    }

    [Fact]
    public async Task ShouldValidatePath()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://xkcd.com",
        };

        // When
        var ex = await Assert.ThrowsAsync<ValidationException>(()=> _sut.AddComic(request, CancellationToken.None));

        // Then
        Assert.Contains("Path segment", ex.Message);
    }

    [Fact]
    public async Task ShouldOnlyAcceptKnownHosts()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://some.unknown/c",
        };

        // When
        var ex = await Assert.ThrowsAsync<ValidationException>(()=> _sut.AddComic(request, CancellationToken.None));

        // Then
        Assert.Contains("Host is not supported", ex.Message);
    }

    [Fact]
    public async Task ShouldPassCancellationTokenToInsert()
    {
        // Given
        var cs = new CancellationTokenSource();

        // When
        var request = new AddComicRequest { Url = "https://xkcd.com/1" };
        await _sut.AddComic(request, cs.Token);

        // Then
        await _eventStoreMock
            .Received(1)
            .Insert(
                Arg.Any<string>(),
                Arg.Any<IEvent>(),
                cs.Token);
    }
}
