using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Using manual since NuGet package with AddAzureBlobClient (Aspire.Azure.Storage.Blobs) failing, maybe due to package not updated to net9.0
builder.Services.AddAzureClients(clientFactory =>
{
    var blobServiceUri = builder.Configuration["services:storage:blob:0"]!;
    if (builder.Environment.IsDevelopment())
    {
        /*
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;
AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
        */
        const string endpointsProtocol = "DefaultEndpointsProtocol=http";
        const string accountName = "AccountName=devstoreaccount1";
        const string accountKey = "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
        var endpoint = $"BlobEndpoint={blobServiceUri}/devstoreaccount1";
        var blobServiceConnectionString = $"{endpointsProtocol};{accountName};{accountKey};{endpoint}";

        clientFactory.AddBlobServiceClient(blobServiceConnectionString);
    }
    else
    {
        var serviceUri = new Uri(blobServiceUri);
        clientFactory.AddBlobServiceClient(serviceUri);
        clientFactory.UseCredential(new DefaultAzureCredential());
    }
});

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

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

app.MapDefaultEndpoints();

{
    // Temporary to test connection to Blob store

    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TestBlobs");
    try
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        var containerConnectionUri = configuration["services:storage:blob:0"];
        logger.LogInformation("BlobContainerConnection: {Uri}", containerConnectionUri);

        var blobService = app.Services.GetRequiredService<BlobServiceClient>();
        var testContainer = blobService.GetBlobContainerClient("test1");

        await testContainer.CreateIfNotExistsAsync();
        var blobs = testContainer.GetBlobsAsync();
        logger.LogInformation("Blobs fetched");
        await foreach (var page in blobs.AsPages())
        {
            foreach (var blob in page.Values)
            {
                logger.LogInformation("Blob '{BlobName}'", blob.Name);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Temporary test blob store failed");
    }
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
