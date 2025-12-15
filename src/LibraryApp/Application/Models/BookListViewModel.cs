namespace LibraryApp.Models;

public class BookListViewModel
{
    public IReadOnlyCollection<Book> Books { get; init; } = Array.Empty<Book>();
    public BookSearchModel Filters { get; init; } = new();
    public string? Message { get; init; }
    public string? Error { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page * PageSize < TotalCount;
}


