using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth.Internal.Support;
using OakERP.Common.Dtos.Auth;
using OakERP.Common.Errors;
using Shouldly;

namespace OakERP.Tests.Unit.Auth.Internal;

public sealed class AuthTransactionRunnerTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Commit_When_Result_Is_Success()
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        var runner = new AuthTransactionRunner(unitOfWork.Object);

        AuthResultDto result = await runner.ExecuteAsync(() =>
            Task.FromResult(AuthResultDto.SuccessWith("token"))
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

        var runner = new AuthTransactionRunner(unitOfWork.Object);

        AuthResultDto result = await runner.ExecuteAsync(() =>
            Task.FromResult(AuthResultDto.Fail("code", "failure", FailureKind.Validation))
        );

        result.Success.ShouldBeFalse();
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
