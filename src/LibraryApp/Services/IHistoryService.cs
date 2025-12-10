using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IHistoryService
{
    Task AddLoanAsync(Book book, CancellationToken cancellationToken = default);
    Task AddReturnAsync(Book book, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanHistoryEntry>> GetAsync(HistorySearchModel? search = null, CancellationToken cancellationToken = default);
}




