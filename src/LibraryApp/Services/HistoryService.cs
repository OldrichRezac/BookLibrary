using LibraryApp.Models;

namespace LibraryApp.Services;

public class HistoryService : IHistoryService
{
    private readonly IHistoryRepository _repository;

    public HistoryService(IHistoryRepository repository)
    {
        _repository = repository;
    }

    public Task AddLoanAsync(Book book, CancellationToken cancellationToken = default) =>
        AddEntryAsync(book, LoanAction.Loan, cancellationToken);

    public Task AddReturnAsync(Book book, CancellationToken cancellationToken = default) =>
        AddEntryAsync(book, LoanAction.Return, cancellationToken);

    public async Task<IReadOnlyList<LoanHistoryEntry>> GetAsync(HistorySearchModel? search = null, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        IEnumerable<LoanHistoryEntry> query = items;

        if (search is not null)
        {
            if (!string.IsNullOrWhiteSpace(search.Title))
            {
                query = query.Where(e => e.Title.Contains(search.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search.Isbn))
            {
                query = query.Where(e => e.Isbn.Contains(search.Isbn, StringComparison.OrdinalIgnoreCase));
            }

            if (search.Action.HasValue)
            {
                query = query.Where(e => e.Action == search.Action.Value);
            }

            if (search.FromUtc.HasValue)
            {
                query = query.Where(e => e.OccurredAtUtc >= search.FromUtc.Value);
            }

            if (search.ToUtc.HasValue)
            {
                query = query.Where(e => e.OccurredAtUtc <= search.ToUtc.Value);
            }
        }

        return query
            .OrderByDescending(e => e.OccurredAtUtc)
            .ThenBy(e => e.Title)
            .ToList();
    }

    private Task AddEntryAsync(Book book, LoanAction action, CancellationToken cancellationToken)
    {
        var entry = new LoanHistoryEntry
        {
            BookId = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            Action = action,
            OccurredAtUtc = DateTime.UtcNow
        };

        return _repository.AddAsync(entry, cancellationToken);
    }
}




