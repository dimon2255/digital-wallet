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

public sealed class CaptureHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUtxoRepository> _utxoRepository = new();
    private readonly Mock<IEscrowRepository> _escrowRepository = new();
    private readonly Mock<IBitcoinNetwork> _bitcoinNetwork = new();
    private readonly Mock<IWalletKeyService> _walletKeyService = new();
    private readonly Mock<IBitcoinSettings> _settings = new();

    private CaptureHandler CreateSut()
    {
        var coinSelectionService = new CoinSelectionService();
        var feeEstimationService = new FeeEstimationService(_settings.Object);
        var escrowService = new EscrowService(
            coinSelectionService, feeEstimationService, _walletKeyService.Object);

        return new CaptureHandler(
            _walletRepository.Object,
            _transactionRepository.Object,
            _utxoRepository.Object,
            _escrowRepository.Object,
            _bitcoinNetwork.Object,
            escrowService,
            _settings.Object);
    }

    private static PaymentTransaction CreateAuth(Guid? buyerId = null, Guid? merchantId = null)
    {
        return new PaymentTransaction(
            "idem-auth",
            PaymentOperationType.Auth,
            Money.FromSatoshis(10_000),
            buyerId ?? Guid.NewGuid(),
            merchantId ?? Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_AuthorizationNotFound_Throws()
    {
        var authId = Guid.NewGuid();

        _transactionRepository
            .Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTransaction?)null);

        var command = new CaptureAuthCommand(authId, 5_000, null, null, "idem-cap-1");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NotAnAuthorization_Throws()
    {
        var charge = new PaymentTransaction(
            "idem-charge",
            PaymentOperationType.Charge,
            Money.FromSatoshis(10_000),
            Guid.NewGuid(),
            Guid.NewGuid());

        _transactionRepository
            .Setup(r => r.GetByIdAsync(charge.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(charge);

        var command = new CaptureAuthCommand(charge.Id, 5_000, null, null, "idem-cap-2");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyCaptured_Throws()
    {
        var auth = CreateAuth();
        auth.State = TransactionState.CaptureBroadcast;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(auth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var command = new CaptureAuthCommand(auth.Id, 5_000, null, null, "idem-cap-3");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadySettled_Throws()
    {
        var auth = CreateAuth();
        auth.State = TransactionState.Settled;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(auth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var command = new CaptureAuthCommand(auth.Id, 5_000, null, null, "idem-cap-4");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NotActive_Throws()
    {
        var auth = CreateAuth();
        auth.State = TransactionState.Failed;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(auth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var command = new CaptureAuthCommand(auth.Id, 5_000, null, null, "idem-cap-5");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }
}
