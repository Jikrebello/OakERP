using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repositories.General_Ledger;

public interface IGlJournalLineRepository
{
    Task<GlJournalLine?> GetByIdAsync(Guid id);

    IQueryable<GlJournalLine> Query();

    Task CreateAsync(GlJournalLine glJournalLine);

    Task UpdateAsync(GlJournalLine glJournalLine);

    Task DeleteAsync(GlJournalLine glJournalLine);
}
