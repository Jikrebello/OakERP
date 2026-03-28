using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth;
using OakERP.Domain.Repository_Interfaces.Users;

namespace OakERP.Tests.Unit.Auth;

public class AuthServiceTestFactory
{
    public Mock<IIdentityGateway> IdentityGateway { get; } = new(MockBehavior.Strict);
    public Mock<IJwtGenerator> JwtGenerator { get; } = new(MockBehavior.Strict);
    public Mock<ITenantRepository> TenantRepository { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<AuthService>> Logger { get; } = new();

    public AuthServiceTestFactory()
    {
        JwtGenerator.Setup(j => j.Generate(It.IsAny<JwtTokenInput>())).Returns("mock-token");

        UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
    }

    public AuthService CreateService() =>
        new(
            IdentityGateway.Object,
            JwtGenerator.Object,
            TenantRepository.Object,
            UnitOfWork.Object,
            Logger.Object
        );
}
