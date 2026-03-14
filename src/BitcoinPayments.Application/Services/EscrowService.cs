using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.ValueObjects;
using NBitcoin;
using DomainBitcoinAddress = BitcoinPayments.Domain.ValueObjects.BitcoinAddress;
using DomainMoney = BitcoinPayments.Domain.ValueObjects.Money;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Builds escrow-based authorization, capture, and void transactions.
/// </summary>
public sealed class EscrowService
{
    private readonly CoinSelectionService coinSelectionService;
    private readonly FeeEstimationService feeEstimationService;
    private readonly IWalletKeyService walletKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EscrowService"/> class.
    /// </summary>
    public EscrowService(
        CoinSelectionService coinSelectionService,
        FeeEstimationService feeEstimationService,
        IWalletKeyService walletKeyService)
    {
        this.coinSelectionService = coinSelectionService;
        this.feeEstimationService = feeEstimationService;
        this.walletKeyService = walletKeyService;
    }

    /// <summary>
    /// Builds an authorization funding transaction and escrow record.
    /// </summary>
    public AuthorizationBuildResult BuildAuthorization(
        Wallet buyerWallet,
        Wallet merchantWallet,
        IReadOnlyCollection<Utxo> availableUtxos,
        DomainMoney amount,
        int authWindowBlocks,
        int? feeRateSatPerByte,
        long currentBlockHeight)
    {
        var network = TransactionBuilderService.ResolveNetwork(buyerWallet.Network);
        var feeRateValue = feeEstimationService.GetFeeRateSatPerByte(feeRateSatPerByte);
        var selectedUtxos = coinSelectionService.SelectLargestFirst(availableUtxos, amount, feeRateValue, outputCount: 2);

        var buyerEscrowIndex = buyerWallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var merchantEscrowIndex = merchantWallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var buyerChangeIndex = buyerWallet.ReserveNextIndex(WalletAddressPurpose.Change);

        var buyerEscrowAddress = walletKeyService.DeriveAddress(buyerWallet, WalletAddressPurpose.Receiving, buyerEscrowIndex);
        var merchantEscrowAddress = walletKeyService.DeriveAddress(merchantWallet, WalletAddressPurpose.Receiving, merchantEscrowIndex);
        var buyerChangeAddress = walletKeyService.DeriveAddress(buyerWallet, WalletAddressPurpose.Change, buyerChangeIndex);

        var buyerPubKey = walletKeyService.GetPrivateKey(buyerWallet, WalletAddressPurpose.Receiving, buyerEscrowIndex).PubKey;
        var merchantPubKey = walletKeyService.GetPrivateKey(merchantWallet, WalletAddressPurpose.Receiving, merchantEscrowIndex).PubKey;

        var redeemScript = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, new[] { buyerPubKey, merchantPubKey });
        var escrowAddress = redeemScript.WitHash.GetAddress(network);

        var builder = network.CreateTransactionBuilder();
        var coins = selectedUtxos
            .Select(utxo => new Coin(
                utxo.TransactionId.ToUInt256(),
                (uint)utxo.OutputIndex,
                utxo.Amount.ToNBitcoin(),
                Script.FromHex(utxo.ScriptPubKey)))
            .ToArray();

        var keys = selectedUtxos
            .Select(utxo => walletKeyService.GetPrivateKey(
                buyerWallet,
                utxo.IsChangeAddress ? WalletAddressPurpose.Change : WalletAddressPurpose.Receiving,
                utxo.DerivationIndex))
            .ToArray();

        builder.AddCoins(coins);
        builder.AddKeys(keys);
        builder.Send(escrowAddress, amount.ToNBitcoin());
        builder.SetChange(buyerChangeAddress.Address.ToNBitcoin(network));
        builder.SendEstimatedFees(feeEstimationService.GetFeeRate(feeRateSatPerByte));

        var transaction = builder.BuildTransaction(sign: true);
        if (!builder.Verify(transaction))
        {
            throw new InvalidOperationStateException("Built authorization funding transaction failed verification.");
        }

        var totalInput = selectedUtxos.Aggregate(DomainMoney.Zero, static (current, utxo) => current + utxo.Amount);
        var totalOutput = transaction.Outputs.Aggregate(DomainMoney.Zero, static (current, output) => current + DomainMoney.FromSatoshis(output.Value.Satoshi));
        var fee = totalInput - totalOutput;
        var transactionId = TransactionId.Parse(transaction.GetHash().ToString());

