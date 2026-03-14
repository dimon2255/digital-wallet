using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;
using BitcoinPayments.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;
using DomainBitcoinAddress = BitcoinPayments.Domain.ValueObjects.BitcoinAddress;
using DomainMoney = BitcoinPayments.Domain.ValueObjects.Money;

namespace BitcoinPayments.Infrastructure.Bitcoin;

/// <summary>
/// Implements Bitcoin network operations against testnet4 and regtest.
/// </summary>
public sealed class NBitcoinNetworkService : IBitcoinNetwork
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly BitcoinOptions options;
    private readonly TestnetBroadcaster testnetBroadcaster;
    private readonly ILogger<NBitcoinNetworkService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NBitcoinNetworkService"/> class.
    /// </summary>
    public NBitcoinNetworkService(
        IHttpClientFactory httpClientFactory,
        IOptions<BitcoinOptions> options,
        TestnetBroadcaster testnetBroadcaster,
        ILogger<NBitcoinNetworkService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.testnetBroadcaster = testnetBroadcaster;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<BroadcastResult> BroadcastTransactionAsync(string rawTransactionHex, CancellationToken cancellationToken)
    {
        var txid = IsRegtest()
            ? (await CreateRpcClient().SendCommandAsync("sendrawtransaction", rawTransactionHex).ConfigureAwait(false)).ResultString
            : await testnetBroadcaster.BroadcastAsync(rawTransactionHex, cancellationToken);

        logger.LogInformation("Broadcast transaction {TransactionId} to {Network}.", txid, options.Network);
        return new BroadcastResult(TransactionId.Parse(txid), BuildExplorerUrl(txid));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ObservedUtxo>> GetAddressUtxosAsync(
        IReadOnlyCollection<DomainBitcoinAddress> addresses,
        CancellationToken cancellationToken)
    {
        if (addresses.Count == 0)
        {
            return Array.Empty<ObservedUtxo>();
        }

        return IsRegtest()
            ? await GetRegtestUtxosAsync(addresses, cancellationToken)
            : await GetTestnetUtxosAsync(addresses, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NetworkTransactionStatus> GetTransactionStatusAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken)
    {
        if (IsRegtest())
        {
            var result = await CreateRpcClient().SendCommandAsync("getrawtransaction", transactionId.Value, true).ConfigureAwait(false);
            var json = result.Result;
            var regtestConfirmations = json["confirmations"] is null ? 0 : int.Parse(json["confirmations"]!.ToString(), CultureInfo.InvariantCulture);
            return new NetworkTransactionStatus(transactionId, regtestConfirmations == 0, regtestConfirmations, regtestConfirmations > 0);
        }

        using var httpClient = httpClientFactory.CreateClient(nameof(NBitcoinNetworkService));
        var status = await httpClient.GetFromJsonAsync<TestnetTransactionStatusDto>(
            $"tx/{transactionId.Value}/status",
            cancellationToken);

        if (status is null)
        {
            return new NetworkTransactionStatus(transactionId, false, 0, false);
        }

        var currentHeight = await GetCurrentBlockHeightAsync(cancellationToken);
        var confirmations = status.confirmed && status.block_height.HasValue
            ? (int)Math.Max(0, currentHeight - status.block_height.Value + 1)
            : 0;

        return new NetworkTransactionStatus(transactionId, !status.confirmed, confirmations, status.confirmed);
    }

    /// <inheritdoc />
    public async Task<long> GetCurrentBlockHeightAsync(CancellationToken cancellationToken)
    {
        if (IsRegtest())
        {
            var response = await CreateRpcClient().SendCommandAsync("getblockcount").ConfigureAwait(false);
            return long.Parse(response.Result.ToString(), CultureInfo.InvariantCulture);
        }

        using var httpClient = httpClientFactory.CreateClient(nameof(NBitcoinNetworkService));
        var heightText = await httpClient.GetStringAsync("blocks/tip/height", cancellationToken);
        return long.Parse(heightText, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public async Task<FundingResult> FundAddressAsync(DomainBitcoinAddress address, DomainMoney amount, CancellationToken cancellationToken)
    {
        if (!IsRegtest())
        {
            return new FundingResult(
                null,
                $"Use a Testnet4 faucet to fund {address.Value}. Suggested amount: {amount.Satoshis} sat.");
        }

        await CreateRpcClient().SendCommandAsync("generatetoaddress", 101, address.Value).ConfigureAwait(false);
        logger.LogInformation("Generated regtest blocks to fund address {Address}.", address.Value);
        return new FundingResult(null, $"Generated 101 regtest blocks to fund {address.Value}.");
    }

    private async Task<IReadOnlyCollection<ObservedUtxo>> GetTestnetUtxosAsync(
        IReadOnlyCollection<DomainBitcoinAddress> addresses,
        CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(nameof(NBitcoinNetworkService));
        var currentHeight = await GetCurrentBlockHeightAsync(cancellationToken);
        var network = ResolveNetwork();
        var observed = new List<ObservedUtxo>();

        foreach (var address in addresses)
        {
            var items = await httpClient.GetFromJsonAsync<TestnetUtxoDto[]>($"address/{address.Value}/utxo", cancellationToken)
                        ?? Array.Empty<TestnetUtxoDto>();

            foreach (var item in items)
            {
                var confirmations = item.status.confirmed && item.status.block_height.HasValue
                    ? (int)Math.Max(0, currentHeight - item.status.block_height.Value + 1)
                    : 0;

                observed.Add(
                    new ObservedUtxo(
                        TransactionId.Parse(item.txid),
                        item.vout,
                        DomainMoney.FromSatoshis(item.value),
                        address.ToNBitcoin(network).ScriptPubKey.ToHex(),
                        address,
                        confirmations));
            }
        }

        return observed;
    }

    private async Task<IReadOnlyCollection<ObservedUtxo>> GetRegtestUtxosAsync(
        IReadOnlyCollection<DomainBitcoinAddress> addresses,
        CancellationToken cancellationToken)
    {
        var network = ResolveNetwork();
        var rpcClient = CreateRpcClient();
        var observed = new List<ObservedUtxo>();

        foreach (var address in addresses)
        {
            var response = await rpcClient.SendCommandAsync("listunspent", 0, 9999999, new[] { address.Value }).ConfigureAwait(false);
            foreach (var item in response.Result)
            {
                var amountBtc = decimal.Parse(item["amount"]!.ToString(), CultureInfo.InvariantCulture);
                observed.Add(
                    new ObservedUtxo(
                        TransactionId.Parse(item["txid"]!.ToString()),
                        int.Parse(item["vout"]!.ToString(), CultureInfo.InvariantCulture),
                        DomainMoney.FromSatoshis((long)(amountBtc * 100_000_000m)),
                        item["scriptPubKey"]!.ToString(),
                        DomainBitcoinAddress.Parse(item["address"]!.ToString()),
                        int.Parse(item["confirmations"]!.ToString(), CultureInfo.InvariantCulture)));
            }
        }

        return observed;
    }

    private RPCClient CreateRpcClient()
    {
        if (string.IsNullOrWhiteSpace(options.RpcUri) ||
            string.IsNullOrWhiteSpace(options.RpcUser) ||
            string.IsNullOrWhiteSpace(options.RpcPassword))
        {
            throw new InvalidOperationStateException("Regtest RPC is not configured.");
        }

        return new RPCClient(
            new NetworkCredential(options.RpcUser, options.RpcPassword),
            new Uri(options.RpcUri),
            ResolveNetwork());
    }

    private bool IsRegtest() => options.Network.Trim().Equals("regtest", StringComparison.OrdinalIgnoreCase);

    private Network ResolveNetwork() =>
        options.Network.Trim().ToLowerInvariant() switch
        {
            "testnet4" => Network.GetNetwork("testnet4") ?? Network.TestNet,
            "regtest" => Network.RegTest,
            _ => throw new MainnetGuardException($"Unsupported or unsafe network '{options.Network}'."),
        };

    private string? BuildExplorerUrl(string txid) =>
        IsRegtest() ? null : $"https://mempool.space/testnet4/tx/{txid}";

    private sealed record TestnetUtxoDto(string txid, int vout, long value, TestnetStatusDto status);

    private sealed record TestnetTransactionStatusDto(bool confirmed, long? block_height);

    private sealed record TestnetStatusDto(bool confirmed, long? block_height);
}
