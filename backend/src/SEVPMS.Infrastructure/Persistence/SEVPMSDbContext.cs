using Microsoft.EntityFrameworkCore;

namespace SEVPMS.Infrastructure.Persistence;

public sealed class SEVPMSDbContext(DbContextOptions<SEVPMSDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SEVPMSDbContext).Assembly);
    }
}
