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
/// Builds and signs standard Bitcoin transactions.
/// </summary>
public sealed class TransactionBuilderService
{
    private const long DustLimitSatoshis = 546L;

    private readonly CoinSelectionService coinSelectionService;
    private readonly FeeEstimationService feeEstimationService;
    private readonly IWalletKeyService walletKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBuilderService"/> class.
    /// </summary>
    public TransactionBuilderService(
        CoinSelectionService coinSelectionService,
        FeeEstimationService feeEstimationService,
        IWalletKeyService walletKeyService)
    {
        this.coinSelectionService = coinSelectionService;
        this.feeEstimationService = feeEstimationService;
        this.walletKeyService = walletKeyService;
    }

    /// <summary>
    /// Builds a direct charge transaction from the buyer wallet to the merchant wallet.
    /// </summary>
    public BuiltTransaction BuildCharge(
        Wallet buyerWallet,
        Wallet merchantWallet,
        IReadOnlyCollection<Utxo> availableUtxos,
        DomainMoney amount,
        int? feeRateSatPerByte)
    {
        return BuildStandardPayment(
            sourceWallet: buyerWallet,
            destinationWallet: merchantWallet,
            availableUtxos,
            amount,
            feeRateSatPerByte);
    }

    /// <summary>
    /// Builds a refund transaction from the merchant wallet to the buyer wallet.
    /// </summary>
    public BuiltTransaction BuildRefund(
        Wallet merchantWallet,
        Wallet buyerWallet,
        IReadOnlyCollection<Utxo> availableUtxos,
        DomainMoney amount,
        int? feeRateSatPerByte)
    {
        return BuildStandardPayment(
            sourceWallet: merchantWallet,
            destinationWallet: buyerWallet,
            availableUtxos,
            amount,
            feeRateSatPerByte);
    }

    private BuiltTransaction BuildStandardPayment(
        Wallet sourceWallet,
        Wallet destinationWallet,
        IReadOnlyCollection<Utxo> availableUtxos,
        DomainMoney amount,
        int? feeRateSatPerByte)
    {
        var feeRateValue = feeEstimationService.GetFeeRateSatPerByte(feeRateSatPerByte);
        var selectedUtxos = coinSelectionService.SelectLargestFirst(availableUtxos, amount, feeRateValue, outputCount: 2);
        var network = ResolveNetwork(sourceWallet.Network);

        var destinationIndex = destinationWallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var destinationAddress = walletKeyService.DeriveAddress(
            destinationWallet,
            WalletAddressPurpose.Receiving,
            destinationIndex);

        var changeIndex = sourceWallet.ReserveNextIndex(WalletAddressPurpose.Change);
        var changeAddress = walletKeyService.DeriveAddress(sourceWallet, WalletAddressPurpose.Change, changeIndex);

        var builder = network.CreateTransactionBuilder();
        var coins = selectedUtxos
            .Select(utxo => new Coin(
                fromTxHash: utxo.TransactionId.ToUInt256(),
                fromOutputIndex: (uint)utxo.OutputIndex,
                amount: utxo.Amount.ToNBitcoin(),
                scriptPubKey: Script.FromHex(utxo.ScriptPubKey)))
            .ToArray();

        var keys = selectedUtxos
            .Select(utxo => walletKeyService.GetPrivateKey(
                sourceWallet,
                utxo.IsChangeAddress ? WalletAddressPurpose.Change : WalletAddressPurpose.Receiving,
                utxo.DerivationIndex))
            .ToArray();

        builder.AddCoins(coins);
        builder.AddKeys(keys);
        builder.Send(destinationAddress.Address.ToNBitcoin(network), amount.ToNBitcoin());
        builder.SetChange(changeAddress.Address.ToNBitcoin(network));
        builder.SendEstimatedFees(feeEstimationService.GetFeeRate(feeRateSatPerByte));

        var transaction = builder.BuildTransaction(sign: true);
        if (!builder.Verify(transaction))
        {
            throw new InvalidOperationStateException("Built transaction failed NBitcoin verification.");
        }

        var outputLookup = BuildOutputLookup(
            transaction,
            network,
            new[]
            {
                new TrackedAddress(destinationWallet.Id, destinationAddress, IsEscrow: false),
                new TrackedAddress(sourceWallet.Id, changeAddress, IsEscrow: false),
            });

        var totalInput = selectedUtxos.Aggregate(DomainMoney.Zero, static (current, utxo) => current + utxo.Amount);
        var totalOutput = transaction.Outputs.Aggregate(DomainMoney.Zero, static (current, output) => current + DomainMoney.FromSatoshis(output.Value.Satoshi));
        var fee = totalInput - totalOutput;

        if (fee.Satoshis < 0)
        {
            throw new InvalidOperationStateException("Calculated fee cannot be negative.");
        }

        return new BuiltTransaction(transaction.ToHex(), TransactionId.Parse(transaction.GetHash().ToString()), fee, selectedUtxos, outputLookup);
    }

