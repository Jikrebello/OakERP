namespace OakERP.Application.Interfaces.Persistence;

/// <summary>
/// Defines a contract for managing database transactions in a unit of work pattern.
/// </summary>
/// <remarks>The <see cref="IUnitOfWork"/> interface provides methods to begin, commit, and roll back
/// transactions. It is typically used to ensure that a series of operations are executed within the same transactional
/// context.</remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <remarks>This method initiates a transaction that can be used to group multiple database operations
    /// into a single unit of work. Ensure that the transaction is committed or rolled back  to finalize or discard the
    /// changes, respectively.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <remarks>This method finalizes all changes made during the transaction and makes them permanent.  If
    /// the transaction is already committed or rolled back, calling this method will have no effect.</remarks>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Asynchronously rolls back the current transaction, undoing any changes made since the transaction began.
    /// </summary>
    /// <remarks>This method should be called to revert a transaction if an error occurs or if the changes
    /// should not be committed.  Once the transaction is rolled back, it cannot be reused, and a new transaction must
    /// be started if further operations are needed.</remarks>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    Task RollbackAsync();
}
