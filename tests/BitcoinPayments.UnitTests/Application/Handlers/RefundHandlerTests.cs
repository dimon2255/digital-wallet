using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.Handlers;
using BitcoinPayments.Application.Services;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Handlers;

public sealed class RefundHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUtxoRepository> _utxoRepository = new();
    private readonly Mock<IBitcoinNetwork> _bitcoinNetwork = new();
    private readonly Mock<IWalletKeyService> _walletKeyService = new();
    private readonly Mock<IBitcoinSettings> _settings = new();

    private RefundHandler CreateSut()
    {
        var coinSelectionService = new CoinSelectionService();
        var feeEstimationService = new FeeEstimationService(_settings.Object);
        var transactionBuilderService = new TransactionBuilderService(
            coinSelectionService, feeEstimationService, _walletKeyService.Object);
        var walletSyncService = new WalletSynchronizationService(
            _walletKeyService.Object, _bitcoinNetwork.Object, _utxoRepository.Object);

        return new RefundHandler(
            _walletRepository.Object,
            _transactionRepository.Object,
            _utxoRepository.Object,
            _bitcoinNetwork.Object,
            walletSyncService,
            transactionBuilderService,
            _settings.Object);
    }

    [Fact]
    public async Task Handle_ParentTransactionNotFound_Throws()
    {
        var parentId = Guid.NewGuid();

        _transactionRepository
            .Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTransaction?)null);

        var command = new CreateRefundCommand(parentId, 5_000, null, null, "idem-refund-1");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ParentNotSettled_Throws()
    {
        var parent = new PaymentTransaction(
            "idem-parent",
            PaymentOperationType.Charge,
            Money.FromSatoshis(20_000),
            Guid.NewGuid(),
            Guid.NewGuid());
        parent.State = TransactionState.Mempool;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parent);

        var command = new CreateRefundCommand(parent.Id, 5_000, null, null, "idem-refund-2");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RefundExceedsOriginalAmount_Throws()
    {
        var buyerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();

        var parent = new PaymentTransaction(
            "idem-parent",
            PaymentOperationType.Charge,
            Money.FromSatoshis(20_000),
            buyerId,
            merchantId);
        parent.State = TransactionState.Settled;

        var existingRefund = new PaymentTransaction(
            "idem-prev-refund",
            PaymentOperationType.Refund,
            Money.FromSatoshis(15_000),
            buyerId,
            merchantId);

        _transactionRepository
            .Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parent);

        _transactionRepository
            .Setup(r => r.GetChildrenAsync(parent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingRefund });

        var command = new CreateRefundCommand(parent.Id, 10_000, null, null, "idem-refund-3");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }
}
