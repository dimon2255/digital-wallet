using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;

namespace BitcoinPayments.UnitTests.Domain;

public sealed class EscrowTests
{
    [Fact]
    public void DefaultState_IsCreated()
    {
        var escrow = new Escrow();

        Assert.Equal(EscrowState.Created, escrow.State);
    }

    [Fact]
    public void MarkFunded_SetsStateAndFundingData()
    {
        var escrow = new Escrow();
        var fundingUtxoId = Guid.NewGuid();
        const long expiresAtBlock = 850_000;

        escrow.MarkFunded(fundingUtxoId, expiresAtBlock);

        Assert.Equal(EscrowState.Funded, escrow.State);
        Assert.Equal(fundingUtxoId, escrow.FundingUtxoId);
        Assert.Equal(expiresAtBlock, escrow.ExpiresAtBlock);
    }

    [Fact]
    public void MarkCaptured_SetsState()
    {
        var escrow = new Escrow();
        escrow.MarkFunded(Guid.NewGuid(), 100);

        escrow.MarkCaptured();

        Assert.Equal(EscrowState.Captured, escrow.State);
    }

    [Fact]
    public void MarkVoided_SetsState()
    {
        var escrow = new Escrow();
        escrow.MarkFunded(Guid.NewGuid(), 100);

        escrow.MarkVoided();

        Assert.Equal(EscrowState.Voided, escrow.State);
    }

    [Fact]
    public void MarkExpired_SetsState()
    {
        var escrow = new Escrow();
        escrow.MarkFunded(Guid.NewGuid(), 100);

        escrow.MarkExpired();

        Assert.Equal(EscrowState.Expired, escrow.State);
    }
}
