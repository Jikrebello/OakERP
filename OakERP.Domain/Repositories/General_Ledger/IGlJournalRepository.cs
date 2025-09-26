using OakERP.Domain.Entities.General_Ledger;

namespace OakERP.Domain.Repositories.General_Ledger;

public interface IGlJournalRepository
{
    Task<GlJournal?> GetByIdAsync(Guid id);

    IQueryable<GlJournal> Query();

    Task CreateAsync(GlJournal glJournal);

    Task UpdateAsync(GlJournal glJournal);

    Task DeleteAsync(GlJournal glJournal);
}
