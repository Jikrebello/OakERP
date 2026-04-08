using Moq;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingTransactionExecutorTests
{
    private readonly PostingServiceTestFactory _factory = new();

    [Fact]
    public async Task ExecuteAsync_Should_Begin_Save_And_Commit_On_Success()
    {
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var executor = CreateExecutor();

        string result = await executor.ExecuteAsync(
            _ => Task.FromResult("posted"),
            "concurrency failure",
            CancellationToken.None
        );

        result.ShouldBe("posted");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _factory.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Rollback_And_Translate_Concurrency_Failure()
    {
        var original = new Exception("save failed");

        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(original);
        _factory
            .PersistenceFailureClassifier.Setup(x => x.IsConcurrencyConflict(original))
            .Returns(true);

        var executor = CreateExecutor();

        var ex = await Should.ThrowAsync<ConcurrencyConflictException>(() =>
            executor.ExecuteAsync(
                _ => Task.FromResult(123),
                "The document was modified during posting.",
                CancellationToken.None
            )
        );

        ex.Message.ShouldBe("The document was modified during posting.");
        ex.InnerException.ShouldBe(original);
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Rollback_And_Rethrow_Non_Concurrency_Failure()
    {
        var original = new Exception("save failed");

        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(original);

        var executor = CreateExecutor();

        var ex = await Should.ThrowAsync<Exception>(() =>
            executor.ExecuteAsync(
                _ => Task.FromResult(123),
                "The document was modified during posting.",
                CancellationToken.None
            )
        );

        ex.ShouldBe(original);
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    private PostingTransactionExecutor CreateExecutor() =>
        new(
            new PostingPersistenceDependencies(
                _factory.FiscalPeriodRepository.Object,
                _factory.GlAccountRepository.Object,
                _factory.GlEntryRepository.Object,
                _factory.InventoryLedgerRepository.Object,
                _factory.UnitOfWork.Object,
                _factory.PersistenceFailureClassifier.Object
            )
        );
}
