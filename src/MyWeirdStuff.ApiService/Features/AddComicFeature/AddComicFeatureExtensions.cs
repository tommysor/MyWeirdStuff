namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public static class AddComicFeatureExtensions
{
    public static IServiceCollection AddAddComicFeature(this IServiceCollection services)
    {
        services.AddScoped<AddComicService>();
        return services;
    }

    public static void MapAddComicFeature(this WebApplication app)
    {
        app.MapPost("/AddComic", AddComicEndpoints.AddComic);
    }
}
