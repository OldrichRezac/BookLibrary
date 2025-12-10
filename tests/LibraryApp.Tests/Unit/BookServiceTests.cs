using LibraryApp.Models;
using LibraryApp.Services;
using Xunit;

namespace LibraryApp.Tests;

public class BookServiceTests
{
    private readonly InMemoryBookRepository _repository = new();

    [Fact]
    public async Task AddBookAsync_AddsBook()
    {
        var service = CreateService();
        var input = new BookInputModel
        {
            Title = "Test Book",
            Author = "Tester",
            Isbn = "12345",
            PublishedYear = 2020,
            TotalCopies = 3,
            AvailableCopies = 2
        };

        var (success, _, created) = await service.AddBookAsync(input);

        Assert.True(success);
        Assert.NotNull(created);
        Assert.Equal(1, (await _repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddBookAsync_DuplicateIsbnFails()
    {
        var service = CreateService();
        var first = new BookInputModel { Title = "One", Author = "A", Isbn = "dup", TotalCopies = 1, AvailableCopies = 1 };
        var second = new BookInputModel { Title = "Two", Author = "B", Isbn = "dup", TotalCopies = 1, AvailableCopies = 1 };

        await service.AddBookAsync(first);
        var (success, error, _) = await service.AddBookAsync(second);

        Assert.False(success);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public async Task LoanBookAsync_DecrementsWhenAvailable()
    {
        var service = CreateService();
        var book = await SeedBookAsync(new Book { Title = "Loanable", Author = "A", Isbn = "L-1", TotalCopies = 1, AvailableCopies = 1 });

        var result = await service.LoanBookAsync(book.Id);

        Assert.True(result);
        var stored = await _repository.GetByIdAsync(book.Id);
        Assert.Equal(0, stored?.AvailableCopies);
    }

    [Fact]
    public async Task LoanBookAsync_FailsWhenNoneAvailable()
    {
        var service = CreateService();
        var book = await SeedBookAsync(new Book { Title = "Empty", Author = "A", Isbn = "L-0", TotalCopies = 0, AvailableCopies = 0 });

        var result = await service.LoanBookAsync(book.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ReturnBookAsync_IncrementsCopies()
    {
        var service = CreateService();
        var book = await SeedBookAsync(new Book { Title = "Returnable", Author = "A", Isbn = "R-1", TotalCopies = 1, AvailableCopies = 0 });

        var result = await service.ReturnBookAsync(book.Id);

        Assert.True(result);
        var stored = await _repository.GetByIdAsync(book.Id);
        Assert.Equal(1, stored?.AvailableCopies);
    }

    [Fact]
    public async Task Search_FiltersByTitleAuthorIsbn()
    {
        var service = CreateService();
        await SeedBookAsync(new Book { Title = "C# in Depth", Author = "Jon Skeet", Isbn = "111", TotalCopies = 1, AvailableCopies = 1 });
        await SeedBookAsync(new Book { Title = "CLR via C#", Author = "Jeffrey Richter", Isbn = "222", TotalCopies = 1, AvailableCopies = 1 });

        var results = await service.GetAllAsync(new BookSearchModel { Author = "skeet" });

        Assert.Single(results);
        Assert.Equal("Jon Skeet", results.First().Author);
    }

    [Fact]
    public async Task Search_FiltersByTitle_CaseInsensitivePartial()
    {
        var service = CreateService();
        await SeedBookAsync(new Book { Title = "Pán prstenů", Author = "Tolkien", Isbn = "A1", TotalCopies = 1, AvailableCopies = 1 });
        await SeedBookAsync(new Book { Title = "Hobit", Author = "Tolkien", Isbn = "A2", TotalCopies = 1, AvailableCopies = 1 });

        var results = await service.GetAllAsync(new BookSearchModel { Title = "PRST" });

        Assert.Single(results);
        Assert.Equal("Pán prstenů", results.First().Title);
    }

    [Fact]
    public async Task Search_FiltersByIsbnPartial()
    {
        var service = CreateService();
        await SeedBookAsync(new Book { Title = "A", Author = "X", Isbn = "978-80-123", TotalCopies = 1, AvailableCopies = 1 });
        await SeedBookAsync(new Book { Title = "B", Author = "Y", Isbn = "111-22-333", TotalCopies = 1, AvailableCopies = 1 });

        var results = await service.GetAllAsync(new BookSearchModel { Isbn = "978-80" });

        Assert.Single(results);
        Assert.Equal("A", results.First().Title);
    }

    [Fact]
    public async Task Search_CombinesFiltersWithAnd()
    {
        var service = CreateService();
        await SeedBookAsync(new Book { Title = "C# in Depth", Author = "Jon Skeet", Isbn = "111", TotalCopies = 1, AvailableCopies = 1 });
        await SeedBookAsync(new Book { Title = "C# in Depth", Author = "Other Author", Isbn = "222", TotalCopies = 1, AvailableCopies = 1 });

        var results = await service.GetAllAsync(new BookSearchModel { Title = "depth", Author = "skeet" });

        Assert.Single(results);
        Assert.Equal("Jon Skeet", results.First().Author);
    }

    [Fact]
    public async Task AddBookAsync_RejectsNegativeAvailable()
    {
        var service = CreateService();
        var input = new BookInputModel { Title = "A", Author = "B", Isbn = "1", TotalCopies = 1, AvailableCopies = -1 };

        var (success, error, _) = await service.AddBookAsync(input);

        Assert.False(success);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public async Task AddBookAsync_RejectsNegativeTotal()
    {
        var service = CreateService();
        var input = new BookInputModel { Title = "A", Author = "B", Isbn = "1", TotalCopies = -1, AvailableCopies = 0 };

        var (success, error, _) = await service.AddBookAsync(input);

        Assert.False(success);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public async Task AddBookAsync_RejectsAvailableGreaterThanTotal()
    {
        var service = CreateService();
        var input = new BookInputModel { Title = "A", Author = "B", Isbn = "1", TotalCopies = 1, AvailableCopies = 2 };

        var (success, error, _) = await service.AddBookAsync(input);

        Assert.False(success);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public async Task AddBookAsync_TrimsFields()
    {
        var service = CreateService();
        var input = new BookInputModel
        {
            Title = "  Title  ",
            Author = "  Author  ",
            Isbn = "  999  ",
            TotalCopies = 1,
            AvailableCopies = 1
        };

        var (success, _, created) = await service.AddBookAsync(input);

        Assert.True(success);
        Assert.NotNull(created);
        Assert.Equal("Title", created!.Title);
        Assert.Equal("Author", created.Author);
        Assert.Equal("999", created.Isbn);
    }

    [Fact]
    public async Task LoanBookAsync_ReturnsFalseWhenNotFound()
    {
        var service = CreateService();

        var result = await service.LoanBookAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task LoanBookAsync_AddsHistoryEntryOnSuccess()
    {
        var history = new InMemoryHistoryService();
        var service = new BookService(_repository, history);
        var book = await SeedBookAsync(new Book { Title = "Loan", Author = "A", Isbn = "L-1", TotalCopies = 1, AvailableCopies = 1 });

        var result = await service.LoanBookAsync(book.Id);

        Assert.True(result);
        Assert.Single(history.Entries);
        Assert.Equal(LoanAction.Loan, history.Entries[0].Action);
    }

    [Fact]
    public async Task LoanBookAsync_FailsWhenNoCopies()
    {
        var history = new InMemoryHistoryService();
        var service = new BookService(_repository, history);
        var book = await SeedBookAsync(new Book { Title = "Empty", Author = "A", Isbn = "L-0", TotalCopies = 1, AvailableCopies = 0 });

        var result = await service.LoanBookAsync(book.Id);

        Assert.False(result);
        Assert.Empty(history.Entries);
    }

    [Fact]
    public async Task ReturnBookAsync_AddsHistoryEntryOnSuccess()
    {
        var history = new InMemoryHistoryService();
        var service = new BookService(_repository, history);
        var book = await SeedBookAsync(new Book { Title = "Returnable", Author = "A", Isbn = "R-1", TotalCopies = 1, AvailableCopies = 0 });

        var result = await service.ReturnBookAsync(book.Id);

        Assert.True(result);
        Assert.Single(history.Entries);
        Assert.Equal(LoanAction.Return, history.Entries[0].Action);
    }

    [Fact]
    public async Task ReturnBookAsync_FailsWhenFull()
    {
        var history = new InMemoryHistoryService();
        var service = new BookService(_repository, history);
        var book = await SeedBookAsync(new Book { Title = "Full", Author = "A", Isbn = "R-0", TotalCopies = 1, AvailableCopies = 1 });

        var result = await service.ReturnBookAsync(book.Id);

        Assert.False(result);
        Assert.Empty(history.Entries);
    }

    private BookService CreateService() => new(_repository, new InMemoryHistoryService());

    private async Task<Book> SeedBookAsync(Book book)
    {
        await _repository.AddAsync(book);
        return book;
    }

    private class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = new();

        public Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            _books.Add(book);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Book>>(_books.ToList());

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Book?>(_books.FirstOrDefault(b => b.Id == id));

        public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
            => Task.FromResult<Book?>(_books.FirstOrDefault(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> UpdateCopiesAsync(Guid id, int delta, CancellationToken cancellationToken = default)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book is null)
            {
                return Task.FromResult(false);
            }

            if (delta < 0 && book.AvailableCopies + delta < 0)
            {
                return Task.FromResult(false);
            }

            if (delta > 0 && book.AvailableCopies + delta > book.TotalCopies)
            {
                return Task.FromResult(false);
            }

            book.AvailableCopies += delta;
            return Task.FromResult(true);
        }
    }

    private class InMemoryHistoryService : IHistoryService
    {
        public List<LoanHistoryEntry> Entries { get; } = new();

        public Task AddLoanAsync(Book book, CancellationToken cancellationToken = default)
            => AddAsync(book, LoanAction.Loan);

        public Task AddReturnAsync(Book book, CancellationToken cancellationToken = default)
            => AddAsync(book, LoanAction.Return);

        public Task<IReadOnlyList<LoanHistoryEntry>> GetAsync(HistorySearchModel? search = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<LoanHistoryEntry>>(Entries.ToList());

        private Task AddAsync(Book book, LoanAction action)
        {
            Entries.Add(new LoanHistoryEntry
            {
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                Isbn = book.Isbn,
                Action = action,
                OccurredAtUtc = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }
    }
}


