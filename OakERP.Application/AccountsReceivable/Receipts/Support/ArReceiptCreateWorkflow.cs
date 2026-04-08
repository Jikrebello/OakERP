using Microsoft.Extensions.Logging;
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
    ArReceiptServiceDependencies dependencies,
    ILogger<Services.ArReceiptService> logger
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

            var customer = await customerRepository.FindNoTrackingAsync(
                command.CustomerId,
                cancellationToken
            );
            if (customer is null)
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.CustomerNotFound);
            }

            if (!customer.IsActive)
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.CustomerInactive);
            }

            var bankAccount = await bankAccountRepository.FindNoTrackingAsync(
                command.BankAccountId,
                cancellationToken
            );
            if (bankAccount is null)
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.BankAccountNotFound);
            }

            if (!bankAccount.IsActive)
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.BankAccountInactive);
            }

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    bankAccount.CurrencyCode,
                    validatedCommand.CurrencyCode
                )
            )
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.ReceiptCurrencyMismatch);
            }

            bool docNoExists = await arReceiptRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            );
            if (docNoExists)
            {
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.DuplicateDocumentNumber);
            }

            await dependencies.UnitOfWork.BeginTransactionAsync();

            try
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
                        cancellationToken
                    );
                if (invoiceLoadFailure is not null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return invoiceLoadFailure;
                }

                var (allocationFailure, settledAmounts, receiptAllocations) =
                    await SettlementAllocationApplicator.ApplyAsync(
                        ArReceiptSettlementAdapters.CreateAllocationInputs(validatedCommand.Allocations),
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
                    await dependencies.UnitOfWork.RollbackAsync();
                    return allocationFailure;
                }

                await dependencies.UnitOfWork.SaveChangesAsync(cancellationToken);
                await dependencies.UnitOfWork.CommitAsync();

                return ArReceiptSnapshotFactory.BuildSuccess(
                    receipt,
                    invoiceLoadInvoices!.Values,
                    "AR receipt created successfully.",
                    settledAmounts,
                    receiptAllocations
                );
            }
            catch (Exception ex)
            {
                await dependencies.UnitOfWork.RollbackAsync();
                if (dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(ex))
                {
                    logger.LogWarning(
                        ex,
                        "Concurrency failure while creating AR receipt {DocNo}",
                        validatedCommand.DocNo
                    );
                    return ArReceiptCommandResultDto.Fail(
                        ArReceiptErrors.AllocationConcurrencyConflict
                    );
                }

                if (dependencies.PersistenceFailureClassifier.IsArReceiptDocNoConflict(ex))
                {
                    return ArReceiptCommandResultDto.Fail(ArReceiptErrors.DuplicateDocumentNumber);
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while creating AR receipt {DocNo}",
                    validatedCommand.DocNo
                );
                return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedCreateFailure);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AR receipt.");
            return ArReceiptCommandResultDto.Fail(ArReceiptErrors.UnexpectedCreateFailure);
        }
    }
}
