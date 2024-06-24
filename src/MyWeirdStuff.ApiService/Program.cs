using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using MyWeirdStuff.ApiService.Features.AddComicFeature;
using MyWeirdStuff.ApiService.Features.SharedFeature;
using MyWeirdStuff.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Using manual since NuGet package with AddAzureBlobClient (Aspire.Azure.Storage.Blobs) failing, maybe due to package not updated to net9.0
builder.Services.AddAzureClients(clientFactory =>
{
    var blobServiceUri = builder.Configuration["services:storage:blob:0"]!;
    var tableServiceUri = builder.Configuration["services:storage:table:0"]!;
    if (builder.Environment.IsDevelopment())
    {
        /*
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;
AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;
TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;
        */
        const string endpointsProtocol = "DefaultEndpointsProtocol=http";
        const string accountName = "AccountName=devstoreaccount1";
        const string accountKey = "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        var blobServiceConnectionString = $"{endpointsProtocol};{accountName};{accountKey};BlobEndpoint={blobServiceUri}/devstoreaccount1";
        clientFactory.AddBlobServiceClient(blobServiceConnectionString);

        var tableServiceConnectionString = $"{endpointsProtocol};{accountName};{accountKey};TableEndpoint={tableServiceUri}/devstoreaccount1";
        clientFactory.AddTableServiceClient(tableServiceConnectionString);
    }
    else
    {
        clientFactory.AddBlobServiceClient(new Uri(blobServiceUri));
        clientFactory.AddTableServiceClient(new Uri(tableServiceUri));
        clientFactory.UseCredential(new DefaultAzureCredential());
    }
});

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure();
builder.Services.AddSharedFeature();
builder.Services.AddAddComicFeature();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapAddComicFeature();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var tableServiceClient = scope.ServiceProvider.GetRequiredService<Azure.Data.Tables.TableServiceClient>();
    await tableServiceClient.CreateTableIfNotExistsAsync("comics");
}

// c1
// c2
// c3
// c4

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
