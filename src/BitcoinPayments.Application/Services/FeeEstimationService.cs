using BitcoinPayments.Application.Abstractions;
using NBitcoin;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Resolves transaction fee rates.
/// </summary>
public sealed class FeeEstimationService
{
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeeEstimationService"/> class.
    /// </summary>
    public FeeEstimationService(IBitcoinSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Gets the effective fee rate in sat/vbyte.
    /// </summary>
    public int GetFeeRateSatPerByte(int? requestedFeeRateSatPerByte) =>
        requestedFeeRateSatPerByte is > 0 ? requestedFeeRateSatPerByte.Value : settings.DefaultFeeRateSatPerByte;

    /// <summary>
    /// Gets the effective NBitcoin fee rate.
    /// </summary>
    public FeeRate GetFeeRate(int? requestedFeeRateSatPerByte)
    {
        var satPerByte = GetFeeRateSatPerByte(requestedFeeRateSatPerByte);
        return new FeeRate(NBitcoin.Money.Satoshis(satPerByte * 1000L));
    }
}
