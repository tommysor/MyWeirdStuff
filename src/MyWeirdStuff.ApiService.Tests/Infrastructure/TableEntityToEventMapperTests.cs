using Azure.Data.Tables;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Infrastructure;

namespace MyWeirdStuff.ApiService.Tests.Infrastructure;

public class TableEntityToEventMapperTests
{
    private readonly TableEntity _tableEntity;
    private const string _tableEntityPartitionKey = "xkcd.com-1";

    public TableEntityToEventMapperTests()
    {
        _tableEntity = new TableEntity
        {
            PartitionKey = _tableEntityPartitionKey,
            RowKey = "20240703123456789",
            Timestamp = DateTimeOffset.UtcNow,
            ETag = new Azure.ETag("etag"),
        };
        _tableEntity.Add("EventType", "ComicAddedEvent");
        _tableEntity.Add("EventTypeVersion", 0);
        _tableEntity.Add("Url", "https://xkcd.com/1/");        
    }

    [Fact]
    public void Map_WhenComicAddedEvent_ThenMapsToComicAddedEvent()
    {        
        // Given
        var actual = TableEntityToEventMapper.Map(_tableEntity);
        
        // Then
        Assert.IsType<ComicAddedEvent>(actual);
    }

    [Fact]
    public void Map_WhenComicAddedEvent_ThenIdAndUrlShouldBeMapped()
    {
        // When
        var returned = TableEntityToEventMapper.Map(_tableEntity);

        // Then
        var actual = Assert.IsType<ComicAddedEvent>(returned);
        Assert.Equal(_tableEntityPartitionKey, actual.PartitionKey);
        Assert.Equal("https://xkcd.com/1/", actual.Url);
    }

    [Fact]
    public void Map_WhenEventTypeMissing_ShouldDefaultToComicAddedEvent()
    {
        // Given
        _tableEntity.Remove("EventType");
        _tableEntity.Remove("EventTypeVersion");
        
        // When
        var actual = TableEntityToEventMapper.Map(_tableEntity);
        
        // Then
        var comicAddedEvent = Assert.IsType<ComicAddedEvent>(actual);
        Assert.Equal(_tableEntityPartitionKey, comicAddedEvent.PartitionKey);
    }

    [Fact]
    public void Map_WhenEventTypeVersionMissing_ShouldDefaultToComicAddedEvent()
    {
        // Given
        _tableEntity.Remove("EventTypeVersion");
        
        // When
        var actual = TableEntityToEventMapper.Map(_tableEntity);
        
        // Then
        var comicAddedEvent = Assert.IsType<ComicAddedEvent>(actual);
        Assert.Equal(_tableEntityPartitionKey, comicAddedEvent.PartitionKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Map_WhenEventTypeVersionNullOrZero_ShouldDefaultToComicAddedEvent(int? eventTypeVersion)
    {
        // Given
        _tableEntity["EventTypeVersion"] = eventTypeVersion;
        
        // When
        var actual = TableEntityToEventMapper.Map(_tableEntity);
        
        // Then
        var comicAddedEvent = Assert.IsType<ComicAddedEvent>(actual);
        Assert.Equal(_tableEntityPartitionKey, comicAddedEvent.PartitionKey);
    }
}
