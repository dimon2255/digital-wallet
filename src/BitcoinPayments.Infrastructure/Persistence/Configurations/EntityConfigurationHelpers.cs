using System.Text.Json;
using BitcoinPayments.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

internal static class EntityConfigurationHelpers
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static ValueConverter<Money, long> MoneyConverter { get; } =
        new(value => value.Satoshis, value => Money.FromSatoshis(value));

    public static ValueConverter<Money?, long?> NullableMoneyConverter { get; } =
        new(
            value => value.HasValue ? value.Value.Satoshis : null,
            value => value.HasValue ? Money.FromSatoshis(value.Value) : null);

    public static ValueConverter<BitcoinPayments.Domain.ValueObjects.BitcoinAddress, string> BitcoinAddressConverter { get; } =
        new(value => value.Value, value => BitcoinPayments.Domain.ValueObjects.BitcoinAddress.Parse(value));

    public static ValueConverter<TransactionId, string> TransactionIdConverter { get; } =
        new(value => value.Value, value => TransactionId.Parse(value));

    public static ValueConverter<TransactionId?, string?> NullableTransactionIdConverter { get; } =
        new(
            value => value.HasValue ? value.Value.Value : null,
            value => string.IsNullOrWhiteSpace(value) ? null : TransactionId.Parse(value));

    public static ValueConverter<Dictionary<string, string?>, string> MetadataConverter { get; } =
        new(
            value => JsonSerializer.Serialize(value, JsonSerializerOptions),
            value => string.IsNullOrWhiteSpace(value)
                ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                : JsonSerializer.Deserialize<Dictionary<string, string?>>(value, JsonSerializerOptions)
                  ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));

    public static ValueComparer<Dictionary<string, string?>> MetadataComparer { get; } =
        new(
            (left, right) => JsonSerializer.Serialize(left, JsonSerializerOptions) == JsonSerializer.Serialize(right, JsonSerializerOptions),
            value => JsonSerializer.Serialize(value, JsonSerializerOptions).GetHashCode(StringComparison.Ordinal),
            value => value.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase));
}
