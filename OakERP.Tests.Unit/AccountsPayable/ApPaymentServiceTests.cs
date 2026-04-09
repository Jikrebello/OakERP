using Microsoft.EntityFrameworkCore;
using Moq;
using OakERP.Common.Enums;
using OakERP.Common.Errors;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsPayable;

public sealed class ApPaymentServiceTests
{
    private readonly ApPaymentServiceTestFactory _factory = new();

    [Fact]
    public async Task CreateAsync_Should_Create_Unapplied_Draft_Payment()
    {
        var vendor = ApPaymentServiceTestFactory.CreateVendor();
        var bankAccount = ApPaymentServiceTestFactory.CreateBankAccount();
        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-2001",
            VendorId = vendor.Id,
            BankAccountId = bankAccount.Id,
            PaymentDate = DaysFromToday(-4),
            Amount = 125m,
            Memo = "Vendor payment",
            PerformedBy = "unit-user",
        };

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.AccountsPayable.ApPayment>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeTrue();
        result.Message.ShouldBe("AP payment created successfully.");
        result.Payment.ShouldNotBeNull();
        result.Payment!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Payment.AllocatedAmount.ShouldBe(0m);
        result.Payment.UnappliedAmount.ShouldBe(125m);
        result.Invoices.ShouldBeEmpty();
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Payment_With_Initial_Allocations_And_Close_Fully_Settled_Invoices()
    {
        var vendor = ApPaymentServiceTestFactory.CreateVendor();
        var bankAccount = ApPaymentServiceTestFactory.CreateBankAccount();
        var invoiceA = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendor.Id,
            docTotal: 100m,
            docNo: "APINV-2001"
        );
        var invoiceB = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendor.Id,
            docTotal: 50m,
            docNo: "APINV-2002"
        );

        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-2002",
            VendorId = vendor.Id,
            BankAccountId = bankAccount.Id,
            PaymentDate = DaysFromToday(-4),
            Amount = 150m,
            PerformedBy = "unit-user",
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceA.Id, AmountApplied = 100m },
                new ApPaymentAllocationInputDto { ApInvoiceId = invoiceB.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.AccountsPayable.ApPayment>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoiceA, invoiceB]);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        result.Payment!.AllocatedAmount.ShouldBe(150m);
        result.Payment.UnappliedAmount.ShouldBe(0m);
        result.Invoices.Count.ShouldBe(2);
        result.Invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();
        result.Invoices.All(x => x.RemainingAmount == 0m).ShouldBeTrue();
    }

    [Fact]
    public async Task AllocateAsync_Should_Apply_To_Draft_Payment_And_Leave_Invoice_Posted_When_Partial()
    {
        var vendorId = Guid.NewGuid();
        var payment = ApPaymentServiceTestFactory.CreateDraftPayment(
            vendorId: vendorId,
            amount: 100m
        );
        var invoice = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendorId,
            docTotal: 150m
        );
        var command = new AllocateApPaymentCommand
        {
            PaymentId = payment.Id,
            AllocationDate = DaysFromToday(-3),
            PerformedBy = "unit-user",
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoice.Id, AmountApplied = 60m },
            ],
        };

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.Is<IReadOnlyCollection<Guid>>(ids =>
                        ids.Count == 1 && ids.Single() == invoice.Id
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var service = _factory.CreateService();

        var result = await service.AllocateAsync(command);

        result.Success.ShouldBeTrue();
        result.Payment.ShouldNotBeNull();
        result.Payment!.AllocatedAmount.ShouldBe(60m);
        result.Payment.UnappliedAmount.ShouldBe(40m);
        result.Invoices.Single().DocStatus.ShouldBe(DocStatus.Posted);
        result.Invoices.Single().RemainingAmount.ShouldBe(90m);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Vendor_Mismatch()
    {
        var vendor = ApPaymentServiceTestFactory.CreateVendor();
        var otherVendor = ApPaymentServiceTestFactory.CreateVendor();
        var bankAccount = ApPaymentServiceTestFactory.CreateBankAccount();
        var invoice = ApPaymentServiceTestFactory.CreatePostedInvoice(vendorId: otherVendor.Id);
        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-2003",
            VendorId = vendor.Id,
            BankAccountId = bankAccount.Id,
            PaymentDate = DaysFromToday(-4),
            Amount = 100m,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoice.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.AddAsync(It.IsAny<Domain.Entities.AccountsPayable.ApPayment>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("same vendor");
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_Should_Reject_When_Request_Exceeds_Payment_Unapplied_Amount()
    {
        var vendorId = Guid.NewGuid();
        var payment = ApPaymentServiceTestFactory.CreateDraftPayment(
            vendorId: vendorId,
            amount: 100m,
            allocatedAmount: 80m
        );
        var invoice = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendorId,
            docTotal: 100m
        );
        var command = new AllocateApPaymentCommand
        {
            PaymentId = payment.Id,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoice.Id, AmountApplied = 25m },
            ],
        };

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);

        var service = _factory.CreateService();

        var result = await service.AllocateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("unapplied amount");
    }

    [Fact]
    public async Task AllocateAsync_Should_Reject_When_Request_Exceeds_Invoice_Remaining_Amount()
    {
        var vendorId = Guid.NewGuid();
        var payment = ApPaymentServiceTestFactory.CreateDraftPayment(
            vendorId: vendorId,
            amount: 100m
        );
        var invoice = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendorId,
            docTotal: 100m,
            settledAmount: 90m
        );
        var command = new AllocateApPaymentCommand
        {
            PaymentId = payment.Id,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoice.Id, AmountApplied = 15m },
            ],
        };

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);

        var service = _factory.CreateService();

        var result = await service.AllocateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("remaining balance");
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Conflict_For_Duplicate_DocNo()
    {
        var vendor = ApPaymentServiceTestFactory.CreateVendor();
        var bankAccount = ApPaymentServiceTestFactory.CreateBankAccount();
        var command = new CreateApPaymentCommand
        {
            DocNo = "APPAY-DUP",
            VendorId = vendor.Id,
            BankAccountId = bankAccount.Id,
            PaymentDate = DaysFromToday(-4),
            Amount = 10m,
        };

        _factory
            .VendorRepository.Setup(x =>
                x.FindNoTrackingAsync(vendor.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(vendor);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ApPaymentRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Conflict);
        result.Message.ShouldContain("already exists");
        _factory.UnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_Should_Return_Conflict_On_Concurrency_Failure()
    {
        var vendorId = Guid.NewGuid();
        var payment = ApPaymentServiceTestFactory.CreateDraftPayment(
            vendorId: vendorId,
            amount: 100m
        );
        var invoice = ApPaymentServiceTestFactory.CreatePostedInvoice(
            vendorId: vendorId,
            docTotal: 100m
        );
        var command = new AllocateApPaymentCommand
        {
            PaymentId = payment.Id,
            Allocations =
            [
                new ApPaymentAllocationInputDto { ApInvoiceId = invoice.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .ApPaymentRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(payment.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(payment);
        _factory
            .ApInvoiceRepository.Setup(x =>
                x.GetTrackedForSettlementAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);
        var concurrencyException = new DbUpdateConcurrencyException();
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(concurrencyException);
        _factory
            .PersistenceFailureClassifier.Setup(x => x.IsConcurrencyConflict(concurrencyException))
            .Returns(true);

        var service = _factory.CreateService();

        var result = await service.AllocateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Conflict);
        result.Message.ShouldContain("modified while allocations were being saved");
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
    }
}
