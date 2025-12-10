using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class BookInputModel
{
    [Required(ErrorMessage = "Položka Název je povinná.")]
    [StringLength(200, ErrorMessage = "Název může mít maximálně 200 znaků.")]
    [Display(Name = "Název")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Položka Autor je povinná.")]
    [StringLength(200, ErrorMessage = "Autor může mít maximálně 200 znaků.")]
    [Display(Name = "Autor")]
    public string Author { get; set; } = string.Empty;

    [Range(0, 2100, ErrorMessage = "Rok vydání musí být v rozmezí 0 až 2100.")]
    [Display(Name = "Rok vydání")]
    public int PublishedYear { get; set; }

    [Required(ErrorMessage = "Položka ISBN je povinná.")]
    [StringLength(32, ErrorMessage = "ISBN může mít maximálně 32 znaků.")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Celkový počet kusů musí být nezáporné číslo.")]
    [Display(Name = "Celkový počet kusů")]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Počet dostupných kusů musí být nezáporné číslo.")]
    [Display(Name = "Počet dostupných kusů")]
    public int AvailableCopies { get; set; }
}


