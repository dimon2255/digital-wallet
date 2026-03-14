using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core idempotency repository.
/// </summary>
public sealed class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyKeyRepository"/> class.
    /// </summary>
    public IdempotencyKeyRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<IdempotencyKeyRecord?> GetAsync(string key, CancellationToken cancellationToken) =>
        dbContext.IdempotencyKeys.FirstOrDefaultAsync(record => record.Key == key, cancellationToken);

    /// <inheritdoc />
    public async Task UpsertAsync(IdempotencyKeyRecord record, CancellationToken cancellationToken)
    {
        var existing = await dbContext.IdempotencyKeys.FirstOrDefaultAsync(candidate => candidate.Key == record.Key, cancellationToken);
        if (existing is null)
        {
            await dbContext.IdempotencyKeys.AddAsync(record, cancellationToken);
        }
        else
        {
            existing.ResponseStatusCode = record.ResponseStatusCode;
            existing.ResponseBody = record.ResponseBody;
            existing.CreatedAt = record.CreatedAt;
            existing.ExpiresAt = record.ExpiresAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
