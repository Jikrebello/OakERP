using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.AccountsPayable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Common;
using OakERP.Domain.Repository_Interfaces.General_Ledger;

namespace OakERP.Application.AccountsPayable;

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

            var vendor = await vendorRepository.FindNoTrackingAsync(
                command.VendorId,
                cancellationToken
            );
            if (vendor is null)
            {
                return ApInvoiceCommandResultDto.Fail(
                    "Vendor was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!vendor.IsActive)
            {
                return ApInvoiceCommandResultDto.Fail(
                    "AP invoices can be created only for active vendors.",
                    HttpStatusCode.BadRequest
                );
            }

            var currency = await currencyRepository.FindNoTrackingAsync(
                validatedCommand.CurrencyCode,
                cancellationToken
            );
            if (currency is null || !currency.IsActive)
            {
                return ApInvoiceCommandResultDto.Fail(
                    "AP invoice currency was not found or is inactive.",
                    HttpStatusCode.BadRequest
                );
            }

            if (
                await apInvoiceRepository.ExistsDocNoAsync(
                    validatedCommand.DocNo,
                    cancellationToken
                )
            )
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

            string[] accountNumbers =
            [
                .. validatedCommand
                    .Lines.Select(x => x.AccountNo!)
                    .Distinct(StringComparer.Ordinal),
            ];

            foreach (string accountNo in accountNumbers)
            {
                var account = await glAccountRepository.FindNoTrackingAsync(
                    accountNo,
                    cancellationToken
                );
                if (account is null || !account.IsActive)
                {
                    return ApInvoiceCommandResultDto.Fail(
                        $"GL account '{accountNo}' is missing or inactive.",
                        HttpStatusCode.BadRequest
                    );
                }
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
                DateOnly dueDate = command.DueDate ?? command.InvoiceDate.AddDays(vendor.TermsDays);

                var invoice = new ApInvoice
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
                if (
                    persistenceFailureClassifier.IsUniqueConstraint(
                        ex,
                        "ix_ap_invoices_doc_no"
                    )
                )
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
}
