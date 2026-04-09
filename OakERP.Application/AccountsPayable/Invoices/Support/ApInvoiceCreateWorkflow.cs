using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsPayable;

namespace OakERP.Application.AccountsPayable.Invoices.Support;

internal sealed class ApInvoiceCreateWorkflow(
    ApInvoiceCreateDependencies repositories,
    InvoiceCreateWorkflowDependencies dependencies,
    ILogger<ApInvoiceService> logger
)
{
    private sealed record CreatePreconditions(Vendor Vendor);

    public async Task<ApInvoiceCommandResultDto> ExecuteAsync(
        CreateApInvoiceCommand command,
        CancellationToken cancellationToken
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

            return await dependencies.TransactionRunner.ExecuteAsync(
                async _ =>
                {
                    var invoice = BuildInvoice(command, validatedCommand, preconditions!.Vendor);

                    await repositories.ApInvoiceRepository.AddAsync(invoice);

                    return ApInvoiceSnapshotFactory.BuildSuccess(
                        invoice,
                        "AP invoice created successfully."
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ApInvoiceCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                new WorkflowExceptionRule<ApInvoiceCommandResultDto>(
                    dependencies.PersistenceFailureClassifier.IsApInvoiceDocNoConflict,
                    _ => ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.DuplicateDocumentNumber)
                ),
                new WorkflowExceptionRule<ApInvoiceCommandResultDto>(
                    dependencies.PersistenceFailureClassifier.IsApInvoiceVendorInvoiceNoConflict,
                    _ =>
                        ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.DuplicateVendorInvoiceNumber)
                )
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(ex, "Unexpected failure before creating AP invoice.");
            return ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.UnexpectedCreateFailure);
        }
    }

    private async Task<(
        CreatePreconditions? preconditions,
        ApInvoiceCommandResultDto? failure
    )> ValidatePreconditionsAsync(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        Vendor? vendor = await repositories.VendorRepository.FindNoTrackingAsync(
            command.VendorId,
            cancellationToken
        );
        if (vendor is null)
        {
            return (null, ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.VendorNotFound));
        }

        if (!vendor.IsActive)
        {
            return (null, ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.VendorInactive));
        }

        if (!await IsActiveCurrencyAsync(validatedCommand.CurrencyCode, cancellationToken))
        {
            return (
                null,
                ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.CurrencyMissingOrInactive)
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
        var currency = await repositories.CurrencyRepository.FindNoTrackingAsync(
            currencyCode,
            cancellationToken
        );
        return currency is not null && currency.IsActive;
    }

    private async Task<ApInvoiceCommandResultDto?> ValidateUniquenessAsync(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        if (
            await repositories.ApInvoiceRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            )
        )
        {
            return ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.DuplicateDocumentNumber);
        }

        if (
            await repositories.ApInvoiceRepository.ExistsVendorInvoiceNoAsync(
                command.VendorId,
                validatedCommand.InvoiceNo,
                cancellationToken
            )
        )
        {
            return ApInvoiceCommandResultDto.Fail(ApInvoiceErrors.DuplicateVendorInvoiceNumber);
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
            var account = await repositories.GlAccountRepository.FindNoTrackingAsync(
                accountNo,
                cancellationToken
            );
            if (account is null || !account.IsActive)
            {
                return ApInvoiceCommandResultDto.Fail(
                    ApInvoiceErrors.InactiveOrMissingGlAccount(accountNo)
                );
            }
        }

        return null;
    }

    private ApInvoice BuildInvoice(
        CreateApInvoiceCommand command,
        ApInvoiceCreateValidationResult validatedCommand,
        Vendor vendor
    )
    {
        DateTimeOffset updatedAt = dependencies.Clock.UtcNow;
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
