using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Moq;
using OakERP.Client.Services.Auth;
using OakERP.UI.ViewModels.Auth;

namespace OakERP.Tests.Unit.Auth;

public sealed class AuthViewModelTests
{
    [Fact]
    public async Task LoginAsync_Should_Not_Call_Auth_Service_When_Form_Is_Invalid()
    {
        var authService = new Mock<OakERP.Client.Services.Auth.IAuthService>();
        var viewModel = new LoginViewModel(
            Mock.Of<IAuthSessionManager>(),
            authService.Object,
            Mock.Of<IToastService>()
        );
        EnableValidation(viewModel.EditContext);

        await viewModel.LoginAsync();

        authService.Verify(
            service => service.LoginAsync(It.IsAny<OakERP.Common.Dtos.Auth.LoginDto>()),
            Times.Never
        );
        Assert.False(viewModel.IsBusy);
    }

    [Fact]
    public async Task RegisterAsync_Should_Not_Call_Auth_Service_When_Form_Is_Invalid()
    {
        var authService = new Mock<OakERP.Client.Services.Auth.IAuthService>();
        var viewModel = new RegisterViewModel(
            Mock.Of<IAuthSessionManager>(),
            authService.Object,
            Mock.Of<IToastService>()
        );
        EnableValidation(viewModel.EditContext);

        await viewModel.RegisterAsync();

        authService.Verify(
            service => service.RegisterAsync(It.IsAny<OakERP.Common.Dtos.Auth.RegisterDto>()),
            Times.Never
        );
        Assert.False(viewModel.IsBusy);
    }

    private static void EnableValidation(EditContext editContext)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        editContext.EnableDataAnnotationsValidation(serviceProvider);
    }
}
