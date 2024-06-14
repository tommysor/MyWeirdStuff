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
    public async Task ShouldGenerateStreamIdWithSameStartWhenHostIsEqual()
    {
        // Given
        var requestWithSameHost1 = new AddComicRequest
        {
            Url = "https://a.b/c",
        };
        var requestWithSameHost2 = new AddComicRequest
        {
            Url = "https://a.b/d",
        };

        // When
        await _sut.AddComic(requestWithSameHost1);
        await _sut.AddComic(requestWithSameHost2);

        // Then
        var calls = _eventStoreMock
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IEventStore.Insert))
            .ToArray();
        Assert.Equal(2, calls.Length);
        var streamIdFirstSameHost = (string?)calls[0].GetArguments()[0];
        var streamIdSecondSameHost = (string?)calls[1].GetArguments()[0];
        Assert.NotNull(streamIdFirstSameHost);
        Assert.NotNull(streamIdSecondSameHost);
        Assert.Equal(streamIdFirstSameHost[..8], streamIdSecondSameHost[..8]);
    }

    [Fact]
    public async Task ShouldGenerateStreamIdWithDifferentStartWhenHostIsDifferent()
    {
        // Given
        var requestWithDifferentHost1 = new AddComicRequest
        {
            Url = "https://a.b/c",
        };
        var requestWithDifferentHost2 = new AddComicRequest
        {
            Url = "https://x.y/z",
        };

        // When
        await _sut.AddComic(requestWithDifferentHost1);
        await _sut.AddComic(requestWithDifferentHost2);

        // Then
        var calls = _eventStoreMock
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IEventStore.Insert))
            .ToArray();
        Assert.Equal(2, calls.Length);
        var streamIdFirstDifferentHost = (string?)calls[0].GetArguments()[0];
        var streamIdSecondDifferentHost = (string?)calls[1].GetArguments()[0];
        Assert.NotNull(streamIdFirstDifferentHost);
        Assert.NotNull(streamIdSecondDifferentHost);
        Assert.NotEqual(streamIdFirstDifferentHost[..8], streamIdSecondDifferentHost[..8]);
    }

    [Fact]
    public async Task ShouldGenerateStreamIdIgnoringWwwPart()
    {
        // Given
        var requestWithWww = new AddComicRequest
        {
            Url = "https://www.a.b/c",
        };
        var requestWithoutWww = new AddComicRequest
        {
            Url = "https://a.b/d",
        };

        // When
        await _sut.AddComic(requestWithWww);
        await _sut.AddComic(requestWithoutWww);

        // Then
        var calls = _eventStoreMock
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IEventStore.Insert))
            .ToArray();
        Assert.Equal(2, calls.Length);
        var streamIdFirstWithWww = (string?)calls[0].GetArguments()[0];
        var streamIdSecondWithoutWww = (string?)calls[1].GetArguments()[0];
        Assert.NotNull(streamIdFirstWithWww);
        Assert.NotNull(streamIdSecondWithoutWww);
        Assert.Equal(streamIdFirstWithWww[..8], streamIdSecondWithoutWww[..8]);
    }

    [Fact]
    public async Task ShouldGenerateStreamIdIgnoringCase()
    {
        // Given
        var requestWithUpperCase = new AddComicRequest
        {
            Url = "https://A.B/c",
        };
        var requestWithLowerCase = new AddComicRequest
        {
            Url = "https://a.b/d",
        };

        // When
        await _sut.AddComic(requestWithUpperCase);
        await _sut.AddComic(requestWithLowerCase);

        // Then
        var calls = _eventStoreMock
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IEventStore.Insert))
            .ToArray();
        Assert.Equal(2, calls.Length);
        var streamIdFirstWithUpperCase = (string?)calls[0].GetArguments()[0];
        var streamIdSecondWithLowerCase = (string?)calls[1].GetArguments()[0];
        Assert.NotNull(streamIdFirstWithUpperCase);
        Assert.NotNull(streamIdSecondWithLowerCase);
        Assert.Equal(streamIdFirstWithUpperCase[..8], streamIdSecondWithLowerCase[..8]);
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
