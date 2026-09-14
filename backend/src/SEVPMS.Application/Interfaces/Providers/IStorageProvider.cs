namespace SEVPMS.Application.Interfaces.Providers;

public interface IStorageProvider
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
}
