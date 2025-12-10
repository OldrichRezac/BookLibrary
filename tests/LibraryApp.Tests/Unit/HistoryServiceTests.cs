using LibraryApp.Models;
using LibraryApp.Services;
using Xunit;

namespace LibraryApp.Tests;

public class HistoryServiceTests
{
    private readonly InMemoryHistoryRepository _repository = new();

    [Fact]
    public async Task AddLoanAsync_AddsEntryWithLoanAction()
    {
        var service = CreateService();
        var book = CreateBook("Babička", "Božena Němcová", "111");

        await service.AddLoanAsync(book);

        Assert.Single(_repository.Entries);
        var entry = _repository.Entries[0];
        Assert.Equal(book.Id, entry.BookId);
        Assert.Equal(book.Title, entry.Title);
        Assert.Equal(book.Author, entry.Author);
        Assert.Equal(book.Isbn, entry.Isbn);
        Assert.Equal(LoanAction.Loan, entry.Action);
    }

    [Fact]
    public async Task AddReturnAsync_AddsEntryWithReturnAction()
    {
        var service = CreateService();
        var book = CreateBook("Kytice", "Karel Jaromír Erben", "222");

        await service.AddReturnAsync(book);

        Assert.Single(_repository.Entries);
        Assert.Equal(LoanAction.Return, _repository.Entries[0].Action);
    }

    [Fact]
    public async Task GetAsync_FiltersByTitleIsbnActionAndDateRange()
    {
        var service = CreateService();
        var now = DateTime.UtcNow;

        var loanBook = CreateBook("Povídky z jedné kapsy", "Karel Čapek", "978-80-242-2420-3");
        await service.AddLoanAsync(loanBook);
        _repository.Entries[0].OccurredAtUtc = now.AddDays(-2);

        var returnBook = CreateBook("Babička", "Božena Němcová", "978-80-206-0747-4");
        await service.AddReturnAsync(returnBook);
        _repository.Entries[1].OccurredAtUtc = now;

        var search = new HistorySearchModel
        {
            Title = "povídky",
            Isbn = "978-80-242",
            Action = LoanAction.Loan,
            FromUtc = now.AddDays(-3),
            ToUtc = now.AddDays(-1)
        };

        var results = await service.GetAsync(search);

        Assert.Single(results);
        Assert.Equal(loanBook.Title, results[0].Title);
        Assert.Equal(LoanAction.Loan, results[0].Action);
    }

    [Fact]
    public async Task GetAsync_OrdersByDateDescThenTitle()
    {
        var service = CreateService();
        var now = DateTime.UtcNow;

        var first = CreateBook("Zeta", "A", "111");
        await service.AddLoanAsync(first);
        _repository.Entries[0].OccurredAtUtc = now;

        var second = CreateBook("Alpha", "B", "222");
        await service.AddLoanAsync(second);
        _repository.Entries[1].OccurredAtUtc = now;

        var third = CreateBook("Middle", "C", "333");
        await service.AddLoanAsync(third);
        _repository.Entries[2].OccurredAtUtc = now.AddHours(-1);

        var results = await service.GetAsync();

        Assert.Equal(3, results.Count);
        // Newest timestamps first, and within the same timestamp sorted by Title ascending.
        Assert.Equal("Alpha", results[0].Title);
        Assert.Equal("Zeta", results[1].Title);
        Assert.Equal("Middle", results[2].Title);
    }

    private HistoryService CreateService() => new(_repository);

    private static Book CreateBook(string title, string author, string isbn) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            Isbn = isbn,
            TotalCopies = 1,
            AvailableCopies = 1
        };

    private class InMemoryHistoryRepository : IHistoryRepository
    {
        public List<LoanHistoryEntry> Entries { get; } = new();

        public Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<LoanHistoryEntry>>(Entries.ToList());
    }
}


