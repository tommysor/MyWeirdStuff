var builder = DistributedApplication.CreateBuilder(args);

// Using container since NuGet package with AddAzureStorage (Aspire.Hosting.Azure.Storage) failing, maybe due to package not updated to net9.0
var storage = builder.AddContainer("storage", "mcr.microsoft.com/azure-storage/azurite", "latest")
    .WithEndpoint(containerPort: 10000, hostPort: 10000, scheme: "http", name: "blob")
    ;
var blob = storage.GetEndpoint("blob");

var apiService = builder.AddProject<Projects.MyWeirdStuff_ApiService>("apiservice")
    .WithReference(blob)
    ;

builder.AddProject<Projects.MyWeirdStuff_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
