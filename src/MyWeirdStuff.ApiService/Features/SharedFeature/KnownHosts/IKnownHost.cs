using System.Security.Cryptography;
using System.Text;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public interface IKnownHost
{
    private static readonly HashAlgorithm _hashAlgorithm = SHA256.Create();

    public string GenerateStreamIdPartFromPath(string path);

    public string GenerateStreamId(string url)
    {
        var uri = new Uri(url);
        string host = GetHostFromUrl(uri);
        string streamIdHostPart = GenerateStreamIdPartFromHost(host);
        string path = GetPathWithBasicSanitation(uri);
        string streamIdPathPart = GenerateStreamIdPartFromPath(path);
        return $"{streamIdHostPart}-{streamIdPathPart}";
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

    private static string GenerateStreamIdPartFromHost(string host)
    {
        var remaining = 8 - host.Length;
        if (remaining > 0)
        {
            var hostBytes = Encoding.UTF8.GetBytes(host);
            var hashBytes = _hashAlgorithm.ComputeHash(hostBytes);
            var hashHex = Convert.ToHexString(hashBytes);
            return host + hashHex[..remaining];
        }

        return host;
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
