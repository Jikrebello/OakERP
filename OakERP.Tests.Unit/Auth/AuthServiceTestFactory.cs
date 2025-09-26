using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Auth;
using OakERP.Domain.Entities.Users;
using OakERP.Domain.Repositories.Users;

namespace OakERP.Tests.Unit.Auth;

public class AuthServiceTestFactory
{
    public Mock<UserManager<ApplicationUser>> UserManager { get; }
    public Mock<SignInManager<ApplicationUser>> SignInManager { get; }
    public Mock<IJwtGenerator> JwtGenerator { get; } = new(MockBehavior.Strict);
    public Mock<ITenantRepository> TenantRepository { get; } = new(MockBehavior.Strict);
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<ILogger<AuthService>> Logger { get; } = new();

    public AuthServiceTestFactory()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        UserManager = new(userStore.Object, null, null, null, null, null, null, null, null);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        SignInManager = new(
            UserManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null,
            null,
            null,
            null
        );

        JwtGenerator.Setup(j => j.Generate(It.IsAny<ApplicationUser>())).Returns("mock-token");

        UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        UnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
    }

    public AuthService CreateService() =>
        new(
            UserManager.Object,
            SignInManager.Object,
            JwtGenerator.Object,
            TenantRepository.Object,
            UnitOfWork.Object,
            Logger.Object
        );
}
