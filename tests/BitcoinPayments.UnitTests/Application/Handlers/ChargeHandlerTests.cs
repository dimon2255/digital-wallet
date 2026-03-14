using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.Handlers;
using BitcoinPayments.Application.Services;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Handlers;

public sealed class ChargeHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUtxoRepository> _utxoRepository = new();
    private readonly Mock<IBitcoinNetwork> _bitcoinNetwork = new();
    private readonly Mock<IWalletKeyService> _walletKeyService = new();
    private readonly Mock<IBitcoinSettings> _settings = new();

    private ChargeHandler CreateSut()
    {
        var coinSelectionService = new CoinSelectionService();
        var feeEstimationService = new FeeEstimationService(_settings.Object);
        var transactionBuilderService = new TransactionBuilderService(
            coinSelectionService, feeEstimationService, _walletKeyService.Object);
        var walletSyncService = new WalletSynchronizationService(
            _walletKeyService.Object, _bitcoinNetwork.Object, _utxoRepository.Object);

        return new ChargeHandler(
            _walletRepository.Object,
            _transactionRepository.Object,
            _utxoRepository.Object,
            _bitcoinNetwork.Object,
            transactionBuilderService,
            walletSyncService,
            _settings.Object);
    }

    [Fact]
    public async Task Handle_BuyerWalletNotFound_ThrowsInvalidOperationStateException()
    {
        var buyerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();

        _walletRepository
            .Setup(r => r.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        var command = new CreateChargeCommand(
            buyerId, merchantId, 10_000, null, null, "idem-charge-1");

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MerchantWalletNotFound_ThrowsInvalidOperationStateException()
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

        var command = new CreateChargeCommand(
            buyerId, merchantId, 10_000, null, null, "idem-charge-2");

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationStateException>(
            async () => await sut.Handle(command, CancellationToken.None));
    }
}
