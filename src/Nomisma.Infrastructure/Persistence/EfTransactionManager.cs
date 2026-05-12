using System.Data;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Persistence;

namespace Nomisma.Infrastructure.Persistence;

public sealed class EfTransactionManager : ITransactionManager
{
    private readonly NomismaDbContext _dbContext;

    public EfTransactionManager(NomismaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        try
        {
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
