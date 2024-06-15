using MyWeirdStuff.ApiService.Features.SharedFeature.Infrastructure;

namespace MyWeirdStuff.ApiService.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEventStore, EventStore>();
        return services;
    }
}
