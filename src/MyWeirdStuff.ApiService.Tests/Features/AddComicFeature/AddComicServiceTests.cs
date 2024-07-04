using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MyWeirdStuff.ApiService.Features.AddComicFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;
using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;
using NSubstitute;

namespace MyWeirdStuff.ApiService.Tests.Features.AddComicFeature;

public sealed class AddComicServiceTests
{
    private readonly AddComicService _sut;
    private readonly IEventStore _eventStoreMock;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public AddComicServiceTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSharedFeature();
        serviceCollection.AddAddComicFeature();

        _eventStoreMock = Substitute.For<IEventStore>();
        serviceCollection.AddSingleton(_eventStoreMock);

        _fakeTimeProvider = new();
        serviceCollection.AddSingleton<TimeProvider>(_fakeTimeProvider);

        serviceCollection.AddHybridCache();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<AddComicService>();
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
    public async Task ShouldDeduplicateRequests()
    {
        // Given
        var request = new AddComicRequest { Url = "https://xkcd.com/1" };

        // When
        var task1 = _sut.AddComic(request, CancellationToken.None);
        var task2 = _sut.AddComic(request, CancellationToken.None);
        var task3 = _sut.AddComic(request, CancellationToken.None);
        await Task.WhenAll(task1, task2, task3);

        // Then
        await _eventStoreMock
            .ReceivedWithAnyArgs(1)
            .Insert(default!, default!, default);

        var actual1 = await task1;
        Assert.Equal(request.Url, actual1.Url);
        var actual2 = await task2;
        Assert.Equal(request.Url, actual2.Url);
    }
}
