using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers;

public sealed class ApplicationLinkBuilder(IConfiguration configuration) : IApplicationLinkBuilder
{
    public string PasswordReset(string token)
    {
        var baseUrl = configuration["App:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("App:FrontendBaseUrl is not configured.");

        return $"{baseUrl.TrimEnd('/')}/password-reset?token={Uri.EscapeDataString(token)}";
    }
}
