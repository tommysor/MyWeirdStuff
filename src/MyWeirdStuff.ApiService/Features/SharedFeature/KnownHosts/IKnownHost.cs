using System.Security.Cryptography;
using System.Text;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public interface IKnownHost
{
    private static readonly HashAlgorithm _hashAlgorithm = SHA256.Create();

    public string GenerateStreamId(string url)
    {
        string host = GetHostFromUrl(url);
        var hostBytes = Encoding.UTF8.GetBytes(host);
        var hashBytes = _hashAlgorithm.ComputeHash(hostBytes);
        var hashHex = Convert.ToHexString(hashBytes);
        var hostStart = new string(host.Take(4).ToArray());
        var remaining = 8 - hostStart.Length;
        var streamId = hostStart + hashHex[..remaining];

        //todo add specific comic id
        return streamId;
    }
    
    public static string GetHostFromUrl(string url)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        if (host.StartsWith("www."))
        {
            host = host[4..];
        }

        return host;
    }
}