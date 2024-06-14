using System;
using Azure.Data.Tables;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.Events;

public interface IEvent : ITableEntity
{
}
