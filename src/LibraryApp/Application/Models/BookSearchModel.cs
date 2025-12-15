namespace LibraryApp.Models;

public class BookSearchModel
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Author) ||
        !string.IsNullOrWhiteSpace(Isbn);
}


