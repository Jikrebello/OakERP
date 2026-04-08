using Microsoft.EntityFrameworkCore;
using Moq;
using OakERP.Common.Errors;
using OakERP.Common.Enums;
using Shouldly;

namespace OakERP.Tests.Unit.AccountsReceivable;

public sealed class ArReceiptServiceTests
{
    private readonly ArReceiptServiceTestFactory _factory = new();

    [Fact]
    public async Task CreateAsync_Should_Create_Unapplied_Draft_Receipt()
    {
        var customer = ArReceiptServiceTestFactory.CreateCustomer();
        var bankAccount = ArReceiptServiceTestFactory.CreateBankAccount();
        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-2001",
            CustomerId = customer.Id,
            BankAccountId = bankAccount.Id,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 125m,
            CurrencyCode = "ZAR",
            Memo = "Customer payment",
            PerformedBy = "unit-user",
        };

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsReceivable.ArReceipt>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeTrue();
        result.Message.ShouldBe("AR receipt created successfully.");
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.DocStatus.ShouldBe(DocStatus.Draft);
        result.Receipt.AllocatedAmount.ShouldBe(0m);
        result.Receipt.UnappliedAmount.ShouldBe(125m);
        result.Invoices.ShouldBeEmpty();
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Receipt_With_Initial_Allocations_And_Close_Fully_Settled_Invoices()
    {
        var customer = ArReceiptServiceTestFactory.CreateCustomer();
        var bankAccount = ArReceiptServiceTestFactory.CreateBankAccount();
        var invoiceA = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customer.Id,
            docTotal: 100m,
            docNo: "ARINV-2001"
        );
        var invoiceB = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customer.Id,
            docTotal: 50m,
            docNo: "ARINV-2002"
        );

        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-2002",
            CustomerId = customer.Id,
            BankAccountId = bankAccount.Id,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 150m,
            CurrencyCode = "ZAR",
            PerformedBy = "unit-user",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceA.Id, AmountApplied = 100m },
                new ArReceiptAllocationInputDto { ArInvoiceId = invoiceB.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsReceivable.ArReceipt>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
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
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.AllocatedAmount.ShouldBe(150m);
        result.Receipt.UnappliedAmount.ShouldBe(0m);
        result.Invoices.Count.ShouldBe(2);
        result.Invoices.All(x => x.DocStatus == DocStatus.Closed).ShouldBeTrue();
        result.Invoices.All(x => x.RemainingAmount == 0m).ShouldBeTrue();
    }

    [Fact]
    public async Task AllocateAsync_Should_Apply_To_Draft_Receipt_And_Leave_Invoice_Posted_When_Partial()
    {
        var customerId = Guid.NewGuid();
        var receipt = ArReceiptServiceTestFactory.CreateDraftReceipt(
            customerId: customerId,
            amount: 100m
        );
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customerId,
            docTotal: 150m
        );
        var command = new AllocateArReceiptCommand
        {
            ReceiptId = receipt.Id,
            AllocationDate = new DateOnly(2026, 4, 6),
            PerformedBy = "unit-user",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoice.Id, AmountApplied = 60m },
            ],
        };

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
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
        result.Receipt.ShouldNotBeNull();
        result.Receipt!.AllocatedAmount.ShouldBe(60m);
        result.Receipt.UnappliedAmount.ShouldBe(40m);
        result.Invoices.Single().DocStatus.ShouldBe(DocStatus.Posted);
        result.Invoices.Single().RemainingAmount.ShouldBe(90m);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Customer_Mismatch()
    {
        var customer = ArReceiptServiceTestFactory.CreateCustomer();
        var otherCustomer = ArReceiptServiceTestFactory.CreateCustomer();
        var bankAccount = ArReceiptServiceTestFactory.CreateBankAccount();
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(customerId: otherCustomer.Id);
        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-2003",
            CustomerId = customer.Id,
            BankAccountId = bankAccount.Id,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 100m,
            CurrencyCode = "ZAR",
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoice.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.ExistsDocNoAsync(command.DocNo, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        _factory
            .ArReceiptRepository.Setup(x =>
                x.AddAsync(It.IsAny<OakERP.Domain.Entities.AccountsReceivable.ArReceipt>())
            )
            .Returns(Task.CompletedTask);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([invoice]);

        var service = _factory.CreateService();

        var result = await service.CreateAsync(command);

        result.Success.ShouldBeFalse();
        result.FailureKind.ShouldBe(FailureKind.Validation);
        result.Message.ShouldContain("same customer");
        _factory.UnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _factory.UnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task AllocateAsync_Should_Reject_When_Request_Exceeds_Receipt_Unapplied_Amount()
    {
        var customerId = Guid.NewGuid();
        var receipt = ArReceiptServiceTestFactory.CreateDraftReceipt(
            customerId: customerId,
            amount: 100m,
            allocatedAmount: 80m
        );
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customerId,
            docTotal: 100m
        );
        var command = new AllocateArReceiptCommand
        {
            ReceiptId = receipt.Id,
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoice.Id, AmountApplied = 25m },
            ],
        };

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
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
        var customerId = Guid.NewGuid();
        var receipt = ArReceiptServiceTestFactory.CreateDraftReceipt(
            customerId: customerId,
            amount: 100m
        );
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customerId,
            docTotal: 100m,
            settledAmount: 90m
        );
        var command = new AllocateArReceiptCommand
        {
            ReceiptId = receipt.Id,
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoice.Id, AmountApplied = 15m },
            ],
        };

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
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
        var customer = ArReceiptServiceTestFactory.CreateCustomer();
        var bankAccount = ArReceiptServiceTestFactory.CreateBankAccount();
        var command = new CreateArReceiptCommand
        {
            DocNo = "RCPT-DUP",
            CustomerId = customer.Id,
            BankAccountId = bankAccount.Id,
            ReceiptDate = new DateOnly(2026, 4, 5),
            Amount = 10m,
        };

        _factory
            .CustomerRepository.Setup(x =>
                x.FindNoTrackingAsync(customer.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customer);
        _factory
            .BankAccountRepository.Setup(x =>
                x.FindNoTrackingAsync(bankAccount.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(bankAccount);
        _factory
            .ArReceiptRepository.Setup(x =>
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
        var customerId = Guid.NewGuid();
        var receipt = ArReceiptServiceTestFactory.CreateDraftReceipt(
            customerId: customerId,
            amount: 100m
        );
        var invoice = ArReceiptServiceTestFactory.CreatePostedInvoice(
            customerId: customerId,
            docTotal: 100m
        );
        var command = new AllocateArReceiptCommand
        {
            ReceiptId = receipt.Id,
            Allocations =
            [
                new ArReceiptAllocationInputDto { ArInvoiceId = invoice.Id, AmountApplied = 50m },
            ],
        };

        _factory
            .ArReceiptRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(receipt.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(receipt);
        _factory
            .ArInvoiceRepository.Setup(x =>
                x.GetTrackedForAllocationAsync(
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
