using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup;

public abstract class IntegrationTestBase
{
    internal ApplicationDbContext DbContext = null!;
    private NpgsqlConnection _connection = null!;
    private IDbContextTransaction _transaction = null!;

    protected virtual bool UseTransaction => true;

    public virtual async Task SetUp()
    {
        _connection = new NpgsqlConnection(
            "Host=localhost;Port=5433;Username=oakadmin;Password=oakpass;Database=oakerp"
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
