using Microsoft.EntityFrameworkCore;
using OakERP.Common.Enums;
using OakERP.Domain.Entities.Inventory;
using OakERP.Infrastructure.Persistence;
using OakERP.Infrastructure.Posting.Inventory;
using OakERP.Infrastructure.Repositories.Inventory;
using Shouldly;

namespace OakERP.Tests.Unit.Posting;

public sealed class MovingAverageInventoryCostServiceTests
{
    [Fact]
    public async Task GetUnitCostForSaleAsync_Should_Use_Only_Ledgers_On_Or_Before_PostingDate()
    {
        Guid itemId = Guid.NewGuid();
        Guid locationId = Guid.NewGuid();

        await using var db = CreateDb();
        db.InventoryLedgers.AddRange(
            CreateLedger(
                itemId,
                locationId,
                DaysFromToday(-30),
                5m,
                10m,
                50m,
                UtcAtHourDaysFromToday(-30, 8)
            ),
            CreateLedger(
                itemId,
                locationId,
                DaysFromToday(-20),
                5m,
                20m,
                100m,
                UtcAtHourDaysFromToday(-20, 8)
            )
        );
        await db.SaveChangesAsync();

        var service = new MovingAverageInventoryCostService(new InventoryLedgerRepository(db));

        decimal unitCost = await service.GetUnitCostForSaleAsync(
            itemId,
            locationId,
            DaysFromToday(-25)
        );

        unitCost.ShouldBe(10m);
    }

    [Fact]
    public async Task GetUnitCostForSaleAsync_Should_Order_SameDate_Ledgers_By_CreatedAt_Then_Id()
    {
        Guid itemId = Guid.NewGuid();
        Guid locationId = Guid.NewGuid();
        Guid earlierId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid laterId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        await using var db = CreateDb();
        db.InventoryLedgers.AddRange(
            new InventoryLedger
            {
                Id = laterId,
                ItemId = itemId,
                LocationId = locationId,
                TrxDate = DaysFromToday(-30),
                TransactionType = InventoryTransactionType.Issue,
                Qty = -5m,
                UnitCost = 20m,
                ValueChange = -100m,
                SourceType = "TEST",
                SourceId = Guid.NewGuid(),
                CreatedAt = UtcAtHourDaysFromToday(-30, 8),
            },
            new InventoryLedger
            {
                Id = earlierId,
                ItemId = itemId,
                LocationId = locationId,
                TrxDate = DaysFromToday(-30),
                TransactionType = InventoryTransactionType.Receipt,
                Qty = 5m,
                UnitCost = 10m,
                ValueChange = 50m,
                SourceType = "TEST",
                SourceId = Guid.NewGuid(),
                CreatedAt = UtcAtHourDaysFromToday(-30, 8),
            }
        );
        await db.SaveChangesAsync();

        var service = new MovingAverageInventoryCostService(new InventoryLedgerRepository(db));

        decimal unitCost = await service.GetUnitCostForSaleAsync(
            itemId,
            locationId,
            DaysFromToday(-30)
        );

        unitCost.ShouldBe(10m);
    }

    [Fact]
    public async Task GetUnitCostForSaleAsync_Should_Return_LastKnown_Cost_When_Running_Qty_Reaches_Zero()
    {
        Guid itemId = Guid.NewGuid();
        Guid locationId = Guid.NewGuid();

        await using var db = CreateDb();
        db.InventoryLedgers.AddRange(
            CreateLedger(
                itemId,
                locationId,
                DaysFromToday(-30),
                5m,
                10m,
                50m,
                UtcAtHourDaysFromToday(-30, 8)
            ),
            CreateLedger(
                itemId,
                locationId,
                DaysFromToday(-29),
                -5m,
                10m,
                -50m,
                UtcAtHourDaysFromToday(-29, 8)
            )
        );
        await db.SaveChangesAsync();

        var service = new MovingAverageInventoryCostService(new InventoryLedgerRepository(db));

        decimal unitCost = await service.GetUnitCostForSaleAsync(
            itemId,
            locationId,
            DaysFromToday(-28)
        );

        unitCost.ShouldBe(10m);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static InventoryLedger CreateLedger(
        Guid itemId,
        Guid locationId,
        DateOnly trxDate,
        decimal qty,
        decimal unitCost,
        decimal valueChange,
        DateTimeOffset createdAt
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            LocationId = locationId,
            TrxDate = trxDate,
            TransactionType =
                qty > 0 ? InventoryTransactionType.Receipt : InventoryTransactionType.Issue,
            Qty = qty,
            UnitCost = unitCost,
            ValueChange = valueChange,
            SourceType = "TEST",
            SourceId = Guid.NewGuid(),
            CreatedAt = createdAt,
        };
}
