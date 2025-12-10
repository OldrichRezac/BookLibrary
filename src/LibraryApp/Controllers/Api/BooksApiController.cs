using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class BooksApiController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksApiController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks([FromQuery] BookSearchModel filters)
    {
        var books = await _bookService.GetAllAsync(filters);
        return Ok(books.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> GetBook(Guid id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book is null)
        {
            return NotFound();
        }

        return Ok(ToDto(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(BookInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (success, error, created) = await _bookService.AddBookAsync(model);
        if (!success || created is null)
        {
            return Conflict(new { message = error ?? "Nepodařilo se vytvořit knihu." });
        }

        return CreatedAtAction(nameof(GetBook), new { id = created.Id }, ToDto(created));
    }

    [HttpPost("{id:guid}/loan")]
    public async Task<IActionResult> Loan(Guid id)
    {
        var ok = await _bookService.LoanBookAsync(id);
        if (!ok)
        {
            return BadRequest(new { message = "Knihu se nepodařilo půjčit (možná není k dispozici)." });
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var ok = await _bookService.ReturnBookAsync(id);
        if (!ok)
        {
            return BadRequest(new { message = "Knihu se nepodařilo vrátit (už jsou vráceny všechny kusy nebo kniha neexistuje)." });
        }

        return NoContent();
    }

    private static BookDto ToDto(Book book) =>
        new(book.Id, book.Title, book.Author, book.PublishedYear, book.Isbn, book.AvailableCopies, book.TotalCopies);
}


