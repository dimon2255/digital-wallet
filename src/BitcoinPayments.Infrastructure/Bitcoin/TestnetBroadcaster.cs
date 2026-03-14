using System.Net.Http.Headers;
using BitcoinPayments.Domain.Exceptions;

namespace BitcoinPayments.Infrastructure.Bitcoin;

/// <summary>
/// Broadcasts transactions through the configured public testnet API.
/// </summary>
public sealed class TestnetBroadcaster
{
    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestnetBroadcaster"/> class.
    /// </summary>
    public TestnetBroadcaster(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Broadcasts a raw transaction hex string.
    /// </summary>
    public async Task<string> BroadcastAsync(string rawTransactionHex, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(nameof(NBitcoinNetworkService));
        using var request = new HttpRequestMessage(HttpMethod.Post, "tx")
        {
            Content = new StringContent(rawTransactionHex),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var txid = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (!response.IsSuccessStatusCode)
        {
            throw new TransactionBroadcastException(
                $"Failed to broadcast transaction to testnet API. HTTP {(int)response.StatusCode}: {txid}");
        }

        return txid.Trim('"');
    }
}
