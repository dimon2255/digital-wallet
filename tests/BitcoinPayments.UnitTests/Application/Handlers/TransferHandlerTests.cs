using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.Handlers;
using BitcoinPayments.Application.Services.Transfers;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Handlers;

public sealed class TransferHandlerTests
{
    private readonly Mock<IWalletRepository> walletRepo = new();
    private readonly Mock<ITransactionRepository> txRepo = new();
    private readonly Mock<IBitcoinSettings> settings = new();

    public TransferHandlerTests()
    {
        settings.Setup(s => s.TransferMode).Returns("auto");
        settings.Setup(s => s.BuildExplorerUrl(It.IsAny<string?>())).Returns((string?)null);
    }

    private TransferHandler CreateHandler()
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(InternalLedgerStrategy))).Returns(new InternalLedgerStrategy(txRepo.Object));
        var resolver = new TransferStrategyResolver(settings.Object, sp.Object);
        return new TransferHandler(walletRepo.Object, txRepo.Object, resolver, settings.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenSourceWalletNotOwned()
    {
        var sourceWallet = new Wallet("Source", "key", "testnet4") { UserId = "other-user" };
        walletRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(sourceWallet);
        txRepo.Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PaymentTransaction?)null);

        var handler = CreateHandler();
        var command = new CreateTransferCommand(sourceWallet.Id, Guid.NewGuid(), 1000, null, "key-1", "my-user");

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnExisting_WhenIdempotencyKeyMatches()
    {
        var existing = new PaymentTransaction("key-1", PaymentOperationType.Transfer, Money.FromSatoshis(1000), Guid.NewGuid(), Guid.NewGuid())
        {
            State = TransactionState.TransferCompleted,
        };
        txRepo.Setup(r => r.GetByIdempotencyKeyAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = CreateHandler();
        var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), 1000, null, "key-1", "user");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("transfercompleted", result.State);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSourceWalletNotFound()
    {
        walletRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Wallet?)null);
        txRepo.Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PaymentTransaction?)null);

        var handler = CreateHandler();
        var command = new CreateTransferCommand(Guid.NewGuid(), Guid.NewGuid(), 1000, null, "key-1", "user");

        await Assert.ThrowsAsync<InvalidOperationStateException>(() => handler.Handle(command, CancellationToken.None));
    }
}
