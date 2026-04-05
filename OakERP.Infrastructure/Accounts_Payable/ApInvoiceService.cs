using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using OakERP.Application.AccountsPayable;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Common;
using OakERP.Domain.Repository_Interfaces.General_Ledger;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApInvoiceService(
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    ICurrencyRepository currencyRepository,
    IGlAccountRepository glAccountRepository,
    ApInvoiceCommandValidator commandValidator,
    ApInvoiceSnapshotFactory snapshotFactory,
    IUnitOfWork unitOfWork,
    ILogger<ApInvoiceService> logger
) : IApInvoiceService
{
    public async Task<ApInvoiceCommandResultDTO> CreateAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ApInvoiceCreateValidationResult validatedCommand = commandValidator.ValidateCreate(
                command
            );
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
                return ApInvoiceCommandResultDTO.Fail(
                    "Vendor was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!vendor.IsActive)
            {
                return ApInvoiceCommandResultDTO.Fail(
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
                return ApInvoiceCommandResultDTO.Fail(
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
                return ApInvoiceCommandResultDTO.Fail(
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
                return ApInvoiceCommandResultDTO.Fail(
                    "This vendor invoice number already exists for the selected vendor.",
                    HttpStatusCode.Conflict
                );
            }

            string[] accountNumbers = validatedCommand
                .Lines.Select(x => x.AccountNo!)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (string accountNo in accountNumbers)
            {
                var account = await glAccountRepository.FindNoTrackingAsync(
                    accountNo,
                    cancellationToken
                );
                if (account is null || !account.IsActive)
                {
                    return ApInvoiceCommandResultDTO.Fail(
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
                    Lines = validatedCommand
                        .Lines.Select(
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
                        )
                        .ToList(),
                };

                await apInvoiceRepository.AddAsync(invoice);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();

                return snapshotFactory.BuildSuccess(invoice, "AP invoice created successfully.");
            }
            catch (DbUpdateException ex) when (IsUniqueConstraint(ex, "ix_ap_invoices_doc_no"))
            {
                await unitOfWork.RollbackAsync();
                return ApInvoiceCommandResultDTO.Fail(
                    "An AP invoice with this document number already exists.",
                    HttpStatusCode.Conflict
                );
            }
            catch (DbUpdateException ex)
                when (IsUniqueConstraint(ex, "ix_ap_invoices_vendor_id_invoice_no"))
            {
                await unitOfWork.RollbackAsync();
                return ApInvoiceCommandResultDTO.Fail(
                    "This vendor invoice number already exists for the selected vendor.",
                    HttpStatusCode.Conflict
                );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                logger.LogError(
                    ex,
                    "Unexpected failure while creating AP invoice {DocNo}",
                    validatedCommand.DocNo
                );
                return ApInvoiceCommandResultDTO.Fail(
                    "An unexpected error occurred while creating the AP invoice.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AP invoice.");
            return ApInvoiceCommandResultDTO.Fail(
                "An unexpected error occurred while creating the AP invoice.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException ex, string constraintName) =>
        ex.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            constraintName,
            StringComparison.Ordinal
        );
}
