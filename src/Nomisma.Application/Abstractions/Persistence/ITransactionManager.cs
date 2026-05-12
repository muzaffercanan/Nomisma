using System.Data;

namespace Nomisma.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
