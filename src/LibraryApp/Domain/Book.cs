using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [Range(0, 2100, ErrorMessage = "Rok musí být mezi 0 a 2100.")]
    public int PublishedYear { get; set; }

    [Required, StringLength(32)]
    public string Isbn { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Celkový počet kusů nesmí být záporný.")]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Počet dostupných kusů nesmí být záporný.")]
    public int AvailableCopies { get; set; }
}


