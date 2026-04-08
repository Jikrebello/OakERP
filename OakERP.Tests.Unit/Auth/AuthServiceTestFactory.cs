using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.RepositoryInterfaces.Users;

namespace OakERP.Tests.Unit.Auth;

public class AuthServiceTestFactory
{
    public Mock<IIdentityGateway> IdentityGateway { get; } = new(MockBehavior.Strict);
    public Mock<IJwtGenerator> JwtGenerator { get; } = new(MockBehavior.Strict);
    public Mock<ITenantRepository> TenantRepository { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<IClock> Clock { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<AuthService>> Logger { get; } = new();

    public AuthServiceTestFactory()
    {
        JwtGenerator.Setup(j => j.Generate(It.IsAny<JwtTokenInput>())).Returns("mock-token");

        UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        Clock
            .SetupGet(x => x.UtcNow)
            .Returns(new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero));
    }

    public AuthService CreateService() =>
        new(
            IdentityGateway.Object,
            JwtGenerator.Object,
            TenantRepository.Object,
            UnitOfWork.Object,
            Clock.Object,
            Logger.Object
        );
}
