using MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

namespace MyWeirdStuff.ApiService.Features.SharedFeature;

public static class SharedFeatureExtensions
{
    public static IServiceCollection AddSharedFeature(this IServiceCollection services)
    {
        services.AddSingleton<IKnownHostsService, KnownHostsService>();
        return services;
    }
}
