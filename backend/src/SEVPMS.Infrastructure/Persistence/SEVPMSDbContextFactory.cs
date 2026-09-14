using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SEVPMS.Infrastructure.Persistence;

public sealed class SEVPMSDbContextFactory
    : IDesignTimeDbContextFactory<SEVPMSDbContext>
{
    public SEVPMSDbContext CreateDbContext(
        string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SEVPMSDbContext>();

        const string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;" +
            "Database=SEVPMSDb;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;" +
            "MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(
            connectionString);

        return new SEVPMSDbContext(
            optionsBuilder.Options);
    }
}