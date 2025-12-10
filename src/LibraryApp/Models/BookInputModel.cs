using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class BookInputModel
{
    [Required, StringLength(200)]
    [Display(Name = "Název")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [Display(Name = "Autor")]
    public string Author { get; set; } = string.Empty;

    [Range(0, 2100)]
    [Display(Name = "Rok vydání")]
    public int PublishedYear { get; set; }

    [Required, StringLength(32)]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Celkový počet kusů")]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Počet dostupných kusů")]
    public int AvailableCopies { get; set; }
}


