using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public class EfBookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;

    public EfBookRepository(LibraryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _db.Books.AddAsync(book, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.Books.AsNoTracking().ToListAsync(cancellationToken);
        return items;
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var normalized = isbn.ToLower();
        return _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Isbn.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> UpdateCopiesAsync(Guid id, int delta, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        if (delta < 0 && book.AvailableCopies + delta < 0)
        {
            return false;
        }

        book.AvailableCopies += delta;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}


