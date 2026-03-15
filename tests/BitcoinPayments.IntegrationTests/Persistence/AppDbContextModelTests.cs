using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Infrastructure.Identity;
using BitcoinPayments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.IntegrationTests.Persistence;

public sealed class AppDbContextModelTests
{
    [Fact]
    public void AppDbContext_ShouldExposeCoreEntitySets()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        using var dbContext = new AppDbContext(options);

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Wallet)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(PaymentTransaction)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Escrow)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Utxo)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(IdempotencyKeyRecord)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(ApplicationUser)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(RefreshToken)));
    }
}
