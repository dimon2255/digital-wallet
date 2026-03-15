using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Services.Transfers;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Services;

public sealed class InternalLedgerStrategyTests
{
    [Fact]
    public async Task Execute_ShouldCreateTransferCompletedTransaction()
    {
        var txRepo = new Mock<ITransactionRepository>();
        txRepo.Setup(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var strategy = new InternalLedgerStrategy(txRepo.Object);

        var source = new Wallet("Source", "k", "testnet4") { UserId = "user-1" };
        var dest = new Wallet("Dest", "k", "testnet4") { UserId = "user-1" };
        var context = new TransferContext(source, dest, 5000, null, "key-1");

        var result = await strategy.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(TransactionState.TransferCompleted, result.State);
        Assert.Equal(PaymentOperationType.Transfer, result.OperationType);
        Assert.Equal(5000L, result.Amount.Satoshis);
        Assert.NotNull(result.Fee);
        Assert.Equal(0L, result.Fee!.Value.Satoshis);
        Assert.Null(result.BitcoinTxId);

        txRepo.Verify(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
