using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Common;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;

namespace OakERP.Application.AccountsPayable.Invoices.Services;

public sealed class ApInvoiceService(
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    ICurrencyRepository currencyRepository,
    IGlAccountRepository glAccountRepository,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier persistenceFailureClassifier,
    ILogger<ApInvoiceService> logger
) : IApInvoiceService
{
    private sealed record CreatePreconditions(Vendor Vendor);

    public async Task<ApInvoiceCommandResultDto> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApInvoiceCreateValidationResult validatedCommand =
                ApInvoiceCommandValidator.ValidateCreate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            var (preconditions, preconditionFailure) = await ValidatePreconditionsAsync(
                command,
                validatedCommand,
                cancellationToken
            );
            if (preconditionFailure is not null)
            {
                return preconditionFailure;
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var invoice = BuildInvoice(command, validatedCommand, preconditions!.Vendor);

                await apInvoiceRepository.AddAsync(invoice);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return ApInvoiceSnapshotFactory.BuildSuccess(
                    invoice,
                    "AP invoice created successfully."
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                if (persistenceFailureClassifier.IsUniqueConstraint(ex, "ix_ap_invoices_doc_no"))
                {
                    return ApInvoiceCommandResultDto.Fail(
                        "An AP invoice with this document number already exists.",
                        HttpStatusCode.Conflict
                    );
                }

                if (
                    persistenceFailureClassifier.IsUniqueConstraint(
                        ex,
                        "ix_ap_invoices_vendor_id_invoice_no"
                    )
                )
                {
                    return ApInvoiceCommandResultDto.Fail(
                        "This vendor invoice number already exists for the selected vendor.",
                        HttpStatusCode.Conflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while creating AP invoice {DocNo}",
                    validatedCommand.DocNo
                );
                return ApInvoiceCommandResultDto.Fail(
                    "An unexpected error occurred while creating the AP invoice.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AP invoice.");
            return ApInvoiceCommandResultDto.Fail(
                "An unexpected error occurred while creating the AP invoice.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private async Task<(CreatePreconditions? preconditions, ApInvoiceCommandResultDto? failure)> ValidatePreconditionsAsync(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        Vendor? vendor = await vendorRepository.FindNoTrackingAsync(command.VendorId, cancellationToken);
        if (vendor is null)
        {
            return (
                null,
                ApInvoiceCommandResultDto.Fail("Vendor was not found.", HttpStatusCode.NotFound)
            );
        }

        if (!vendor.IsActive)
        {
            return (
                null,
                ApInvoiceCommandResultDto.Fail(
                    "AP invoices can be created only for active vendors.",
                    HttpStatusCode.BadRequest
                )
            );
        }

        if (
            !await IsActiveCurrencyAsync(validatedCommand.CurrencyCode, cancellationToken)
        )
        {
            return (
                null,
                ApInvoiceCommandResultDto.Fail(
                    "AP invoice currency was not found or is inactive.",
                    HttpStatusCode.BadRequest
                )
            );
        }

        ApInvoiceCommandResultDto? uniquenessFailure = await ValidateUniquenessAsync(
            command,
            validatedCommand,
            cancellationToken
        );
        if (uniquenessFailure is not null)
        {
            return (null, uniquenessFailure);
        }

        ApInvoiceCommandResultDto? accountFailure = await ValidateAccountsAsync(
            validatedCommand,
            cancellationToken
        );
        if (accountFailure is not null)
        {
            return (null, accountFailure);
        }

        return (new CreatePreconditions(vendor), null);
    }

    private async Task<bool> IsActiveCurrencyAsync(
        string currencyCode,
        CancellationToken cancellationToken
    )
    {
        var currency = await currencyRepository.FindNoTrackingAsync(currencyCode, cancellationToken);
        return currency is not null && currency.IsActive;
    }

    private async Task<ApInvoiceCommandResultDto?> ValidateUniquenessAsync(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        if (await apInvoiceRepository.ExistsDocNoAsync(validatedCommand.DocNo, cancellationToken))
        {
            return ApInvoiceCommandResultDto.Fail(
                "An AP invoice with this document number already exists.",
                HttpStatusCode.Conflict
            );
        }

        if (
            await apInvoiceRepository.ExistsVendorInvoiceNoAsync(
                command.VendorId,
                validatedCommand.InvoiceNo,
                cancellationToken
            )
        )
        {
            return ApInvoiceCommandResultDto.Fail(
                "This vendor invoice number already exists for the selected vendor.",
                HttpStatusCode.Conflict
            );
        }

        return null;
    }

    private async Task<ApInvoiceCommandResultDto?> ValidateAccountsAsync(
        ApInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        string[] accountNumbers =
        [
            .. validatedCommand.Lines.Select(x => x.AccountNo!).Distinct(StringComparer.Ordinal),
        ];

        foreach (string accountNo in accountNumbers)
        {
            var account = await glAccountRepository.FindNoTrackingAsync(accountNo, cancellationToken);
            if (account is null || !account.IsActive)
            {
                return ApInvoiceCommandResultDto.Fail(
                    $"GL account '{accountNo}' is missing or inactive.",
                    HttpStatusCode.BadRequest
                );
            }
        }

        return null;
    }

    private static ApInvoice BuildInvoice(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        Vendor vendor
    )
    {
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        DateOnly dueDate = command.DueDate ?? command.InvoiceDate.AddDays(vendor.TermsDays);

        return new ApInvoice
        {
            DocNo = validatedCommand.DocNo,
            VendorId = command.VendorId,
            InvoiceNo = validatedCommand.InvoiceNo,
            InvoiceDate = command.InvoiceDate,
            DueDate = dueDate,
            DocStatus = DocStatus.Draft,
            CurrencyCode = validatedCommand.CurrencyCode,
            Memo = validatedCommand.Memo,
            TaxTotal = command.TaxTotal,
            DocTotal = command.DocTotal,
            CreatedBy = validatedCommand.PerformedBy,
            UpdatedBy = validatedCommand.PerformedBy,
            UpdatedAt = updatedAt,
            Lines =
            [
                .. validatedCommand.Lines.Select(
                    (line, index) =>
                        new ApInvoiceLine
                        {
                            LineNo = index + 1,
                            Description = line.Description,
                            AccountNo = line.AccountNo,
                            Qty = line.Qty,
                            UnitPrice = line.UnitPrice,
                            LineTotal = line.LineTotal,
                        }
                ),
            ],
        };
    }
}