        var trackedOutputs = new List<TrackedOutput>();
        for (var outputIndex = 0; outputIndex < transaction.Outputs.Count; outputIndex++)
        {
            var output = transaction.Outputs[outputIndex];
            var destinationAddress = output.ScriptPubKey.GetDestinationAddress(network)?.ToString();
            if (string.Equals(destinationAddress, escrowAddress.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                trackedOutputs.Add(
                    new TrackedOutput(
                        buyerWallet.Id,
                        DomainBitcoinAddress.Parse(escrowAddress.ToString()),
                        DomainMoney.FromSatoshis(output.Value.Satoshi),
                        output.ScriptPubKey.ToHex(),
                        outputIndex,
                        IsChange: false,
                        IsEscrow: true,
                        DerivationIndex: buyerEscrowIndex));
            }
            else if (string.Equals(destinationAddress, buyerChangeAddress.Address.Value, StringComparison.OrdinalIgnoreCase))
            {
                trackedOutputs.Add(
                    new TrackedOutput(
                        buyerWallet.Id,
                        buyerChangeAddress.Address,
                        DomainMoney.FromSatoshis(output.Value.Satoshi),
                        output.ScriptPubKey.ToHex(),
                        outputIndex,
                        IsChange: true,
                        IsEscrow: false,
                        DerivationIndex: buyerChangeAddress.Index));
            }
        }

        var escrow = new Escrow
        {
            EscrowAddress = DomainBitcoinAddress.Parse(escrowAddress.ToString()),
            RedeemScriptHex = redeemScript.ToHex(),
            BuyerPublicKeyHex = buyerEscrowAddress.PublicKeyHex,
            MerchantPublicKeyHex = merchantEscrowAddress.PublicKeyHex,
            TimelockBlocks = authWindowBlocks,
            BuyerKeyIndex = buyerEscrowIndex,
            MerchantKeyIndex = merchantEscrowIndex,
            BuyerChangeIndex = buyerChangeIndex,
            ExpiresAtBlock = currentBlockHeight + authWindowBlocks,
            TimelockRefundTransactionHex = null,
            State = EscrowState.Created,
        };

        return new AuthorizationBuildResult(
            new BuiltTransaction(transaction.ToHex(), transactionId, fee, selectedUtxos, trackedOutputs),
            escrow);
    }

    /// <summary>
    /// Builds a capture transaction that spends the escrow output to the merchant.
    /// </summary>
    public BuiltTransaction BuildCapture(
        Wallet buyerWallet,
        Wallet merchantWallet,
        Escrow escrow,
        Utxo escrowUtxo,
        DomainMoney captureAmount,
        int? feeRateSatPerByte)
    {
        if (captureAmount > escrowUtxo.Amount)
        {
            throw new InvalidOperationStateException("Capture amount exceeds the escrow amount.");
        }

        var network = TransactionBuilderService.ResolveNetwork(buyerWallet.Network);
        var merchantOutputIndex = merchantWallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var buyerChangeIndex = buyerWallet.ReserveNextIndex(WalletAddressPurpose.Change);
        var merchantAddress = walletKeyService.DeriveAddress(merchantWallet, WalletAddressPurpose.Receiving, merchantOutputIndex);
        var buyerChangeAddress = walletKeyService.DeriveAddress(buyerWallet, WalletAddressPurpose.Change, buyerChangeIndex);

        var redeemScript = Script.FromHex(escrow.RedeemScriptHex);
        var escrowCoin = new Coin(
                escrowUtxo.TransactionId.ToUInt256(),
                (uint)escrowUtxo.OutputIndex,
                escrowUtxo.Amount.ToNBitcoin(),
                redeemScript.WitHash.ScriptPubKey)
            .ToScriptCoin(redeemScript);

        var buyerKey = walletKeyService.GetPrivateKey(buyerWallet, WalletAddressPurpose.Receiving, escrow.BuyerKeyIndex);
        var merchantKey = walletKeyService.GetPrivateKey(merchantWallet, WalletAddressPurpose.Receiving, escrow.MerchantKeyIndex);

        var builder = network.CreateTransactionBuilder();
        builder.AddCoins(escrowCoin);
        builder.AddKeys(buyerKey, merchantKey);
        builder.Send(merchantAddress.Address.ToNBitcoin(network), captureAmount.ToNBitcoin());

        var feeRate = feeEstimationService.GetFeeRate(feeRateSatPerByte);
        builder.SendEstimatedFees(feeRate);

        if (escrowUtxo.Amount - captureAmount > DomainMoney.FromSatoshis(546L))
        {
            builder.SetChange(buyerChangeAddress.Address.ToNBitcoin(network));
        }

        var transaction = builder.BuildTransaction(sign: true);
        if (!builder.Verify(transaction))
        {
            throw new InvalidOperationStateException("Built capture transaction failed verification.");
        }

        var totalOutput = transaction.Outputs.Aggregate(DomainMoney.Zero, static (current, output) => current + DomainMoney.FromSatoshis(output.Value.Satoshi));
        var fee = escrowUtxo.Amount - totalOutput;

        var outputs = new List<TrackedOutput>();
        for (var outputIndex = 0; outputIndex < transaction.Outputs.Count; outputIndex++)
        {
            var output = transaction.Outputs[outputIndex];
            var destinationAddress = output.ScriptPubKey.GetDestinationAddress(network)?.ToString();
            if (string.Equals(destinationAddress, merchantAddress.Address.Value, StringComparison.OrdinalIgnoreCase))
            {
                outputs.Add(
                    new TrackedOutput(
                        merchantWallet.Id,
                        merchantAddress.Address,
                        DomainMoney.FromSatoshis(output.Value.Satoshi),
                        output.ScriptPubKey.ToHex(),
                        outputIndex,
                        IsChange: false,
                        IsEscrow: false,
                        DerivationIndex: merchantAddress.Index));
            }
            else if (string.Equals(destinationAddress, buyerChangeAddress.Address.Value, StringComparison.OrdinalIgnoreCase))
            {
                outputs.Add(
                    new TrackedOutput(
                        buyerWallet.Id,
                        buyerChangeAddress.Address,
                        DomainMoney.FromSatoshis(output.Value.Satoshi),
                        output.ScriptPubKey.ToHex(),
                        outputIndex,
                        IsChange: true,
                        IsEscrow: false,
                        DerivationIndex: buyerChangeAddress.Index));
            }
        }

        return new BuiltTransaction(
            transaction.ToHex(),
            TransactionId.Parse(transaction.GetHash().ToString()),
            fee,
            new[] { escrowUtxo },
            outputs);
    }

