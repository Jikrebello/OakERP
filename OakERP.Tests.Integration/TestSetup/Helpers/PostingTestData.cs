using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup.Helpers;

internal static class PostingTestData
{
    public static async Task EnsurePostingPrerequisitesAsync(
        ApplicationDbContext db,
        DateOnly postingDate
    )
    {
        await EnsurePostingAccountsAsync(db);
        await EnsurePostingSettingsAsync(db);
        await EnsureOpenFiscalPeriodAsync(db, postingDate);
        await db.SaveChangesAsync();
    }

    private static async Task EnsurePostingAccountsAsync(ApplicationDbContext db)
    {
        await EnsureGlAccountAsync(db, "1100", "Accounts Receivable", GlAccountType.Asset);
        await EnsureGlAccountAsync(
            db,
            "2000",
            "Accounts Payable",
            GlAccountType.Liability,
            isControl: true
        );
        await EnsureGlAccountAsync(db, "4000", "Revenue", GlAccountType.Revenue);
        await EnsureGlAccountAsync(db, "5000", "Expense", GlAccountType.Expense);
        await EnsureGlAccountAsync(db, "1300", "Inventory", GlAccountType.Asset);
        await EnsureGlAccountAsync(db, "5100", "COGS", GlAccountType.Expense);
        await EnsureGlAccountAsync(db, "2100", "Output VAT", GlAccountType.Liability);
        await EnsureGlAccountAsync(db, "2200", "Input VAT", GlAccountType.Asset);
    }

    private static async Task EnsureGlAccountAsync(
        ApplicationDbContext db,
        string accountNo,
        string name,
        GlAccountType type,
        bool isControl = false
    )
    {
        if (
            db.GlAccounts.Local.Any(x => x.AccountNo == accountNo)
            || await db.GlAccounts.AnyAsync(x => x.AccountNo == accountNo)
        )
        {
            return;
        }

        db.GlAccounts.Add(
            new GlAccount
            {
                AccountNo = accountNo,
                Name = name,
                Type = type,
                IsActive = true,
                IsControl = isControl,
            }
        );
    }

    private static async Task EnsurePostingSettingsAsync(ApplicationDbContext db)
    {
        if (
            db.AppSettings.Local.Any(x => x.Key == GlPostingSettingsKeys.Posting)
            || await db.AppSettings.AnyAsync(x => x.Key == GlPostingSettingsKeys.Posting)
        )
        {
            return;
        }

        db.AppSettings.Add(
            new AppSetting
            {
                Key = GlPostingSettingsKeys.Posting,
                ValueJson = JsonSerializer.Serialize(
                    new GlPostingSettings(
                        "ZAR",
                        "1100",
                        "2000",
                        "4000",
                        "5000",
                        "1300",
                        "5100",
                        "2100",
                        "2200"
                    )
                ),
            }
        );
    }

    private static async Task EnsureOpenFiscalPeriodAsync(
        ApplicationDbContext db,
        DateOnly postingDate
    )
    {
        if (
            db.FiscalPeriods.Local.Any(x =>
                x.FiscalYear == postingDate.Year && x.PeriodNo == postingDate.Month
            )
            || await db.FiscalPeriods.AnyAsync(x =>
                x.FiscalYear == postingDate.Year && x.PeriodNo == postingDate.Month
            )
        )
        {
            return;
        }

        db.FiscalPeriods.Add(
            new FiscalPeriod
            {
                Id = Guid.NewGuid(),
                FiscalYear = postingDate.Year,
                PeriodNo = postingDate.Month,
                PeriodStart = TestDates.StartOfMonth(postingDate),
                PeriodEnd = TestDates.EndOfMonth(postingDate),
                Status = FiscalPeriodStatuses.Open,
            }
        );
    }
}
