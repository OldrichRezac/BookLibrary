# LibraryApp
# Správa knih (.NET 8 MVC + API)   

Jednoduchá ukázková aplikace pro správu knih s webovým UI (MVC) a REST API.

## Požadavky
- .NET 8 SDK

## Jak spustit
```bash
dotnet restore
dotnet run --project src/LibraryApp/LibraryApp.csproj --launch-profile http
```

Výchozí URL: `http://localhost:5174` (Swagger na `/swagger`).

Data se perzistují do `data/library.json` (vytvoří se automaticky). Při prvním spuštění se založí několik ukázkových knih.

## Funkce UI
- Výpis všech knih + filtrování podle názvu, autora nebo ISBN.
- Přidání nové knihy.
- Půjčení a vrácení knihy přímo v seznamu (počítá dostupné kusy).

## REST API (hlavní body)
- `GET /api/books?title=&author=&isbn=` – výpis a vyhledávání.
- `GET /api/books/{id}` – detail knihy.
- `POST /api/books` – vytvoření knihy (`BookInputModel` v těle).
- `POST /api/books/{id}/loan` – půjčení (sníží počet dostupných kusů).
- `POST /api/books/{id}/return` – vrácení (zvýší počet dostupných kusů).

Swagger UI: `/swagger`.

## Testy
```bash
dotnet test
```

## Poznámky k implementaci
- Úložiště `JsonBookRepository` pracuje se souborem `data/library.json` (zámek pro bezpečné zápisy).
- `BookService` řeší business logiku (vyhledávání, kontrola ISBN, půjčování/vracení).
- UI i API sdílejí stejné modely a validace (DataAnnotations).

## AI disclosure
Na generování kódu a textu jsem použil ChatGPT (OpenAI) jako pomocníka pro návrh struktury, psaní kódu a dokumentace. Vše bylo zkontrolováno a upraveno ručně.

 
# BookLibrary
