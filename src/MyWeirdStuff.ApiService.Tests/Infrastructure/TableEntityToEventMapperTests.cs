using Azure.Data.Tables;
using MyWeirdStuff.ApiService.Features.SharedFeature.Events;
using MyWeirdStuff.ApiService.Infrastructure;

namespace MyWeirdStuff.ApiService.Tests.Infrastructure;

public class TableEntityToEventMapperTests
{
    private readonly TableEntity _tableEntity;

    public TableEntityToEventMapperTests()
    {
        _tableEntity = new TableEntity
        {
            PartitionKey = "xkcd.com-1",
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
}
