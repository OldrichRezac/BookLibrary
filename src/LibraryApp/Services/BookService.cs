using LibraryApp.Models;

namespace LibraryApp.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly IHistoryService _historyService;

    public BookService(IBookRepository repository, IHistoryService historyService)
    {
        _repository = repository;
        _historyService = historyService;
    }

    public async Task<(bool Success, string? Error, Book? Created)> AddBookAsync(BookInputModel input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var isbn = Normalize(input.Isbn);
        if (input.AvailableCopies < 0)
        {
            return (false, "Počet dostupných kusů musí být nezáporný.", null);
        }

        if (input.TotalCopies < 0)
        {
            return (false, "Celkový počet kusů musí být nezáporný.", null);
        }

        if (input.AvailableCopies > input.TotalCopies)
        {
            return (false, "Počet dostupných kusů nemůže být větší než celkový počet.", null);
        }

        var existing = await _repository.GetByIsbnAsync(isbn, cancellationToken);
        if (existing is not null)
        {
            return (false, "Kniha s tímto ISBN již existuje.", null);
        }

        var book = new Book
        {
            Title = Normalize(input.Title),
            Author = Normalize(input.Author),
            Isbn = isbn,
            PublishedYear = input.PublishedYear,
            TotalCopies = input.TotalCopies,
            AvailableCopies = input.AvailableCopies
        };

        await _repository.AddAsync(book, cancellationToken);
        return (true, null, book);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(BookSearchModel? search = null, CancellationToken cancellationToken = default)
    {
        var books = await _repository.GetAllAsync(cancellationToken);
        IEnumerable<Book> query = books;

        if (search is not null)
        {
            if (!string.IsNullOrWhiteSpace(search.Title))
            {
                query = query.Where(b => b.Title.Contains(search.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search.Author))
            {
                query = query.Where(b => b.Author.Contains(search.Author, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search.Isbn))
            {
                query = query.Where(b => b.Isbn.Contains(search.Isbn, StringComparison.OrdinalIgnoreCase));
            }
        }

        return query.OrderBy(b => b.Title).ThenBy(b => b.Author).ToList();
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public async Task<bool> LoanBookAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetByIdAsync(id, cancellationToken);
        if (book is null || book.AvailableCopies <= 0)
        {
            return false;
        }

        var updated = await _repository.UpdateCopiesAsync(id, -1, cancellationToken);
        if (updated)
        {
            await _historyService.AddLoanAsync(book, cancellationToken);
        }

        return updated;
    }

    public async Task<bool> ReturnBookAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetByIdAsync(id, cancellationToken);
        if (book is null || book.AvailableCopies >= book.TotalCopies)
        {
            return false;
        }

        var updated = await _repository.UpdateCopiesAsync(id, 1, cancellationToken);
        if (updated)
        {
            await _historyService.AddReturnAsync(book, cancellationToken);
        }

        return updated;
    }

    private static string Normalize(string value) => value.Trim();
}

