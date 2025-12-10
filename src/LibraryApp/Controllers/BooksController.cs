using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] BookSearchModel filters)
    {
        var books = await _bookService.GetAllAsync(filters);
        var pageSize = filters.PageSize <= 0 ? 10 : filters.PageSize;
        var page = filters.Page <= 0 ? 1 : filters.Page;
        var total = books.Count;
        var paged = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var viewModel = new BookListViewModel
        {
            Books = paged,
            Filters = filters,
            Message = TempData["Message"] as string,
            Error = TempData["Error"] as string,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new BookInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _bookService.AddBookAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nepodařilo se přidat knihu.");
            return View(model);
        }

        TempData["Message"] = "Kniha byla úspěšně přidána.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Loan(Guid id)
    {
        var ok = await _bookService.LoanBookAsync(id);
        TempData[ok ? "Message" : "Error"] = ok
            ? "Kniha byla půjčena."
            : "Knihu se nepodařilo půjčit (možná není k dispozici).";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(Guid id)
    {
        var ok = await _bookService.ReturnBookAsync(id);
        TempData[ok ? "Message" : "Error"] = ok
            ? "Kniha byla vrácena."
            : "Knihu se nepodařilo vrátit.";

        return RedirectToAction(nameof(Index));
    }
}

