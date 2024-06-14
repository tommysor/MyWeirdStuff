namespace MyWeirdStuff.ApiService.Features.SharedFeature.KnownHosts;

public interface IKnownHostsService
{
    IKnownHost? GetKnownHost(string url);
}
