using Microsoft.Extensions.Time.Testing;
using MyWeirdStuff.ApiService.Features.AddComicFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
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
        _eventStoreMock = Substitute.For<IEventStore>();
        _fakeTimeProvider = new();
        _sut = new(_eventStoreMock, _fakeTimeProvider);
    }

    [Fact]
    public async Task ShouldAddUrl()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://url.ab/c",
        };

        // When
        var actual = await _sut.AddComic(request);

        // Then
        Assert.Equal("https://url.ab/c", actual.Url);
    }

    [Fact]
    public async Task ShouldInsertEvent()
    {
        // Given
        var request = new AddComicRequest
        {
            Url = "https://url.ab/c",
        };

        // When
        await _sut.AddComic(request);

        // Then
        await _eventStoreMock
            .ReceivedWithAnyArgs(1)
            .Insert(default!, default!);
    }

    [Fact]
    public async Task ShouldNotAddComicIfAlreadyExists()
    {
        // Given
        _eventStoreMock
            .Read(default!)
            .ReturnsForAnyArgs(new List<IEvent>
            {
                new ComicAddedEvent
                {
                    Url = "https://url.ab/c",
                    PartitionKey = "a",
                    RowKey = "b",
                }
            });

        var request = new AddComicRequest
        {
            Url = "https://url.ab/c",
        };

        // When
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(()=> _sut.AddComic(request));

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
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(()=> _sut.AddComic(request));

        // Then
        Assert.Contains("Path segment", ex.Message);
    }
}
