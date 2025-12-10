using System.Text.Json;
using LibraryApp.Models;
using Microsoft.Extensions.Options;

namespace LibraryApp.Services;

public class JsonHistoryRepository : IHistoryRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonHistoryRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonHistoryRepository(IOptions<HistoryRepositoryOptions> options, ILogger<JsonHistoryRepository> logger)
    {
        _logger = logger;
        var configuredPath = options.Value.FilePath;
        var path = string.IsNullOrWhiteSpace(configuredPath) ? "data/history.json" : configuredPath;
        _filePath = Path.GetFullPath(path);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        EnsureFileExists();
    }

    public async Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var entries = await LoadInternalAsync(cancellationToken);
            entries.Add(entry);
            await SaveInternalAsync(entries, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var entries = await LoadInternalAsync(cancellationToken);
            return entries.ToList();
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

    private async Task<List<LoanHistoryEntry>> LoadInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<LoanHistoryEntry>();
            }

            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<LoanHistoryEntry>();
            }

            return JsonSerializer.Deserialize<List<LoanHistoryEntry>>(json, _jsonOptions) ?? new List<LoanHistoryEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read history from {File}", _filePath);
            return new List<LoanHistoryEntry>();
        }
    }

    private async Task SaveInternalAsync(List<LoanHistoryEntry> entries, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(entries, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save history to {File}", _filePath);
            throw;
        }
    }
}

