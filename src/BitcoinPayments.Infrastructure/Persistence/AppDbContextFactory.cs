using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BitcoinPayments.Infrastructure.Persistence;

/// <summary>
/// Creates the application DbContext for EF Core design-time tooling.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("BITCOIN_PAYMENTS_CONNECTION_STRING")
                               ?? "Host=localhost;Port=5432;Database=btc-payments;Username=postgres;Password=password";

        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
