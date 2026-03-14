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

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(BitcoinPayments.Domain.Entities.Wallet)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(BitcoinPayments.Domain.Entities.PaymentTransaction)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(BitcoinPayments.Domain.Entities.Escrow)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(BitcoinPayments.Domain.Entities.Utxo)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(BitcoinPayments.Domain.Entities.IdempotencyKeyRecord)));
    }
}
