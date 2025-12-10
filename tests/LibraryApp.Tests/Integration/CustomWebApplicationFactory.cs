using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LibraryApp.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryBookRepository BookRepository { get; } = new();
    public InMemoryHistoryRepository HistoryRepository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBookRepository>();
            services.RemoveAll<IHistoryRepository>();

            services.AddSingleton<IBookRepository>(BookRepository);
            services.AddSingleton<IHistoryRepository>(HistoryRepository);
        });
    }

    public class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = new();
        private readonly object _sync = new();

        public Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                _books.Add(Clone(book));
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult<IReadOnlyList<Book>>(_books.Select(Clone).ToList());
            }
        }

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult<Book?>(_books.FirstOrDefault(b => b.Id == id)?.Let(Clone));
            }
        }

        public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult<Book?>(_books.FirstOrDefault(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase))?.Let(Clone));
            }
        }

        public Task<bool> UpdateCopiesAsync(Guid id, int delta, CancellationToken cancellationToken = default)
        {
            lock (_sync)
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

        public void Clear()
        {
            lock (_sync)
            {
                _books.Clear();
            }
        }

        public void Seed(params Book[] books)
        {
            lock (_sync)
            {
                _books.Clear();
                _books.AddRange(books.Select(Clone));
            }
        }

        private static Book Clone(Book source) => new()
        {
            Id = source.Id,
            Title = source.Title,
            Author = source.Author,
            Isbn = source.Isbn,
            PublishedYear = source.PublishedYear,
            TotalCopies = source.TotalCopies,
            AvailableCopies = source.AvailableCopies
        };
    }

    public class InMemoryHistoryRepository : IHistoryRepository
    {
        private readonly List<LoanHistoryEntry> _entries = new();
        private readonly object _sync = new();

        public List<LoanHistoryEntry> Entries
        {
            get
            {
                lock (_sync)
                {
                    return _entries.Select(Clone).ToList();
                }
            }
        }

        public Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                _entries.Add(Clone(entry));
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult<IReadOnlyList<LoanHistoryEntry>>(_entries.Select(Clone).ToList());
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _entries.Clear();
            }
        }

        private static LoanHistoryEntry Clone(LoanHistoryEntry source) => new()
        {
            Id = source.Id,
            BookId = source.BookId,
            Title = source.Title,
            Author = source.Author,
            Isbn = source.Isbn,
            Action = source.Action,
            OccurredAtUtc = source.OccurredAtUtc
        };
    }
}

internal static class FunctionalExtensions
{
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> map) => map(self);
}


