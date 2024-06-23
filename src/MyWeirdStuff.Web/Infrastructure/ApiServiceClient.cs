using System.Net;
using MyWeirdStuff.Web.Contracts;
using MyWeirdStuff.Web.Exceptions;

namespace MyWeirdStuff.Web.Infrastructure;

public sealed class ApiServiceClient
{
    private readonly HttpClient _httpClient;

    public ApiServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ComicDto?> AddComic(string url)
    {
        var request = new 
        { 
            Url = url,
        };
        var response = await _httpClient.PostAsJsonAsync("AddComic", request);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new ValidationException(message);
        }
        response.EnsureSuccessStatusCode();
        var comic = await response.Content.ReadFromJsonAsync<ComicDto>();
        return comic;
    }
}
