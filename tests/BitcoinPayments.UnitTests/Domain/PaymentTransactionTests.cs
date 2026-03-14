using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.ValueObjects;
using DomainMoney = BitcoinPayments.Domain.ValueObjects.Money;

namespace BitcoinPayments.UnitTests.Domain;

public sealed class PaymentTransactionTests
{
    private static PaymentTransaction CreateChargePayment() =>
        new(
            "test-key",
            PaymentOperationType.Charge,
            DomainMoney.FromSatoshis(10_000),
            Guid.NewGuid(),
            Guid.NewGuid());

    private static PaymentTransaction CreateAuthPayment() =>
        new(
            "auth-key",
            PaymentOperationType.Auth,
            DomainMoney.FromSatoshis(10_000),
            Guid.NewGuid(),
            Guid.NewGuid());

    private static readonly string ValidTxHex = new('a', 64);

    [Fact]
    public void Constructor_SetsInitialState_Created()
    {
        var payment = CreateChargePayment();

        Assert.Equal(TransactionState.Created, payment.State);
    }

    [Fact]
    public void MarkBroadcast_SetsTransactionData()
    {
        var payment = CreateChargePayment();
        var txId = TransactionId.Parse(ValidTxHex);
        var fee = DomainMoney.FromSatoshis(200);

        payment.MarkBroadcast(txId, "rawhex", fee, TransactionState.Mempool);

        Assert.Equal(txId, payment.BitcoinTxId);
        Assert.Equal("rawhex", payment.RawTransactionHex);
        Assert.Equal(fee, payment.Fee);
        Assert.Equal(TransactionState.Mempool, payment.State);
    }

    [Fact]
    public void MarkFailed_SetsFailedState()
    {
        var payment = CreateChargePayment();

        payment.MarkFailed("error");

        Assert.Equal(TransactionState.Failed, payment.State);
        Assert.Equal("error", payment.ErrorMessage);
    }

    [Fact]
    public void AttachToParent_SetsParentId()
    {
        var payment = CreateChargePayment();
        var parentId = Guid.NewGuid();

        payment.AttachToParent(parentId);

        Assert.Equal(parentId, payment.ParentTransactionId);
    }

    [Fact]
    public void TransitionTo_FromCreated_Succeeds()
    {
        var payment = CreateChargePayment();

        payment.TransitionTo(TransactionState.Mempool);

        Assert.Equal(TransactionState.Mempool, payment.State);
    }

    [Fact]
    public void TransitionTo_FromSettled_ThrowsInvalidOperationStateException()
    {
        var payment = CreateChargePayment();
        payment.State = TransactionState.Settled;

        Assert.Throws<InvalidOperationStateException>(
            () => payment.TransitionTo(TransactionState.Mempool));
    }

    [Fact]
    public void TransitionTo_FromVoided_ThrowsInvalidOperationStateException()
    {
        var payment = CreateChargePayment();
        payment.State = TransactionState.Voided;

        Assert.Throws<InvalidOperationStateException>(
            () => payment.TransitionTo(TransactionState.Mempool));
    }

    [Fact]
    public void UpdateConfirmations_ReachesSettlement()
    {
        var payment = CreateChargePayment();
        payment.State = TransactionState.Mempool;

        payment.UpdateConfirmations(3, 3);

        Assert.Equal(TransactionState.Settled, payment.State);
        Assert.Equal(3, payment.ConfirmationCount);
    }

    [Fact]
    public void UpdateConfirmations_BelowThreshold_SetsConfirming()
    {
        var payment = CreateChargePayment();
        payment.State = TransactionState.Mempool;

        payment.UpdateConfirmations(1, 3);

        Assert.Equal(TransactionState.Confirming, payment.State);
        Assert.Equal(1, payment.ConfirmationCount);
    }

    [Fact]
    public void UpdateConfirmations_AuthType_SetsAuthActive()
    {
        var payment = CreateAuthPayment();
        payment.State = TransactionState.FundingBroadcast;

        payment.UpdateConfirmations(1, 3);

        Assert.Equal(TransactionState.AuthActive, payment.State);
    }
}
