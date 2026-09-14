namespace SEVPMS.Application.Interfaces.Providers;

public interface IApplicationLinkBuilder
{
    string PasswordReset(string token);
}
