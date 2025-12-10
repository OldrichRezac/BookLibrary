using System.Net;
using System.Net.Http.Json;
using LibraryApp.Models;
using LibraryApp.Services;
using Xunit;

namespace LibraryApp.Tests.Integration;

public class BooksApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BooksApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.BookRepository.Clear();
        _factory.HistoryRepository.Clear();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_ReturnsSeededBooks()
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            Author = "Tester",
            Isbn = "123",
            PublishedYear = 2024,
            TotalCopies = 2,
            AvailableCopies = 2
        };
        _factory.BookRepository.Seed(book);

        var response = await _client.GetAsync("/api/BooksApi");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal(book.Title, payload![0].Title);
    }

    [Fact]
    public async Task Create_Then_Loan_Then_Return_Works_And_WritesHistory()
    {
        var input = new BookInputModel
        {
            Title = "Workflow",
            Author = "Tester",
            Isbn = "WF-1",
            PublishedYear = 2020,
            TotalCopies = 1,
            AvailableCopies = 1
        };

        var create = await _client.PostAsJsonAsync("/api/BooksApi", input);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(created);

        var loan = await _client.PostAsync($"/api/BooksApi/{created!.Id}/loan", null);
        Assert.Equal(HttpStatusCode.NoContent, loan.StatusCode);

        var returned = await _client.PostAsync($"/api/BooksApi/{created.Id}/return", null);
        Assert.Equal(HttpStatusCode.NoContent, returned.StatusCode);

        // History should have both actions
        Assert.Equal(2, _factory.HistoryRepository.Entries.Count);
        Assert.Contains(_factory.HistoryRepository.Entries, e => e.Action == LoanAction.Loan);
        Assert.Contains(_factory.HistoryRepository.Entries, e => e.Action == LoanAction.Return);
    }

    [Fact]
    public async Task Create_BlocksDuplicateIsbn()
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Dup",
            Author = "A",
            Isbn = "DUP",
            PublishedYear = 2000,
            TotalCopies = 1,
            AvailableCopies = 1
        };
        _factory.BookRepository.Seed(book);

        var input = new BookInputModel
        {
            Title = "Dup2",
            Author = "B",
            Isbn = "DUP",
            PublishedYear = 2001,
            TotalCopies = 1,
            AvailableCopies = 1
        };

        var response = await _client.PostAsJsonAsync("/api/BooksApi", input);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}


