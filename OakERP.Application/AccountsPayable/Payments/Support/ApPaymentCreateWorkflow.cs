using Microsoft.Extensions.Logging;
using OakERP.Application.Common.Orchestration;
using OakERP.Application.Settlements.Documents;
using OakERP.Common.Enums;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.Bank;

namespace OakERP.Application.AccountsPayable.Payments.Support;

internal sealed class ApPaymentCreateWorkflow(
    IApPaymentRepository apPaymentRepository,
    IApPaymentAllocationRepository apPaymentAllocationRepository,
    IApInvoiceRepository apInvoiceRepository,
    IVendorRepository vendorRepository,
    IBankAccountRepository bankAccountRepository,
    SettlementDocumentWorkflowDependencies dependencies,
    ILogger<ApPaymentService> logger
)
{
    public async Task<ApPaymentCommandResultDto> ExecuteAsync(
        CreateApPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ApPaymentCreateValidationResult validatedCommand =
                ApPaymentCommandValidator.ValidateCreate(command);
            if (validatedCommand.Failure is not null)
            {
                return validatedCommand.Failure;
            }

            GlPostingSettings settings = await dependencies.GlSettingsProvider.GetSettingsAsync(
                cancellationToken
            );

            var (_, vendorFailure) = await SettlementDocumentPreconditions.LoadActivePartyAsync(
                innerCancellationToken =>
                    vendorRepository.FindNoTrackingAsync(command.VendorId, innerCancellationToken),
                vendor => new SettlementDocumentPartySnapshot(vendor.Id, vendor.IsActive),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.VendorNotFound),
                ApPaymentCommandResultDto.Fail(ApPaymentErrors.VendorInactive),
                cancellationToken
            );
            if (vendorFailure is not null)
            {
                return vendorFailure;
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
                    ApPaymentCommandResultDto.Fail(ApPaymentErrors.BankAccountNotFound),
                    ApPaymentCommandResultDto.Fail(ApPaymentErrors.BankAccountInactive),
                    cancellationToken
                );
            if (bankAccountFailure is not null)
            {
                return bankAccountFailure;
            }

            ApPaymentCommandResultDto? currencyFailure =
                SettlementDocumentPreconditions.EnsureCurrency(
                    bankAccount!.CurrencyCode,
                    settings.BaseCurrencyCode,
                    ApSettlementCalculator.MatchesCurrency,
                    ApPaymentCommandResultDto.Fail(ApPaymentErrors.BaseCurrencyOnly)
                );
            if (currencyFailure is not null)
            {
                return currencyFailure;
            }

            ApPaymentCommandResultDto? docNoFailure =
                await SettlementDocumentPreconditions.EnsureDocumentNumberAvailableAsync(
                    innerCancellationToken =>
                        apPaymentRepository.ExistsDocNoAsync(
                            validatedCommand.DocNo,
                            innerCancellationToken
                        ),
                    ApPaymentCommandResultDto.Fail(ApPaymentErrors.DuplicateDocumentNumber),
                    cancellationToken
                );
            if (docNoFailure is not null)
            {
                return docNoFailure;
            }

            return await dependencies.CreateWorkflowRunner.ExecuteAsync(
                async innerCancellationToken =>
                {
                    var payment = new ApPayment
                    {
                        DocNo = validatedCommand.DocNo,
                        VendorId = command.VendorId,
                        BankAccountId = command.BankAccountId,
                        PaymentDate = command.PaymentDate,
                        Amount = command.Amount,
                        DocStatus = DocStatus.Draft,
                        PostingDate = null,
                        Memo = validatedCommand.Memo,
                        CreatedBy = validatedCommand.PerformedBy,
                        UpdatedBy = validatedCommand.PerformedBy,
                        UpdatedAt = dependencies.Clock.UtcNow,
                    };

                    await apPaymentRepository.AddAsync(payment);

                    var (invoiceLoadInvoices, invoiceLoadFailure) =
                        await SettlementInvoiceLoader.LoadAsync(
                            [.. validatedCommand.Allocations.Select(x => x.ApInvoiceId)],
                            ApPaymentSettlementAdapters.CreateInvoiceLoadSpec(
                                apInvoiceRepository,
                                payment.VendorId,
                                settings.BaseCurrencyCode
                            ),
                            innerCancellationToken
                        );
                    if (invoiceLoadFailure is not null)
                    {
                        return invoiceLoadFailure;
                    }

                    var (allocationFailure, settledAmounts, paymentAllocations) =
                        await SettlementAllocationApplicator.ApplyAsync(
                            ApPaymentSettlementAdapters.CreateAllocationInputs(
                                validatedCommand.Allocations
                            ),
                            command.AllocationDate ?? command.PaymentDate,
                            validatedCommand.PerformedBy,
                            dependencies.Clock.UtcNow,
                            ApPaymentSettlementAdapters.CreateAllocationApplySpec(
                                payment,
                                invoiceLoadInvoices!,
                                apPaymentAllocationRepository
                            )
                        );
                    if (allocationFailure is not null)
                    {
                        return allocationFailure;
                    }

                    return ApPaymentSnapshotFactory.BuildSuccess(
                        payment,
                        invoiceLoadInvoices!.Values,
                        "AP payment created successfully.",
                        settledAmounts,
                        paymentAllocations
                    );
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            ApPaymentCommandResultDto? translatedFailure = WorkflowFailureTranslator.TryTranslate(
                ex,
                new WorkflowExceptionRule<ApPaymentCommandResultDto>(
                    innerException =>
                        dependencies.PersistenceFailureClassifier.IsConcurrencyConflict(
                            innerException
                        ),
                    innerException =>
                    {
                        logger.LogWarning(
                            innerException,
                            "Concurrency failure while creating AP payment {DocNo}",
                            command.DocNo
                        );
                        return ApPaymentCommandResultDto.Fail(
                            ApPaymentErrors.AllocationConcurrencyConflict
                        );
                    }
                ),
                new WorkflowExceptionRule<ApPaymentCommandResultDto>(
                    innerException =>
                        dependencies.PersistenceFailureClassifier.IsApPaymentDocNoConflict(
                            innerException
                        ),
                    _ => ApPaymentCommandResultDto.Fail(ApPaymentErrors.DuplicateDocumentNumber)
                )
            );
            if (translatedFailure is not null)
            {
                return translatedFailure;
            }

            logger.LogError(
                ex,
                "Unexpected failure before creating AP payment {DocNo}",
                command.DocNo
            );
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.UnexpectedCreateFailure);
        }
    }
}
