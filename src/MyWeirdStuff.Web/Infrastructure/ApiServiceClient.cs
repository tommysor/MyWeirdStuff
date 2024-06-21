using MyWeirdStuff.Web.Contracts;

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
        response.EnsureSuccessStatusCode();
        var comic = await response.Content.ReadFromJsonAsync<ComicDto>();
        return comic;
    }
}
