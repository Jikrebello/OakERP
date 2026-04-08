using OakERP.Application.Interfaces.Persistence;

namespace OakERP.Application.Posting.Support;

internal sealed class PostingTransactionExecutor(PostingPersistenceDependencies persistenceDependencies)
{
    private IUnitOfWork UnitOfWork => persistenceDependencies.UnitOfWork;
    private IPersistenceFailureClassifier PersistenceFailureClassifier =>
        persistenceDependencies.PersistenceFailureClassifier;

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string concurrencyConflictMessage,
        CancellationToken cancellationToken
    )
    {
        await UnitOfWork.BeginTransactionAsync();

        try
        {
            T result = await operation(cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            await UnitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await UnitOfWork.RollbackAsync();

            if (PersistenceFailureClassifier.IsConcurrencyConflict(ex))
            {
                throw new InvalidOperationException(concurrencyConflictMessage, ex);
            }

            throw;
        }
    }
}