    private static IReadOnlyCollection<TrackedOutput> BuildOutputLookup(
        Transaction transaction,
        Network network,
        IEnumerable<TrackedAddress> trackedAddresses)
    {
        var trackedByAddress = trackedAddresses.ToDictionary(item => item.Address.Address.Value, StringComparer.OrdinalIgnoreCase);
        var outputs = new List<TrackedOutput>();

        for (var outputIndex = 0; outputIndex < transaction.Outputs.Count; outputIndex++)
        {
            var output = transaction.Outputs[outputIndex];
            var destinationAddress = output.ScriptPubKey.GetDestinationAddress(network);
            if (destinationAddress is null || !trackedByAddress.TryGetValue(destinationAddress.ToString(), out var trackedAddress))
            {
                continue;
            }

            if (output.Value.Satoshi < DustLimitSatoshis)
            {
                continue;
            }

            outputs.Add(
                new TrackedOutput(
                    trackedAddress.WalletId,
                    trackedAddress.Address.Address,
                    DomainMoney.FromSatoshis(output.Value.Satoshi),
                    output.ScriptPubKey.ToHex(),
                    outputIndex,
                    trackedAddress.Address.Purpose == WalletAddressPurpose.Change,
                    trackedAddress.IsEscrow,
                    trackedAddress.Address.Index));
        }

        return outputs;
    }

    internal static Network ResolveNetwork(string networkName)
    {
        return networkName.Trim().ToLowerInvariant() switch
        {
            "testnet4" => Network.GetNetwork("testnet4") ?? Network.TestNet,
            "regtest" => Network.RegTest,
            _ => throw new MainnetGuardException($"Unsupported or unsafe network '{networkName}'."),
        };
    }

    private sealed record TrackedAddress(Guid WalletId, DerivedAddress Address, bool IsEscrow);
}

/// <summary>
/// Represents a built transaction and the tracked state changes it implies.
/// </summary>
/// <param name="RawTransactionHex">The raw transaction hex.</param>
/// <param name="TransactionId">The computed transaction id.</param>
/// <param name="Fee">The paid fee.</param>
/// <param name="ConsumedUtxos">The spent UTXOs.</param>
/// <param name="Outputs">The tracked outputs created by the transaction.</param>
public sealed record BuiltTransaction(
    string RawTransactionHex,
    TransactionId TransactionId,
    DomainMoney Fee,
    IReadOnlyCollection<Utxo> ConsumedUtxos,
    IReadOnlyCollection<TrackedOutput> Outputs);

/// <summary>
/// Represents a transaction output that should be tracked in the database.
/// </summary>
/// <param name="WalletId">The owning wallet identifier.</param>
/// <param name="Address">The destination address.</param>
/// <param name="Amount">The amount.</param>
/// <param name="ScriptPubKeyHex">The script pubkey hex.</param>
/// <param name="OutputIndex">The output index.</param>
/// <param name="IsChange">Whether the output is change.</param>
/// <param name="IsEscrow">Whether the output belongs to escrow.</param>
/// <param name="DerivationIndex">The derivation index.</param>
public sealed record TrackedOutput(
    Guid WalletId,
    DomainBitcoinAddress Address,
    DomainMoney Amount,
    string ScriptPubKeyHex,
    int OutputIndex,
    bool IsChange,
    bool IsEscrow,
    int DerivationIndex);
