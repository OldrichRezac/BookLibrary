namespace LibraryApp.Models;

public class HistorySearchModel
{
    public string? Title { get; set; }
    public string? Isbn { get; set; }
    public LoanAction? Action { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Isbn) ||
        Action.HasValue ||
        FromUtc.HasValue ||
        ToUtc.HasValue;
}

