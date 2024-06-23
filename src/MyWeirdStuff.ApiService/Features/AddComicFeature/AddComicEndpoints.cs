using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyWeirdStuff.ApiService.Features.SharedFeature.Contracts;
using MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;

namespace MyWeirdStuff.ApiService.Features.AddComicFeature;

public static class AddComicEndpoints
{
    public static async Task<Results<Ok<ComicDto>, BadRequest<string>>> AddComic(
        [FromBody] AddComicRequest request,
        [FromServices] AddComicService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(AddComicEndpoints));
        try
        {
            var result = await service.AddComic(request, cancellationToken);
            return TypedResults.Ok(result);
        }
        catch (ValidationException ex)
        {
            logger.LogError(ex, "Validation failed");
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
