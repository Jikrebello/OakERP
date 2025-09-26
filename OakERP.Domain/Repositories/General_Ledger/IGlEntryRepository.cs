using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repositories.General_Ledger;

public interface IGlEntryRepository
{
    Task<GlEntry?> GetByIdAsync(Guid id);

    IQueryable<GlEntry> Query();

    Task CreateAsync(GlEntry glEntry);

    Task UpdateAsync(GlEntry glEntry);

    Task DeleteAsync(GlEntry glEntry);
}
