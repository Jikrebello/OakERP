using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Settlements.Documents;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsReceivable;
using OakERP.Domain.Entities.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

internal sealed class ArReceiptCreateWorkflow(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    ICustomerRepository customerRepository,
    IBankAccountRepository bankAccountRepository,
    SettlementDocumentWorkflowDependencies dependencies,
    ILogger<ArReceiptService> logger
)
{
    public async Task<ArReceiptCommandResultDto> ExecuteAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
            ArReceiptCreateValidationResult validatedCommand =
                ArReceiptCommandValidator.ValidateCreate(command, settings);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            var (_, customerFailure) = await SettlementDocumentPreconditions.LoadActivePartyAsync(
                innerCancellationToken =>
                    customerRepository.FindNoTrackingAsync(
                        command.CustomerId,
                        innerCancellationToken
                    ),
                customer => new SettlementDocumentPartySnapshot(customer.Id, customer.IsActive),
                ArReceiptCommandResultDto.Fail(ArReceiptErrors.CustomerNotFound),
                ArReceiptCommandResultDto.Fail(ArReceiptErrors.CustomerInactive),
                cancellationToken
            );
            if (customerFailure is not null)
            {
                return customerFailure;
            }

            var (bankAccount, bankAccountFailure) =
                await SettlementDocumentPreconditions.LoadActiveBankAccountAsync(
                    innerCancellationToken =>
                        bankAccountRepository.FindNoTrackingAsync(
                            command.BankAccountId,
                            innerCancellationToken
                        ),
                    entity => new SettlementDocumentBankAccountSnapshot(
                        entity.Id,
                        entity.IsActive,
                        entity.CurrencyCode
                    ),
                    ArReceiptCommandResultDto.Fail(ArReceiptErrors.BankAccountNotFound),
                    ArReceiptCommandResultDto.Fail(ArReceiptErrors.BankAccountInactive),
                    cancellationToken
                );
            if (bankAccountFailure is not null)
            {
                return bankAccountFailure;
            }

            ArReceiptCommandResultDto? currencyFailure =
                SettlementDocumentPreconditions.EnsureCurrency(
                    bankAccount!.CurrencyCode,
                    validatedCommand.CurrencyCode,
                    ArSettlementCalculator.MatchesCurrency,
                    ArReceiptCommandResultDto.Fail(ArReceiptErrors.ReceiptCurrencyMismatch)
                );
            if (currencyFailure is not null)
            {
                return currencyFailure;
            }

            ArReceiptCommandResultDto? docNoFailure =
                await SettlementDocumentPreconditions.EnsureDocumentNumberAvailableAsync(
                    innerCancellationToken =>
                        arReceiptRepository.ExistsDocNoAsync(
                            validatedCommand.DocNo,
                            innerCancellationToken
                        ),
                    ArReceiptCommandResultDto.Fail(ArReceiptErrors.DuplicateDocumentNumber),
                    cancellationToken
                );
            if (docNoFailure is not null)
            {
                return docNoFailure;
            }

            return await dependencies.CreateWorkflowRunner.ExecuteAsync(
                async innerCancellationToken =>
                {
                    var receipt = new ArReceipt
                    {
                        DocNo = validatedCommand.DocNo,
                        CustomerId = command.CustomerId,
                        BankAccountId = command.BankAccountId,
                        ReceiptDate = command.ReceiptDate,
                        Amount = command.Amount,
                        CurrencyCode = validatedCommand.CurrencyCode,
                        DocStatus = DocStatus.Draft,
                        Memo = validatedCommand.Memo,
                        CreatedBy = validatedCommand.PerformedBy,
                        UpdatedBy = validatedCommand.PerformedBy,
                        UpdatedAt = dependencies.Clock.UtcNow,
                    };

                    await arReceiptRepository.AddAsync(receipt);

                    var (invoiceLoadInvoices, invoiceLoadFailure) =
                        await SettlementInvoiceLoader.LoadAsync(
                            [.. validatedCommand.Allocations.Select(x => x.ArInvoiceId)],
                            ArReceiptSettlementAdapters.CreateInvoiceLoadSpec(
                                arInvoiceRepository,
                                receipt.CustomerId,
                                validatedCommand.CurrencyCode
                            ),
                            innerCancellationToken
                        );
                    if (invoiceLoadFailure is not null)
                    {
                        return invoiceLoadFailure;
                    }

                    var (allocationFailure, settledAmounts, receiptAllocations) =
                        await SettlementAllocationApplicator.ApplyAsync(
                            ArReceiptSettlementAdapters.CreateAllocationInputs(
                                validatedCommand.Allocations
                            ),
                            command.AllocationDate ?? command.ReceiptDate,
                            validatedCommand.PerformedBy,
                            dependencies.Clock.UtcNow,
                            ArReceiptSettlementAdapters.CreateAllocationApplySpec(
                                receipt,
                                invoiceLoadInvoices!,
                                arReceiptAllocationRepository
                            )
                        );
                    if (allocationFailure is not null)
                    {
                        return allocationFailure;
                    }

                    return ArReceiptSnapshotFactory.BuildSuccess(
                        receipt,
                        invoiceLoadInvoices!.Values,
                        "AR receipt created successfully.",
                        settledAmounts,
                        receiptAllocations
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ArReceiptCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                new WorkflowExceptionRule<ArReceiptCommandResultDto>(
                    innerException =>
                        dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(
                            innerException
                        ),
                    innerException =>
                    {
                        logger.LogWarning(
                            innerException,
                            "Concurrency failure while creating AR receipt {DocNo}",
                            command.DocNo
                        );
                        return ArReceiptCommandResultDto.Fail(
                            ArReceiptErrors.AllocationConcurrencyConflict
                        );
                    }
                ),
                new WorkflowExceptionRule<ArReceiptCommandResultDto>(
                    innerException =>
                        dependencies.PersistenceFailureClassifier.IsArReceiptDocNoConflict(
                            innerException
                        ),
                    _ => ArReceiptCommandResultDto.Fail(ArReceiptErrors.DuplicateDocumentNumber)
                )
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(
                ex,
                "Unexpected failure before creating AR receipt {DocNo}",
                command.DocNo
            );
            return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedCreateFailure);
        }
    }
}
