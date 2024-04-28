var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyWeirdStuff_ApiService>("apiservice");

builder.AddProject<Projects.MyWeirdStuff_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
