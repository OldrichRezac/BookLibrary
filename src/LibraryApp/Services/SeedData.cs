using LibraryApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.Services;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
        var existing = await repository.GetAllAsync();
        if (existing.Count > 0)
        {
            return;
        }

        var samples = new[]
        {
            new Book { Title = "Babička", Author = "Božena Němcová", PublishedYear = 1855, Isbn = "978-80-206-0747-4", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Title = "Saturnin", Author = "Zdeněk Jirotka", PublishedYear = 1942, Isbn = "978-80-206-1129-7", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Spalovač mrtvol", Author = "Ladislav Fuks", PublishedYear = 1967, Isbn = "978-80-257-2124-0", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "Kytice", Author = "Karel Jaromír Erben", PublishedYear = 1853, Isbn = "978-80-242-2881-2", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Title = "Osudy dobrého vojáka Švejka", Author = "Jaroslav Hašek", PublishedYear = 1923, Isbn = "978-80-242-3530-8", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Title = "Bylo nás pět", Author = "Karel Poláček", PublishedYear = 1946, Isbn = "978-80-00-05545-7", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Povídky z jedné kapsy", Author = "Karel Čapek", PublishedYear = 1929, Isbn = "978-80-242-2420-3", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Válka s Mloky", Author = "Karel Čapek", PublishedYear = 1936, Isbn = "978-80-257-2501-9", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Smrt krásných srnců", Author = "Ota Pavel", PublishedYear = 1971, Isbn = "978-80-204-3717-5", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Dům o tisíci patrech", Author = "Jan Weiss", PublishedYear = 1929, Isbn = "978-80-7388-750-9", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "Život je jinde", Author = "Milan Kundera", PublishedYear = 1973, Isbn = "978-80-207-1832-8", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Title = "Petrolejové lampy", Author = "Jaroslav Havlíček", PublishedYear = 1935, Isbn = "978-80-7432-997-8", TotalCopies = 1, AvailableCopies = 1 },
            new Book { Title = "Krakatit", Author = "Karel Čapek", PublishedYear = 1924, Isbn = "978-80-257-3672-5", TotalCopies = 2, AvailableCopies = 2 }
        };

        foreach (var book in samples)
        {
            await repository.AddAsync(book);
        }
    }
}


