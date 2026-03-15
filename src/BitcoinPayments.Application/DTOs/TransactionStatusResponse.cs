namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a payment status view model.
/// </summary>
/// <param name="Id"></param>
/// <param name="OperationType"></param>
/// <param name="State"></param>
/// <param name="AmountSatoshis"></param>
/// <param name="FeeSatoshis"></param>
/// <param name="BitcoinTxId"></param>
/// <param name="ConfirmationCount"></param>
/// <param name="ExplorerUrl"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
/// <param name="ErrorMessage"></param>
/// <param name="BuyerWalletId"></param>
/// <param name="MerchantWalletId"></param>
public sealed record TransactionStatusResponse(
    Guid Id,
    string OperationType,
    string State,
    long AmountSatoshis,
    long? FeeSatoshis,
    string? BitcoinTxId,
    int ConfirmationCount,
    string? ExplorerUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ErrorMessage,
    Guid BuyerWalletId,
    Guid MerchantWalletId);
