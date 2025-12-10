using System.Text.Json;
using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LibraryApp.Tests;

public class JsonHistoryRepositoryTests
{
    [Fact]
    public async Task AddAsync_AppendsEntryAndPersists()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "history.json");
        var repo = CreateRepo(file);

        var entry = new LoanHistoryEntry
        {
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Title = "Babička",
            Author = "Božena Němcová",
            Isbn = "978-80-206-0747-4",
            Action = LoanAction.Loan,
            OccurredAtUtc = DateTime.UtcNow
        };

        await repo.AddAsync(entry);

        var all = await repo.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(entry.Title, all[0].Title);

        // Re-load from disk to ensure persistence
        var json = await File.ReadAllTextAsync(file);
        var parsed = JsonSerializer.Deserialize<List<LoanHistoryEntry>>(json);
        Assert.NotNull(parsed);
        Assert.Single(parsed!);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyOnMissingFileOrInvalidJson()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "history.json");

        // Missing file
        var repo = CreateRepo(file);
        var initial = await repo.GetAllAsync();
        Assert.Empty(initial);

        // Invalid JSON should be swallowed and logged, returning empty list
        await File.WriteAllTextAsync(file, "not json");
        var repo2 = CreateRepo(file);
        var after = await repo2.GetAllAsync();
        Assert.Empty(after);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDataFromFile()
    {
        using var temp = new TempDir();
        var file = Path.Combine(temp.Dir, "history.json");
        var entry = new LoanHistoryEntry
        {
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Title = "Kytice",
            Author = "Karel Jaromír Erben",
            Isbn = "978-80-242-2881-2",
            Action = LoanAction.Return,
            OccurredAtUtc = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(new List<LoanHistoryEntry> { entry }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(file, json);

        var repo = CreateRepo(file);
        var all = await repo.GetAllAsync();

        Assert.Single(all);
        Assert.Equal(entry.Title, all[0].Title);
        Assert.Equal(entry.Action, all[0].Action);
    }

    private static JsonHistoryRepository CreateRepo(string filePath)
    {
        var opts = Options.Create(new HistoryRepositoryOptions { FilePath = filePath });
        return new JsonHistoryRepository(opts, NullLogger<JsonHistoryRepository>.Instance);
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


