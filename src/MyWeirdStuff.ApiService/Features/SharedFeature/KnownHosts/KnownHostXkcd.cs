namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public sealed class KnownHostXkcd : IKnownHost
{
    public string GenerateStreamIdPartFromPath(string path)
    {
        return path;
    }
}
