# LibraryApp
# Správa knih (.NET 8 MVC + API)   

Jednoduchá ukázková aplikace pro správu knih s webovým UI (MVC) a REST API.

**Live demo:** https://applibrarynet-g2cnbyb7dedzfbf6.polandcentral-01.azurewebsites.net

## Požadavky
- .NET 8 SDK
- Pro E2E testy: Playwright (instaluje se automaticky přes `dotnet tool install --global Microsoft.Playwright.CLI && playwright install`)

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

### Všechny testy
```bash
dotnet test
```

### Pouze unit testy
```bash
dotnet test --filter "FullyQualifiedName!~Integration&FullyQualifiedName!~E2E"
```

### Pouze integration testy
```bash
dotnet test --filter "FullyQualifiedName~LibraryApp.Tests.Integration"
```

### E2E testy (Playwright)
Pro E2E testy je potřeba spustit aplikaci na `http://localhost:5174` a nastavit proměnnou prostředí:
```bash
# V jednom terminálu spusť aplikaci
dotnet run --project src/LibraryApp/LibraryApp.csproj --urls http://localhost:5174

# V druhém terminálu spusť E2E testy
E2E_BASEURL=http://localhost:5174 dotnet test --filter "Category=E2E"
```

## CI/CD

Projekt používá GitHub Actions workflow (`.github/workflows/dotnet.yml`) s následujícím pipeline:

1. **unit** – build, unit testy, publish artefaktu
2. **integration** – integration testy (závisí na unit)
3. **e2e** – E2E testy s Playwright (závisí na integration)
4. **deploy** – deploy na Azure App Service (závisí na e2e, spustí se pouze při push tagu `v*`)

### Deploy na Azure

Deploy se automaticky spustí při push tagu (např. `v1.0.0`):
```bash
git tag v1.0.0
git push origin v1.0.0
```

Pro manuální redeploy existujícího artefaktu použij GitHub Actions → Run workflow → zadej název artefaktu.

**Požadované nastavení v GitHubu:**
- **Variable**: `AZURE_WEBAPP_NAME` = název Azure web app (např. `AppLibraryNet`)
- **Secret**: `AZURE_WEBAPP_PUBLISH_PROFILE` = obsah publish profilu z Azure Portal

**Azure App Settings:**
Pro produkční prostředí nastav v Azure Portal → Configuration → Application settings:
- `Storage:FilePath` = `/home/site/wwwroot/data/library.json`
- `HistoryStorage:FilePath` = `/home/site/wwwroot/data/history.json`

## Poznámky k implementaci
- Úložiště `JsonBookRepository` pracuje se souborem `data/library.json` (zámek pro bezpečné zápisy).
- `BookService` řeší business logiku (vyhledávání, kontrola ISBN, půjčování/vracení).
- UI i API sdílejí stejné modely a validace (DataAnnotations).
- Testy jsou rozděleny na unit, integration a E2E kategorie.

## AI disclosure
Na generování kódu a textu jsem použil ChatGPT (OpenAI) jako pomocníka pro návrh struktury, psaní kódu a dokumentace. Vše bylo zkontrolováno a upraveno ručně.

 
