using System.Globalization;
using LibraryApp.Models;
using LibraryApp.Tests.Integration;
using Microsoft.Playwright;
using Xunit;
using Xunit.Sdk;

namespace LibraryApp.Tests.E2E;

[Trait("Category", "E2E")]
public class BooksPlaywrightTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BooksPlaywrightTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

[Fact(DisplayName = "E2E: vytvoření knihy, půjčení, vrácení a záznam v historii")]
    public async Task BookLifecycle()
    {
    var baseUrl = Environment.GetEnvironmentVariable("E2E_BASEURL");
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        // E2E se nespustí, pokud není zadaná běžící instance
        return;
    }

    // Start host and reset in-memory data (for integration mode; harmless if hitting external app)
    using var _ = _factory.CreateClient();
    _factory.BookRepository.Clear();
    _factory.HistoryRepository.Clear();

        using var playwright = await CreatePlaywrightOrSkipAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    var page = await browser.NewPageAsync(new BrowserNewPageOptions { BaseURL = baseUrl });

        var title = $"E2E-{Guid.NewGuid():N}".Substring(0, 8);
        var author = "Playwright Tester";
        var isbn = $"978-{new Random().Next(1000000, 9999999)}";
        const int year = 2024;

        // Create book via UI
        await page.GotoAsync("/Books");
        await page.GetByRole(AriaRole.Link, new() { Name = "Přidat knihu" }).ClickAsync();
        await page.FillAsync("input[name='Title']", title);
        await page.FillAsync("input[name='Author']", author);
        await page.FillAsync("input[name='PublishedYear']", year.ToString(CultureInfo.InvariantCulture));
        await page.FillAsync("input[name='Isbn']", isbn);
        await page.FillAsync("input[name='TotalCopies']", "1");
        await page.FillAsync("input[name='AvailableCopies']", "1");
        await page.GetByRole(AriaRole.Button, new() { Name = "Uložit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GotoAsync("/Books");

        var row = page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = title });
        Assert.Equal(1, await row.CountAsync());
        Assert.Contains("1 / 1", await row.Nth(0).TextContentAsync());

        // Loan
        await row.Nth(0).GetByRole(AriaRole.Button, new() { Name = "Půjčit" }).ClickAsync();
        await page.WaitForTimeoutAsync(200); // allow refresh
        Assert.Contains("0 / 1", await row.Nth(0).TextContentAsync());

        // Return
        await row.Nth(0).GetByRole(AriaRole.Button, new() { Name = "Vrátit" }).ClickAsync();
        await page.WaitForTimeoutAsync(200);
        Assert.Contains("1 / 1", await row.Nth(0).TextContentAsync());

        // History contains record
        await page.GotoAsync("/History");
        await page.FillAsync("input[name='Isbn']", isbn);
        await page.GetByRole(AriaRole.Button, new() { Name = "Hledat" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var historyRow = page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = isbn });
        Assert.True(await historyRow.CountAsync() >= 1);
    }

    private static async Task<IPlaywright> CreatePlaywrightOrSkipAsync()
    {
        try
        {
            return await Microsoft.Playwright.Playwright.CreateAsync();
        }
        catch (PlaywrightException)
        {
            throw new SkipException("Playwright není nainstalován. Spusť: dotnet tool install --global Microsoft.Playwright.CLI && playwright install");
        }
    }
}


