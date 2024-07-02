using System.Security.Cryptography;
using System.Text;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public interface IKnownHost
{
    public string GenerateStreamIdPartFromPath(string path);

    public string GenerateStreamId(string url)
    {
        var uri = new Uri(url);
        string host = GetHostFromUrl(uri);
        string path = GetPathWithBasicSanitation(uri);
        string streamIdPathPart = GenerateStreamIdPartFromPath(path);
        return $"{host}-{streamIdPathPart}";
    }

    private static string GetPathWithBasicSanitation(Uri uri)
    {
        // Remove the leading slash
        var path = uri.AbsolutePath[1..];
        // Remove the trailing slash
        if (path[^1] == '/')
        {
            path = path[..^1];
        }

        return path;
    }

    public static string GetHostFromUrl(Uri url)
    {
        var host = url.Host;
        if (host.StartsWith("www."))
        {
            host = host[4..];
        }
        return host;
    }
    
    public static string GetHostFromUrl(string url)
    {
        var uri = new Uri(url);
        string host = GetHostFromUrl(uri);
        return host;
    }
}
