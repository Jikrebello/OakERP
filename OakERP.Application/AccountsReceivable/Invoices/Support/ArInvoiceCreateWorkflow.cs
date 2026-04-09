using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.Inventory;

namespace OakERP.Application.AccountsReceivable.Invoices.Support;

internal sealed class ArInvoiceCreateWorkflow(
    ArInvoiceCreateDependencies repositories,
    InvoiceCreateWorkflowDependencies dependencies,
    ILogger<ArInvoiceService> logger
)
{
    private sealed record CreatePreconditions(Customer Customer);

    public async Task<ArInvoiceCommandResultDto> ExecuteAsync(
        CreateArInvoiceCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ArInvoiceCreateValidationResult validatedCommand =
                ArInvoiceCommandValidator.ValidateCreate(command);
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
                    var invoice = BuildInvoice(command, validatedCommand, preconditions!.Customer);

                    await repositories.ArInvoiceRepository.AddAsync(invoice);

                    return ArInvoiceSnapshotFactory.BuildSuccess(
                        invoice,
                        "AR invoice created successfully."
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ArInvoiceCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                new WorkflowExceptionRule<ArInvoiceCommandResultDto>(
                    dependencies.PersistenceFailureClassifier.IsArInvoiceDocNoConflict,
                    _ => ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.DuplicateDocumentNumber)
                )
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(ex, "Unexpected failure before creating AR invoice.");
            return ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.UnexpectedCreateFailure);
        }
    }

    private async Task<(
        CreatePreconditions? preconditions,
        ArInvoiceCommandResultDto? failure
    )> ValidatePreconditionsAsync(
        CreateArInvoiceCommand command,
        ArInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        Customer? customer = await repositories.CustomerRepository.FindNoTrackingAsync(
            command.CustomerId,
            cancellationToken
        );
        if (customer is null)
        {
            return (null, ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.CustomerNotFound));
        }

        if (!customer.IsActive)
        {
            return (null, ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.CustomerInactive));
        }

        if (!await IsActiveCurrencyAsync(validatedCommand.CurrencyCode, cancellationToken))
        {
            return (
                null,
                ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.CurrencyMissingOrInactive)
            );
        }

        if (
            await repositories.ArInvoiceRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            )
        )
        {
            return (null, ArInvoiceCommandResultDto.Fail(ArInvoiceErrors.DuplicateDocumentNumber));
        }

        ArInvoiceCommandResultDto? revenueAccountFailure = await ValidateRevenueAccountsAsync(
            validatedCommand,
            cancellationToken
        );
        if (revenueAccountFailure is not null)
        {
            return (null, revenueAccountFailure);
        }

        ArInvoiceCommandResultDto? itemFailure = await ValidateItemsAsync(
            validatedCommand,
            cancellationToken
        );
        if (itemFailure is not null)
        {
            return (null, itemFailure);
        }

        ArInvoiceCommandResultDto? locationFailure = await ValidateLocationsAsync(
            validatedCommand,
            cancellationToken
        );
        if (locationFailure is not null)
        {
            return (null, locationFailure);
        }

        ArInvoiceCommandResultDto? taxRateFailure = await ValidateTaxRatesAsync(validatedCommand);
        if (taxRateFailure is not null)
        {
            return (null, taxRateFailure);
        }

        return (new CreatePreconditions(customer), null);
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

    private async Task<ArInvoiceCommandResultDto?> ValidateRevenueAccountsAsync(
        ArInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        string[] accountNumbers =
        [
            .. validatedCommand
                .Lines.Select(x => x.RevenueAccount)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.Ordinal),
        ];

        foreach (string accountNo in accountNumbers)
        {
            var account = await repositories.GlAccountRepository.FindNoTrackingAsync(
                accountNo,
                cancellationToken
            );
            if (account is null || !account.IsActive)
            {
                return ArInvoiceCommandResultDto.Fail(
                    ArInvoiceErrors.InactiveOrMissingRevenueAccount(accountNo)
                );
            }
        }

        return null;
    }

    private async Task<ArInvoiceCommandResultDto?> ValidateItemsAsync(
        ArInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        Guid[] itemIds =
        [
            .. validatedCommand
                .Lines.Select(x => x.ItemId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct(),
        ];

        foreach (Guid itemId in itemIds)
        {
            Item? item = await repositories.ItemRepository.FindNoTrackingAsync(
                itemId,
                cancellationToken
            );
            if (item is null || !item.IsActive)
            {
                return ArInvoiceCommandResultDto.Fail(
                    ArInvoiceErrors.InactiveOrMissingItem(itemId)
                );
            }
        }

        return null;
    }

    private async Task<ArInvoiceCommandResultDto?> ValidateLocationsAsync(
        ArInvoiceCreateValidationResult validatedCommand,
        CancellationToken cancellationToken
    )
    {
        Guid[] locationIds =
        [
            .. validatedCommand
                .Lines.Select(x => x.LocationId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct(),
        ];

        foreach (Guid locationId in locationIds)
        {
            Location? location = await repositories.LocationRepository.FindNoTrackingAsync(
                locationId,
                cancellationToken
            );
            if (location is null || !location.IsActive)
            {
                return ArInvoiceCommandResultDto.Fail(
                    ArInvoiceErrors.InactiveOrMissingLocation(locationId)
                );
            }
        }

        return null;
    }

    private async Task<ArInvoiceCommandResultDto?> ValidateTaxRatesAsync(
        ArInvoiceCreateValidationResult validatedCommand
    )
    {
        Guid[] taxRateIds =
        [
            .. validatedCommand
                .Lines.Select(x => x.TaxRateId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct(),
        ];

        foreach (Guid taxRateId in taxRateIds)
        {
            TaxRate? taxRate = await repositories.TaxRateRepository.FindNoTrackingAsync(taxRateId);
            if (taxRate is null || !taxRate.IsActive)
            {
                return ArInvoiceCommandResultDto.Fail(
                    ArInvoiceErrors.InactiveOrMissingTaxRate(taxRateId)
                );
            }

            if (taxRate.IsInput)
            {
                return ArInvoiceCommandResultDto.Fail(
                    ArInvoiceErrors.InputTaxRateNotAllowed(taxRateId)
                );
            }
        }

        return null;
    }

    private ArInvoice BuildInvoice(
        CreateArInvoiceCommand command,
        ArInvoiceCreateValidationResult validatedCommand,
        Customer customer
    )
    {
        DateTimeOffset updatedAt = dependencies.Clock.UtcNow;
        DateOnly dueDate = command.DueDate ?? command.InvoiceDate.AddDays(customer.TermsDays);

        return new ArInvoice
        {
            DocNo = validatedCommand.DocNo,
            CustomerId = command.CustomerId,
            InvoiceDate = command.InvoiceDate,
            DueDate = dueDate,
            DocStatus = DocStatus.Draft,
            CurrencyCode = validatedCommand.CurrencyCode,
            ShipTo = validatedCommand.ShipTo,
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
                        new ArInvoiceLine
                        {
                            LineNo = index + 1,
                            Description = line.Description,
                            RevenueAccount = line.RevenueAccount,
                            ItemId = line.ItemId,
                            Qty = line.Qty,
                            UnitPrice = line.UnitPrice,
                            TaxRateId = line.TaxRateId,
                            LocationId = line.LocationId,
                            LineTotal = line.LineTotal,
                        }
                ),
            ],
        };
    }
}
