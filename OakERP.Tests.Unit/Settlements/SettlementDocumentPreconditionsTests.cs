using OakERP.Application.Settlements.Documents;
using OakERP.Common.Enums;
using Shouldly;

namespace OakERP.Tests.Unit.Settlements;

public sealed class SettlementDocumentPreconditionsTests
{
    [Fact]
    public async Task LoadActivePartyAsync_Should_Return_NotFound_Failure_When_Party_Is_Missing()
    {
        var (_, failure) = await SettlementDocumentPreconditions.LoadActivePartyAsync<object, string>(
            _ => Task.FromResult<object?>(null),
            _ => new SettlementDocumentPartySnapshot(Guid.NewGuid(), true),
            "missing",
            "inactive",
            CancellationToken.None
        );

        failure.ShouldBe("missing");
    }

    [Fact]
    public async Task LoadActiveBankAccountAsync_Should_Return_Inactive_Failure_When_Bank_Account_Is_Inactive()
    {
        var (_, failure) =
            await SettlementDocumentPreconditions.LoadActiveBankAccountAsync<object, string>(
                _ => Task.FromResult<object?>(new object()),
                _ => new SettlementDocumentBankAccountSnapshot(Guid.NewGuid(), false, "ZAR"),
                "missing",
                "inactive",
                CancellationToken.None
            );

        failure.ShouldBe("inactive");
    }

    [Fact]
    public async Task EnsureDocumentNumberAvailableAsync_Should_Return_Failure_When_Doc_No_Exists()
    {
        string? failure = await SettlementDocumentPreconditions.EnsureDocumentNumberAvailableAsync(
            _ => Task.FromResult(true),
            "duplicate",
            CancellationToken.None
        );

        failure.ShouldBe("duplicate");
    }

    [Fact]
    public void EnsureDraftStatus_Should_Return_Failure_When_Document_Is_Not_Draft()
    {
        string? failure = SettlementDocumentPreconditions.EnsureDraftStatus(
            DocStatus.Posted,
            "not-draft"
        );

        failure.ShouldBe("not-draft");
    }

    [Fact]
    public void EnsureCurrency_Should_Return_Failure_When_Currency_Does_Not_Match()
    {
        string? failure = SettlementDocumentPreconditions.EnsureCurrency(
            "USD",
            "ZAR",
            string.Equals,
            "currency"
        );

        failure.ShouldBe("currency");
    }
}
