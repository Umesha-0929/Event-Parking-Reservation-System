namespace SEVPMS.Application.Interfaces.Providers;

public interface IMapProvider
{
    Task<string?> GetRouteAsync(string from, string to, CancellationToken cancellationToken = default);
}
