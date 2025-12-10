using System.Text.Json;
using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LibraryApp.Tests;

public class JsonBookRepositoryTests
{
    [Fact]
    public async Task AddAsync_AppendsAndPersists()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");
        var repo = CreateRepo(file);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Babička",
            Author = "Božena Němcová",
            Isbn = "978-80-206-0747-4",
            TotalCopies = 2,
            AvailableCopies = 2
        };

        await repo.AddAsync(book);

        var all = await repo.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(book.Title, all[0].Title);

        var json = await File.ReadAllTextAsync(file);
        var parsed = JsonSerializer.Deserialize<List<Book>>(json);
        Assert.NotNull(parsed);
        Assert.Single(parsed!);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyOnMissingFileOrInvalidJson()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");

        var repo = CreateRepo(file);
        var initial = await repo.GetAllAsync();
        Assert.Empty(initial);

        await File.WriteAllTextAsync(file, "not json");
        var repo2 = CreateRepo(file);
        var after = await repo2.GetAllAsync();
        Assert.Empty(after);
    }

    [Fact]
    public async Task GetByIdAsync_FindsBook()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Kytice",
            Author = "Karel Jaromír Erben",
            Isbn = "978-80-242-2881-2",
            TotalCopies = 3,
            AvailableCopies = 3
        };
        var json = JsonSerializer.Serialize(new List<Book> { book }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(file, json);

        var repo = CreateRepo(file);
        var found = await repo.GetByIdAsync(book.Id);

        Assert.NotNull(found);
        Assert.Equal(book.Title, found!.Title);
    }

    [Fact]
    public async Task GetByIsbnAsync_MatchesCaseInsensitive()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");
        var book = new Book { Id = Guid.NewGuid(), Title = "Test", Author = "A", Isbn = "ABC", TotalCopies = 1, AvailableCopies = 1 };
        var json = JsonSerializer.Serialize(new List<Book> { book }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(file, json);

        var repo = CreateRepo(file);
        var found = await repo.GetByIsbnAsync("abc");

        Assert.NotNull(found);
        Assert.Equal(book.Id, found!.Id);
    }

    [Fact]
    public async Task UpdateCopiesAsync_UpdatesAndPersists()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");
        var book = new Book { Id = Guid.NewGuid(), Title = "A", Author = "B", Isbn = "X", TotalCopies = 2, AvailableCopies = 2 };
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(new List<Book> { book }));

        var repo = CreateRepo(file);
        var ok = await repo.UpdateCopiesAsync(book.Id, -1);

        Assert.True(ok);
        var all = await repo.GetAllAsync();
        Assert.Equal(1, all[0].AvailableCopies);

        var json = await File.ReadAllTextAsync(file);
        var parsed = JsonSerializer.Deserialize<List<Book>>(json);
        Assert.Equal(1, parsed![0].AvailableCopies);
    }

    [Fact]
    public async Task UpdateCopiesAsync_ReturnsFalseWhenNotFoundOrNegativeResult()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "library.json");
        var book = new Book { Id = Guid.NewGuid(), Title = "A", Author = "B", Isbn = "X", TotalCopies = 1, AvailableCopies = 0 };
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(new List<Book> { book }));

        var repo = CreateRepo(file);

        var missing = await repo.UpdateCopiesAsync(Guid.NewGuid(), -1);
        Assert.False(missing);

        var negative = await repo.UpdateCopiesAsync(book.Id, -1);
        Assert.False(negative);
    }

    private static JsonBookRepository CreateRepo(string filePath)
    {
        var opts = Options.Create(new JsonRepositoryOptions { FilePath = filePath });
        return new JsonBookRepository(opts, NullLogger<JsonBookRepository>.Instance);
    }

    private sealed class TempDir : IDisposable
    {
        public string Dir { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public TempDir()
        {
            Directory.CreateDirectory(Dir);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Dir, true);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }
}


