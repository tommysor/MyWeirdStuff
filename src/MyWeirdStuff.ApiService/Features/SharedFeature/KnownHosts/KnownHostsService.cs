using MyWeirdStuff.ApiService.Features.SharedFeature.Helpers;

namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public sealed class KnownHostsService : IKnownHostsService
{
    private readonly Dictionary<string, IKnownHost> _knownHosts = new()
    {
        { "xkcd.com", new KnownHostXkcd() },
    };

    public IKnownHost? GetKnownHost(string url)
    {
        var host = StreamIdHelper.GetHost(url);
        if (_knownHosts.TryGetValue(host, out var knownHost))
        {
            return knownHost;
        }
        return null;
    }
}
