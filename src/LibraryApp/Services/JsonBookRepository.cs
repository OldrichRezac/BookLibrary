using System.Text.Json;
using LibraryApp.Models;
using Microsoft.Extensions.Options;

namespace LibraryApp.Services;

public class JsonBookRepository : IBookRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonBookRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonBookRepository(IOptions<JsonRepositoryOptions> options, ILogger<JsonBookRepository> logger)
    {
        _logger = logger;
        var configuredPath = options.Value.FilePath;
        _filePath = Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath) ? "data/library.json" : configuredPath);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        EnsureFileExists();
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var books = await LoadInternalAsync(cancellationToken);
            books.Add(book);
            await SaveInternalAsync(books, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var books = await LoadInternalAsync(cancellationToken);
            return books.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var books = await LoadInternalAsync(cancellationToken);
            return books.FirstOrDefault(b => b.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var books = await LoadInternalAsync(cancellationToken);
            return books.FirstOrDefault(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateCopiesAsync(Guid id, int delta, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var books = await LoadInternalAsync(cancellationToken);
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book is null)
            {
                return false;
            }

            if (delta < 0 && book.AvailableCopies + delta < 0)
            {
                return false;
            }

            book.AvailableCopies += delta;
            await SaveInternalAsync(books, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<Book>> LoadInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<Book>();
            }

            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Book>();
            }

            return JsonSerializer.Deserialize<List<Book>>(json, _jsonOptions) ?? new List<Book>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read books from {File}", _filePath);
            return new List<Book>();
        }
    }

    private async Task SaveInternalAsync(List<Book> books, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(books, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save books to {File}", _filePath);
            throw;
        }
    }
}

