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
    private readonly IKnownHostsService _knownHostsServiceMock;
    private readonly IEventStore _eventStoreMock;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public AddComicServiceTests()
    {
        _knownHostsServiceMock = Substitute.For<IKnownHostsService>();
        _eventStoreMock = Substitute.For<IEventStore>();
        _fakeTimeProvider = new();
        _sut = new(_knownHostsServiceMock, _eventStoreMock, _fakeTimeProvider);
    }

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://a.b/c",
        };

        // When
        var actual = await _sut.AddComic(request, CancellationToken.None);

        // Then
        Assert.Equal("https://a.b/c", actual.Url);
    }

    [Fact]
    public async Task ShouldInsertEvent()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://a.b/c",
        };

        // When
        await _sut.AddComic(request, CancellationToken.None);

        // Then
        await _eventStoreMock
            .ReceivedWithAnyArgs(1)
            .Insert(default!, default!, default);
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
                Url = "https://a.b/c",
                PartitionKey = "a",
                RowKey = "b",
            };
        };
        _eventStoreMock
            .Read(default!, default)
            .ReturnsForAnyArgs(GetEvents());

        var request = new AddComicRequest
        {
            Url = "https://a.b/c",
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
            Url = "https://a.b",
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
        _knownHostsServiceMock
            .GetKnownHost(default!)
            .ReturnsForAnyArgs((IKnownHost?)null);

        var request = new AddComicRequest
        {
            Url = "https://not.relevant/c",
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
        var request = new AddComicRequest { Url = "https://a.b/c" };
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
