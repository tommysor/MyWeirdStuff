var builder = DistributedApplication.CreateBuilder(args);

// Using container since NuGet package with AddAzureStorage (Aspire.Hosting.Azure.Storage) failing, maybe due to package not updated to net9.0
var storage = builder.AddContainer("storage", "mcr.microsoft.com/azure-storage/azurite", "latest")
    .WithEndpoint(containerPort: 10000, hostPort: 10000, scheme: "http", name: "blob")
    .WithEndpoint(containerPort: 10002, hostPort: 10002, scheme: "http", name: "table")
    ;
var blob = storage.GetEndpoint("blob");
var table = storage.GetEndpoint("table");

var apiService = builder.AddProject<Projects.MyWeirdStuff_ApiService>("apiservice")
    .WithReference(blob)
    .WithReference(table)
    ;

builder.AddProject<Projects.MyWeirdStuff_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
