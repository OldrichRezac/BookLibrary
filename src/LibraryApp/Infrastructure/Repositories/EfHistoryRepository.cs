using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public class EfHistoryRepository : IHistoryRepository
{
    private readonly LibraryDbContext _db;

    public EfHistoryRepository(LibraryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(LoanHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        await _db.LoanHistoryEntries.AddAsync(entry, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.LoanHistoryEntries.AsNoTracking().ToListAsync(cancellationToken);
        return items;
    }
}


