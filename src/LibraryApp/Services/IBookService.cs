using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IBookService
{
    Task<IReadOnlyList<Book>> GetAllAsync(BookSearchModel? search = null, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, Book? Created)> AddBookAsync(BookInputModel input, CancellationToken cancellationToken = default);
    Task<bool> LoanBookAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ReturnBookAsync(Guid id, CancellationToken cancellationToken = default);
}

