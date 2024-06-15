namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public static class AddComicFeatureExtensions
{
    public static IServiceCollection AddAddComicFeature(this IServiceCollection services)
    {
        services.AddScoped<AddComicService>();
        return services;
    }
}
