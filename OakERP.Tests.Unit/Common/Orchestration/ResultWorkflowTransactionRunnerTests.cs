using Moq;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Dtos.Base;
using Shouldly;

namespace OakERP.Tests.Unit.Common.Orchestration;

public sealed class ResultWorkflowTransactionRunnerTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Commit_When_Result_Is_Successful()
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        var runner = new ResultWorkflowTransactionRunner(unitOfWork.Object);

        TestResultDto result = await runner.ExecuteAsync(
            _ => Task.FromResult(TestResultDto.Ok()),
            CancellationToken.None
        );

        result.Success.ShouldBeTrue();
        unitOfWork.Verify(x => x.RollbackAsync(), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Roll_Back_When_Result_Is_Failure()
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        var runner = new ResultWorkflowTransactionRunner(unitOfWork.Object);

        TestResultDto result = await runner.ExecuteAsync(
            _ => Task.FromResult(TestResultDto.Fail("failure")),
            CancellationToken.None
        );

        result.Success.ShouldBeFalse();
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Roll_Back_And_Rethrow_When_Operation_Throws()
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        var runner = new ResultWorkflowTransactionRunner(unitOfWork.Object);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            runner.ExecuteAsync<TestResultDto>(
                _ => throw new InvalidOperationException("boom"),
                CancellationToken.None
            )
        );

        unitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }

    private sealed class TestResultDto : BaseResultDto
    {
        public static TestResultDto Ok() => new() { Success = true };

        public static TestResultDto Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
