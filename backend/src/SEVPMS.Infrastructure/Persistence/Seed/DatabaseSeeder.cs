namespace SEVPMS.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static Task SeedAsync(SEVPMSDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Add controlled academic/demo seed data here later.
        return Task.CompletedTask;
    }
}
