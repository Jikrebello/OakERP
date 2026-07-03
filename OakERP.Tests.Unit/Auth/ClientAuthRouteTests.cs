using OakERP.Client.Routes;
using Shouldly;

namespace OakERP.Tests.Unit.Auth;

public sealed class ClientAuthRouteTests
{
    [Fact]
    public void AuthApiRoutes_Should_Target_Api_Auth_Endpoints()
    {
        AuthApiRoutes.Login.ShouldBe("api/auth/login");
        AuthApiRoutes.Register.ShouldBe("api/auth/register");
    }

    [Fact]
    public void AuthRoutes_Should_Remain_Ui_Page_Routes()
    {
        AuthRoutes.Login.ShouldBe("Auth/login");
        AuthRoutes.Register.ShouldBe("Auth/register");
    }
}
