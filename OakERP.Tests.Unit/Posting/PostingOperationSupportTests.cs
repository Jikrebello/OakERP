using Moq;
using OakERP.Application.Posting.Support;
using OakERP.Common.Enums;
using OakERP.Common.Errors;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class PostingOperationSupportTests
{
    [Fact]
    public void PostingInvariantViolationException_Should_Default_To_Unexpected_FailureKind()
    {
        var exception = new PostingInvariantViolationException("Unexpected posting failure.");

        exception.FailureKind.ShouldBe(FailureKind.Unexpected);
        exception.Title.ShouldBe("Posting invariant was violated.");
    }

    [Fact]
    public void EnsureDraftStatus_Should_Classify_NonDraft_Documents_As_Conflict()
    {
        var exception = Should.Throw<PostingInvariantViolationException>(() =>
            PostingOperationSupport.EnsureDraftStatus(
                DocStatus.Posted,
                "Only draft AP invoices can be posted."
            )
        );

        exception.FailureKind.ShouldBe(FailureKind.Conflict);
    }

    [Fact]
    public void EnsureBaseCurrency_Should_Classify_NonBase_Currency_As_Validation()
    {
        var exception = Should.Throw<PostingInvariantViolationException>(() =>
            PostingOperationSupport.EnsureBaseCurrency(
                "USD",
                "ZAR",
                "AP invoice posting currently supports only invoices in the base currency."
            )
        );

        exception.FailureKind.ShouldBe(FailureKind.Validation);
    }

    [Fact]
    public async Task GetOpenPeriodAsync_Should_Classify_Missing_Open_Period_As_Conflict()
    {
        var factory = new PostingServiceTestFactory();
        var postingDate = DaysFromToday(40);

        factory
            .FiscalPeriodRepository.Setup(x =>
                x.GetOpenForDateAsync(postingDate, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Domain.Entities.GeneralLedger.FiscalPeriod?)null);

        var support = CreateSupport(factory);

        var exception = await Should.ThrowAsync<PostingInvariantViolationException>(() =>
            support.GetOpenPeriodAsync(postingDate, CancellationToken.None)
        );

        exception.FailureKind.ShouldBe(FailureKind.Conflict);
    }

    private static PostingOperationSupport CreateSupport(PostingServiceTestFactory factory)
    {
        var persistenceDependencies = new PostingPersistenceDependencies(
            factory.FiscalPeriodRepository.Object,
            factory.GlAccountRepository.Object,
            factory.GlEntryRepository.Object,
            factory.InventoryLedgerRepository.Object,
            factory.UnitOfWork.Object,
            factory.PersistenceFailureClassifier.Object
        );

        return new PostingOperationSupport(
            persistenceDependencies,
            new PostingRuntimeDependencies(
                factory.GlSettingsProvider.Object,
                factory.PostingRuleProvider.Object,
                factory.PostingEngine.Object,
                factory.Clock.Object
            ),
            new PostingResultProcessor(persistenceDependencies)
        );
    }
}
