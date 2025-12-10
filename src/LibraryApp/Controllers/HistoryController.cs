using System.Globalization;
using LibraryApp.Models;
using LibraryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Controllers;

public class HistoryController : Controller
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] HistorySearchModel filters)
    {
        var fromRaw = Request.Query["FromUtc"].ToString();
        var toRaw = Request.Query["ToUtc"].ToString();
        filters.FromUtc = ParseDate(fromRaw);
        filters.ToUtc = ParseDate(toRaw);

        var entries = await _historyService.GetAsync(filters);
        var pageSize = filters.PageSize <= 0 ? 10 : filters.PageSize;
        var page = filters.Page <= 0 ? 1 : filters.Page;
        var total = entries.Count;
        var paged = entries.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var viewModel = new HistoryListViewModel
        {
            Entries = paged,
            Filters = filters,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return View(viewModel);
    }

    private static DateTime? ParseDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // ISO date (from input type="date")
        if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtIsoDate))
        {
            return DateTime.SpecifyKind(dtIsoDate, DateTimeKind.Local).ToUniversalTime();
        }

        // Czech date-only format
        if (DateTime.TryParseExact(input, "dd.MM.yyyy", new CultureInfo("cs-CZ"), DateTimeStyles.AssumeLocal, out var dtCzDate))
        {
            return DateTime.SpecifyKind(dtCzDate, DateTimeKind.Local).ToUniversalTime();
        }

        // ISO datetime-local fallback
        if (DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtIso))
        {
            return DateTime.SpecifyKind(dtIso, DateTimeKind.Local).ToUniversalTime();
        }

        // General fallback with Czech culture
        if (DateTime.TryParse(input, new CultureInfo("cs-CZ"), DateTimeStyles.AssumeLocal, out var dt))
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
        }

        return null;
    }
}

