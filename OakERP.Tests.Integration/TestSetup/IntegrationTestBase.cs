using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup;

/// <summary>
/// Provides a base class for integration tests that require a database context.
/// </summary>
/// <remarks>This class sets up a database connection and initializes an <see cref="ApplicationDbContext"/> for
/// use in integration tests. It optionally uses a database transaction to isolate test cases and ensure a clean state.
/// Derived classes can override <see cref="UseTransaction"/> to control whether transactions are used.</remarks>
public abstract class IntegrationTestBase
{
    internal ApplicationDbContext DbContext = null!;
    private NpgsqlConnection _connection = null!;
    private IDbContextTransaction _transaction = null!;

    protected virtual bool UseTransaction => true;

    public virtual async Task SetUp()
    {
        _connection = new NpgsqlConnection(
            "Host=localhost;Port=5432;Username=oakadmin;Password=oakpass;Database=oakerp"
        );
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connection)
            .Options;

        DbContext = new ApplicationDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();

        if (UseTransaction)
        {
            _transaction = await DbContext.Database.BeginTransactionAsync();
        }
    }

    public virtual async Task TearDown()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}