namespace SEVPMS.Application.Interfaces.Providers;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
