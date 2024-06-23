using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Features.SharedFeature.Models;

namespace MyWeirdStuff.ApiService.Tests.Models;

public class ComicTests
{
    private static async IAsyncEnumerable<IEvent> ToAsyncEnumerable(params IEnumerable<IEvent> events)
    {
        await Task.CompletedTask;
        foreach (var @event in events)
        {
            yield return @event;
        }
    }

    [Fact]
    public async Task ShouldHaveId()
    {
        // Given
        var comicAddedEvent = new ComicAddedEvent
        {
            PartitionKey = "a",
            RowKey = "b",
            Url = "https://a.b/c",
        };
        
        // When
        var actual = await Comic.Create(ToAsyncEnumerable(comicAddedEvent));
        
        // Then
        Assert.NotNull(actual);
        Assert.Equal("a", actual.Id);
    }

    [Fact]
    public async Task ShouldHaveUrl()
    {
        // Given
        var comicAddedEvent = new ComicAddedEvent
        {
            PartitionKey = "a",
            RowKey = "b",
            Url = "https://a.b/c",
        };
        
        // When
        var actual = await Comic.Create(ToAsyncEnumerable(comicAddedEvent));
        
        // Then
        Assert.NotNull(actual);
        Assert.Equal("https://a.b/c", actual.Url.AbsoluteUri);
    }

}
