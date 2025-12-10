using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IHistoryRepository
{
    Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}




