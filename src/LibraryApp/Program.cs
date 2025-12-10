using LibraryApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. 
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonRepositoryOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<HistoryRepositoryOptions>(builder.Configuration.GetSection("HistoryStorage"));
builder.Services.AddSingleton<IBookRepository, JsonBookRepository>();
builder.Services.AddSingleton<IHistoryRepository, JsonHistoryRepository>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IBookService, BookService>();

var app = builder.Build();

// Seed demo data so the UI/API is usable right after start.
await SeedData.EnsureSeedDataAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(options =>
{
    var culture = new System.Globalization.CultureInfo("cs-CZ");
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
    options.SupportedCultures = new[] { culture };
    options.SupportedUICultures = new[] { culture };
});
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Index}/{id?}");

// Attribute-routed APIs.
app.MapControllers();

app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }
