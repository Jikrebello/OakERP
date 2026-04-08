using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.AccountsReceivable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.RepositoryInterfaces.AccountsReceivable;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Application.Posting.Support;

public sealed class PostingSourceRepositories(
    IApPaymentRepository apPaymentRepository,
    IApInvoiceRepository apInvoiceRepository,
    IArInvoiceRepository arInvoiceRepository,
    IArReceiptRepository arReceiptRepository
)
{
    public IApPaymentRepository ApPaymentRepository { get; } = apPaymentRepository;

    public IApInvoiceRepository ApInvoiceRepository { get; } = apInvoiceRepository;

    public IArInvoiceRepository ArInvoiceRepository { get; } = arInvoiceRepository;

    public IArReceiptRepository ArReceiptRepository { get; } = arReceiptRepository;
}

public sealed class PostingPersistenceDependencies(
    IFiscalPeriodRepository fiscalPeriodRepository,
    IGlAccountRepository glAccountRepository,
    IGlEntryRepository glEntryRepository,
    IInventoryLedgerRepository inventoryLedgerRepository,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier persistenceFailureClassifier
)
{
    public IFiscalPeriodRepository FiscalPeriodRepository { get; } = fiscalPeriodRepository;

    public IGlAccountRepository GlAccountRepository { get; } = glAccountRepository;

    public IGlEntryRepository GlEntryRepository { get; } = glEntryRepository;

    public IInventoryLedgerRepository InventoryLedgerRepository { get; } =
        inventoryLedgerRepository;

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;

    public IPersistenceFailureClassifier PersistenceFailureClassifier { get; } =
        persistenceFailureClassifier;
}

public sealed class PostingRuntimeDependencies(
    IGlSettingsProvider glSettingsProvider,
    IPostingRuleProvider postingRuleProvider,
    IPostingEngine postingEngine,
    IClock clock
)
{
    public IGlSettingsProvider GlSettingsProvider { get; } = glSettingsProvider;

    public IPostingRuleProvider PostingRuleProvider { get; } = postingRuleProvider;

    public IPostingEngine PostingEngine { get; } = postingEngine;

    public IClock Clock { get; } = clock;
}

public sealed class PostingContextBuilders(
    IApPaymentPostingContextBuilder apPaymentPostingContextBuilder,
    IApInvoicePostingContextBuilder apInvoicePostingContextBuilder,
    IArInvoicePostingContextBuilder arInvoicePostingContextBuilder,
    IArReceiptPostingContextBuilder arReceiptPostingContextBuilder
)
{
    public IApPaymentPostingContextBuilder ApPaymentPostingContextBuilder { get; } =
        apPaymentPostingContextBuilder;

    public IApInvoicePostingContextBuilder ApInvoicePostingContextBuilder { get; } =
        apInvoicePostingContextBuilder;

    public IArInvoicePostingContextBuilder ArInvoicePostingContextBuilder { get; } =
        arInvoicePostingContextBuilder;

    public IArReceiptPostingContextBuilder ArReceiptPostingContextBuilder { get; } =
        arReceiptPostingContextBuilder;
}
