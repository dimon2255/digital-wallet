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

public sealed class VoidHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUtxoRepository> _utxoRepository = new();
    private readonly Mock<IEscrowRepository> _escrowRepository = new();
    private readonly Mock<IBitcoinNetwork> _bitcoinNetwork = new();
    private readonly Mock<IWalletKeyService> _walletKeyService = new();
    private readonly Mock<IBitcoinSettings> _settings = new();

    private VoidHandler CreateSut()
    {
        var coinSelectionService = new CoinSelectionService();
        var feeEstimationService = new FeeEstimationService(_settings.Object);
        var escrowService = new EscrowService(
            coinSelectionService, feeEstimationService, _walletKeyService.Object);

        return new VoidHandler(
            _walletRepository.Object,
            _transactionRepository.Object,
            _utxoRepository.Object,
            _escrowRepository.Object,
            _bitcoinNetwork.Object,
            escrowService,
            _settings.Object);
    }

    private static PaymentTransaction CreateAuth()
    {
        return new PaymentTransaction(
            "idem-auth",
            PaymentOperationType.Auth,
            Money.FromSatoshis(10_000),
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_AuthorizationNotFound_Throws()
    {
        var authId = Guid.NewGuid();

        _transactionRepository
            .Setup(r => r.GetByIdAsync(authId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTransaction?)null);

        var command = new VoidAuthCommand(authId, null, null, "idem-void-1");
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

        var command = new VoidAuthCommand(charge.Id, null, null, "idem-void-2");
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

        var command = new VoidAuthCommand(auth.Id, null, null, "idem-void-3");
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

        var command = new VoidAuthCommand(auth.Id, null, null, "idem-void-4");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyVoided_Throws()
    {
        var auth = CreateAuth();
        auth.State = TransactionState.Voided;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(auth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var command = new VoidAuthCommand(auth.Id, null, null, "idem-void-5");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyExpired_Throws()
    {
        var auth = CreateAuth();
        auth.State = TransactionState.Expired;

        _transactionRepository
            .Setup(r => r.GetByIdAsync(auth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var command = new VoidAuthCommand(auth.Id, null, null, "idem-void-6");
        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }
}
