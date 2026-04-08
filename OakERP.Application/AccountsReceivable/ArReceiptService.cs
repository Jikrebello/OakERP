using System.Net;
using Microsoft.Extensions.Logging;
using OakERP.Application.Settlements;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.Bank;

namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptService(
    IArReceiptRepository arReceiptRepository,
    IArReceiptAllocationRepository arReceiptAllocationRepository,
    IArInvoiceRepository arInvoiceRepository,
    ICustomerRepository customerRepository,
    IBankAccountRepository bankAccountRepository,
    ArReceiptServiceDependencies dependencies,
    ILogger<ArReceiptService> logger
) : IArReceiptService
{
    public async Task<ArReceiptCommandResultDto> CreateAsync(
        CreateArReceiptCommand command,
        CancellationToken cancellationToken = default
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
                return ArReceiptCommandResultDto.Fail(
                    "Customer was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!customer.IsActive)
            {
                return ArReceiptCommandResultDto.Fail(
                    "AR receipts can be created only for active customers.",
                    HttpStatusCode.BadRequest
                );
            }

            var bankAccount = await bankAccountRepository.FindNoTrackingAsync(
                command.BankAccountId,
                cancellationToken
            );
            if (bankAccount is null)
            {
                return ArReceiptCommandResultDto.Fail(
                    "Bank account was not found.",
                    HttpStatusCode.NotFound
                );
            }

            if (!bankAccount.IsActive)
            {
                return ArReceiptCommandResultDto.Fail(
                    "AR receipts can be created only against active bank accounts.",
                    HttpStatusCode.BadRequest
                );
            }

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    bankAccount.CurrencyCode,
                    validatedCommand.CurrencyCode
                )
            )
            {
                return ArReceiptCommandResultDto.Fail(
                    "Bank account currency must match the receipt currency.",
                    HttpStatusCode.BadRequest
                );
            }

            bool docNoExists = await arReceiptRepository.ExistsDocNoAsync(
                validatedCommand.DocNo,
                cancellationToken
            );
            if (docNoExists)
            {
                return ArReceiptCommandResultDto.Fail(
                    "An AR receipt with this document number already exists.",
                    HttpStatusCode.Conflict
                );
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
                    UpdatedAt = DateTimeOffset.UtcNow,
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
                        "The receipt or one of its invoices was modified during allocation.",
                        HttpStatusCode.Conflict
                    );
                }

                if (
                    dependencies.PersistenceFailureClassifier.IsUniqueConstraint(
                        ex,
                        "ix_ar_receipts_doc_no"
                    )
                )
                {
                    return ArReceiptCommandResultDto.Fail(
                        "An AR receipt with this document number already exists.",
                        HttpStatusCode.Conflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while creating AR receipt {DocNo}",
                    validatedCommand.DocNo
                );
                return ArReceiptCommandResultDto.Fail(
                    "An unexpected error occurred while creating the AR receipt.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure before creating AR receipt.");
            return ArReceiptCommandResultDto.Fail(
                "An unexpected error occurred while creating the AR receipt.",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ArReceiptCommandResultDto> AllocateAsync(
        AllocateArReceiptCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            ArReceiptAllocateValidationResult validatedCommand =
                ArReceiptCommandValidator.ValidateAllocate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            await dependencies.UnitOfWork.BeginTransactionAsync();

            try
            {
                ArReceipt? receipt = await arReceiptRepository.GetTrackedForAllocationAsync(
                    command.ReceiptId,
                    cancellationToken
                );
                if (receipt is null)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(
                        "AR receipt was not found.",
                        HttpStatusCode.NotFound
                    );
                }

                if (receipt.DocStatus != DocStatus.Draft)
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(
                        "Only draft AR receipts can be allocated in this slice.",
                        HttpStatusCode.BadRequest
                    );
                }

                GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                    cancellationToken
                );
                if (
                    !ArSettlementCalculator.MatchesCurrency(
                        receipt.CurrencyCode,
                        settings.BaseCurrencyCode
                    )
                )
                {
                    await dependencies.UnitOfWork.RollbackAsync();
                    return ArReceiptCommandResultDto.Fail(
                        "AR receipt allocation currently supports only receipts in the base currency.",
                        HttpStatusCode.BadRequest
                    );
                }

                var (invoiceLoadInvoices, invoiceLoadFailure) =
                    await SettlementInvoiceLoader.LoadAsync(
                        [.. validatedCommand.Allocations.Select(x => x.ArInvoiceId)],
                        ArReceiptSettlementAdapters.CreateInvoiceLoadSpec(
                            arInvoiceRepository,
                            receipt.CustomerId,
                            receipt.CurrencyCode
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
                        command.AllocationDate ?? receipt.ReceiptDate,
                        validatedCommand.PerformedBy,
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
                    "AR receipt allocations saved successfully.",
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
                        "Concurrency failure while allocating AR receipt {ReceiptId}",
                        command.ReceiptId
                    );
                    return ArReceiptCommandResultDto.Fail(
                        "The receipt or one of its invoices was modified during allocation.",
                        HttpStatusCode.Conflict
                    );
                }

                logger.LogError(
                    ex,
                    "Unexpected failure while allocating AR receipt {ReceiptId}",
                    command.ReceiptId
                );
                return ArReceiptCommandResultDto.Fail(
                    "An unexpected error occurred while allocating the AR receipt.",
                    HttpStatusCode.InternalServerError
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected failure before allocating AR receipt {ReceiptId}",
                command.ReceiptId
            );
            return ArReceiptCommandResultDto.Fail(
                "An unexpected error occurred while allocating the AR receipt.",
                HttpStatusCode.InternalServerError
            );
        }
    }
}
