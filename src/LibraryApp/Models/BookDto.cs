namespace LibraryApp.Models;

public record BookDto(
    Guid Id,
    string Title,
    string Author,
    int PublishedYear,
    string Isbn,
    int AvailableCopies,
    int TotalCopies);


