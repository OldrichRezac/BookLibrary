namespace LibraryApp.Models;

public class LoanHistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public LoanAction Action { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}




