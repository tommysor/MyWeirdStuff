using Azure.Data.Tables;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Infrastructure;

namespace MyWeirdStuff.ApiService.Tests.Infrastructure;

public class TableEntityToEventMapperTests
{
    [Fact]
    public void Map_WhenComicAddedEvent_ThenMapsToComicAddedEvent()
    {
        // When
        var tableEntity = new TableEntity
        {
            PartitionKey = "partitionKey",
            RowKey = "rowKey",
            Timestamp = DateTimeOffset.UtcNow,
            ETag = new Azure.ETag("etag"),
        };
        tableEntity.Add("EventType", "ComicAddedEvent");
        tableEntity.Add("EventTypeVersion", 0);
        tableEntity.Add("Url", "https://xkcd.com/1/");
        
        // Given
        var actual = TableEntityToEventMapper.Map(tableEntity);
        
        // Then
        Assert.IsType<ComicAddedEvent>(actual);
    }
}
