using LibraryApp.Business;

namespace LibraryApp.DataAccess;

public interface IHistoryRepository
{
    Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}




