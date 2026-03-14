using NBitcoin;

namespace BitcoinPayments.Infrastructure.Bitcoin;

/// <summary>
/// Builds escrow-related Bitcoin scripts.
/// </summary>
public sealed class ScriptBuilder
{
    /// <summary>
    /// Builds a 2-of-2 multisig redeem script.
    /// </summary>
    public Script BuildEscrowRedeemScript(PubKey buyerPubKey, PubKey merchantPubKey) =>
        PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, new[] { buyerPubKey, merchantPubKey });

    /// <summary>
    /// Builds a CLTV refund script for the buyer.
    /// </summary>
    public Script BuildTimelockRefundScript(long lockBlockHeight, PubKey buyerPubKey) =>
        new(
            Op.GetPushOp(checked((int)lockBlockHeight)),
            OpcodeType.OP_CHECKLOCKTIMEVERIFY,
            OpcodeType.OP_DROP,
            Op.GetPushOp(buyerPubKey.ToBytes()),
            OpcodeType.OP_CHECKSIG);
}
