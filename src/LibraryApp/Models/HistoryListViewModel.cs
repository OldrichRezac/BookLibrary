namespace LibraryApp.Models;

public class HistoryListViewModel
{
    public IReadOnlyCollection<LoanHistoryEntry> Entries { get; init; } = Array.Empty<LoanHistoryEntry>();
    public HistorySearchModel Filters { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page * PageSize < TotalCount;
}