    /// <summary>
    /// Builds a cooperative void transaction that returns escrow funds to the buyer.
    /// </summary>
    public BuiltTransaction BuildVoid(
        Wallet buyerWallet,
        Wallet merchantWallet,
        Escrow escrow,
        Utxo escrowUtxo,
        int? feeRateSatPerByte)
    {
        var network = TransactionBuilderService.ResolveNetwork(buyerWallet.Network);
        var buyerOutputIndex = buyerWallet.ReserveNextIndex(WalletAddressPurpose.Change);
        var buyerAddress = walletKeyService.DeriveAddress(buyerWallet, WalletAddressPurpose.Change, buyerOutputIndex);

        var redeemScript = Script.FromHex(escrow.RedeemScriptHex);
        var escrowCoin = new Coin(
                escrowUtxo.TransactionId.ToUInt256(),
                (uint)escrowUtxo.OutputIndex,
                escrowUtxo.Amount.ToNBitcoin(),
                redeemScript.WitHash.ScriptPubKey)
            .ToScriptCoin(redeemScript);

        var buyerKey = walletKeyService.GetPrivateKey(buyerWallet, WalletAddressPurpose.Receiving, escrow.BuyerKeyIndex);
        var merchantKey = walletKeyService.GetPrivateKey(merchantWallet, WalletAddressPurpose.Receiving, escrow.MerchantKeyIndex);

        var builder = network.CreateTransactionBuilder();
        builder.AddCoins(escrowCoin);
        builder.AddKeys(buyerKey, merchantKey);
        builder.SendAll(buyerAddress.Address.ToNBitcoin(network));
        builder.SubtractFees();
        builder.SendEstimatedFees(feeEstimationService.GetFeeRate(feeRateSatPerByte));

        var transaction = builder.BuildTransaction(sign: true);
        if (!builder.Verify(transaction))
        {
            throw new InvalidOperationStateException("Built void transaction failed verification.");
        }

        var totalOutput = transaction.Outputs.Aggregate(DomainMoney.Zero, static (current, output) => current + DomainMoney.FromSatoshis(output.Value.Satoshi));
        var fee = escrowUtxo.Amount - totalOutput;

        return new BuiltTransaction(
            transaction.ToHex(),
            TransactionId.Parse(transaction.GetHash().ToString()),
            fee,
            new[] { escrowUtxo },
            new[]
            {
                new TrackedOutput(
                    buyerWallet.Id,
                    buyerAddress.Address,
                    DomainMoney.FromSatoshis(transaction.Outputs[0].Value.Satoshi),
                    transaction.Outputs[0].ScriptPubKey.ToHex(),
                    0,
                    IsChange: true,
                    IsEscrow: false,
                    buyerAddress.Index),
            });
    }
}

/// <summary>
/// Represents the result of building an authorization flow.
/// </summary>
/// <param name="FundingTransaction">The funding transaction.</param>
/// <param name="Escrow">The escrow record.</param>
public sealed record AuthorizationBuildResult(BuiltTransaction FundingTransaction, Escrow Escrow);
