using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.Handlers;
using BitcoinPayments.Application.Services;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Handlers;

public sealed class AuthHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUtxoRepository> _utxoRepository = new();
    private readonly Mock<IEscrowRepository> _escrowRepository = new();
    private readonly Mock<IBitcoinNetwork> _bitcoinNetwork = new();
    private readonly Mock<IWalletKeyService> _walletKeyService = new();
    private readonly Mock<IBitcoinSettings> _settings = new();

    private AuthHandler CreateSut()
    {
        var coinSelectionService = new CoinSelectionService();
        var feeEstimationService = new FeeEstimationService(_settings.Object);
        var walletSyncService = new WalletSynchronizationService(
            _walletKeyService.Object, _bitcoinNetwork.Object, _utxoRepository.Object);
        var escrowService = new EscrowService(
            coinSelectionService, feeEstimationService, _walletKeyService.Object);

        return new AuthHandler(
            _walletRepository.Object,
            _transactionRepository.Object,
            _utxoRepository.Object,
            _escrowRepository.Object,
            _bitcoinNetwork.Object,
            walletSyncService,
            escrowService,
            _settings.Object);
    }

    [Fact]
    public async Task Handle_BuyerWalletNotFound_Throws()
    {
        var buyerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();

        _walletRepository
            .Setup(r => r.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var command = new CreateAuthCommand(
            buyerId, merchantId, 10_000, null, null, null, "idem-auth-1");

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MerchantWalletNotFound_Throws()
    {
        var buyerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var buyerWallet = new Wallet("buyer", "encrypted", "regtest") { Id = buyerId };

        _walletRepository
            .Setup(r => r.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyerWallet);

        _walletRepository
            .Setup(r => r.GetByIdAsync(merchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var command = new CreateAuthCommand(
            buyerId, merchantId, 10_000, null, null, null, "idem-auth-2");

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }
}
