using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OakERP.API.Contracts.Posting;
using OakERP.API.Controllers;
using OakERP.Application.Posting.Contracts;
using OakERP.Common.Enums;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class DocumentPostingControllerTests
{
    [Fact]
    public async Task ApInvoicesController_Post_Should_Map_To_ApInvoice_PostCommand()
    {
        var postingService = new Mock<IPostingService>(MockBehavior.Strict);
        var invoiceService = new Mock<IApInvoiceService>(MockBehavior.Strict);
        var invoiceId = Guid.NewGuid();
        var request = new PostDocumentRequestDto { PostingDate = DaysFromToday(-1), Force = true };
        var expectedResult = new PostResult(
            DocKind.ApInvoice,
            invoiceId,
            "APINV-001",
            request.PostingDate!.Value,
            Guid.NewGuid(),
            2,
            0
        );

        postingService
            .Setup(x =>
                x.PostAsync(
                    It.Is<PostCommand>(command =>
                        command.DocKind == DocKind.ApInvoice
                        && command.SourceId == invoiceId
                        && command.PerformedBy == "unit-user"
                        && command.PostingDate == request.PostingDate
                        && command.Force == request.Force
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var controller = new ApInvoicesController(invoiceService.Object, postingService.Object);
        SetUser(controller);

        var actionResult = await controller.Post(invoiceId, request, CancellationToken.None);

        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ApPaymentsController_Post_Should_Map_To_ApPayment_PostCommand()
    {
        var postingService = new Mock<IPostingService>(MockBehavior.Strict);
        var paymentService = new Mock<IApPaymentService>(MockBehavior.Strict);
        var paymentId = Guid.NewGuid();
        var request = new PostDocumentRequestDto { PostingDate = DaysFromToday(-2), Force = false };
        var expectedResult = new PostResult(
            DocKind.ApPayment,
            paymentId,
            "APPAY-001",
            request.PostingDate!.Value,
            Guid.NewGuid(),
            2,
            0
        );

        postingService
            .Setup(x =>
                x.PostAsync(
                    It.Is<PostCommand>(command =>
                        command.DocKind == DocKind.ApPayment
                        && command.SourceId == paymentId
                        && command.PerformedBy == "unit-user"
                        && command.PostingDate == request.PostingDate
                        && command.Force == request.Force
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var controller = new ApPaymentsController(paymentService.Object, postingService.Object);
        SetUser(controller);

        var actionResult = await controller.Post(paymentId, request, CancellationToken.None);

        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ArInvoicesController_Post_Should_Map_To_ArInvoice_PostCommand()
    {
        var postingService = new Mock<IPostingService>(MockBehavior.Strict);
        var invoiceService = new Mock<IArInvoiceService>(MockBehavior.Strict);
        var invoiceId = Guid.NewGuid();
        var request = new PostDocumentRequestDto { PostingDate = DaysFromToday(-3), Force = false };
        var expectedResult = new PostResult(
            DocKind.ArInvoice,
            invoiceId,
            "ARINV-001",
            request.PostingDate!.Value,
            Guid.NewGuid(),
            5,
            1
        );

        postingService
            .Setup(x =>
                x.PostAsync(
                    It.Is<PostCommand>(command =>
                        command.DocKind == DocKind.ArInvoice
                        && command.SourceId == invoiceId
                        && command.PerformedBy == "unit-user"
                        && command.PostingDate == request.PostingDate
                        && command.Force == request.Force
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var controller = new ArInvoicesController(invoiceService.Object, postingService.Object);
        SetUser(controller);

        var actionResult = await controller.Post(invoiceId, request, CancellationToken.None);

        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ArReceiptsController_Post_Should_Map_To_ArReceipt_PostCommand()
    {
        var postingService = new Mock<IPostingService>(MockBehavior.Strict);
        var receiptService = new Mock<IArReceiptService>(MockBehavior.Strict);
        var receiptId = Guid.NewGuid();
        var request = new PostDocumentRequestDto { PostingDate = DaysFromToday(-4), Force = true };
        var expectedResult = new PostResult(
            DocKind.ArReceipt,
            receiptId,
            "RCPT-001",
            request.PostingDate!.Value,
            Guid.NewGuid(),
            2,
            0
        );

        postingService
            .Setup(x =>
                x.PostAsync(
                    It.Is<PostCommand>(command =>
                        command.DocKind == DocKind.ArReceipt
                        && command.SourceId == receiptId
                        && command.PerformedBy == "unit-user"
                        && command.PostingDate == request.PostingDate
                        && command.Force == request.Force
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var controller = new ArReceiptsController(receiptService.Object, postingService.Object);
        SetUser(controller);

        var actionResult = await controller.Post(receiptId, request, CancellationToken.None);

        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedResult);
    }

    private static void SetUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "unit-user")],
                        "TestAuthType"
                    )
                ),
            },
        };
    }
}
