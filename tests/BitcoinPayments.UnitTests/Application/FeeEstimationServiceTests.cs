using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Services;
using Moq;
using NBitcoin;

namespace BitcoinPayments.UnitTests.Application;

public sealed class FeeEstimationServiceTests
{
    private const int DefaultFeeRate = 2;

    private readonly Mock<IBitcoinSettings> _settingsMock;
    private readonly FeeEstimationService _sut;

    public FeeEstimationServiceTests()
    {
        _settingsMock = new Mock<IBitcoinSettings>();
        _settingsMock.Setup(s => s.DefaultFeeRateSatPerByte).Returns(DefaultFeeRate);
        _sut = new FeeEstimationService(_settingsMock.Object);
    }

    [Fact]
    public void GetFeeRateSatPerByte_WithExplicitRate_ReturnsRequested()
    {
        var result = _sut.GetFeeRateSatPerByte(5);

        Assert.Equal(5, result);
    }

    [Fact]
    public void GetFeeRateSatPerByte_WithNull_ReturnsDefault()
    {
        var result = _sut.GetFeeRateSatPerByte(null);

        Assert.Equal(DefaultFeeRate, result);
    }

    [Fact]
    public void GetFeeRateSatPerByte_WithZero_ReturnsDefault()
    {
        var result = _sut.GetFeeRateSatPerByte(0);

        Assert.Equal(DefaultFeeRate, result);
    }

    [Fact]
    public void GetFeeRateSatPerByte_WithNegative_ReturnsDefault()
    {
        var result = _sut.GetFeeRateSatPerByte(-1);

        Assert.Equal(DefaultFeeRate, result);
    }

    [Fact]
    public void GetFeeRate_ReturnsNBitcoinFeeRate()
    {
        var result = _sut.GetFeeRate(3);

        Assert.IsType<FeeRate>(result);
        var expected = new FeeRate(Money.Satoshis(3 * 1000L));
        Assert.Equal(expected.SatoshiPerByte, result.SatoshiPerByte);
    }
}
