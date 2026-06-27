using GestionHopital.Infrastructure.Donnees;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var dataDir = Environment.GetEnvironmentVariable("DATA_DIR")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionHopital");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "hopital.db");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// MVC + Session
builder.Services.AddControllersWithViews();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(8);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.SameSite = SameSiteMode.Lax;
    opt.Cookie.Name = ".GestionHopital.Session";
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Permet d'heberger l'application sous https://lp2medoune.com/hopital
// tout en conservant le healthcheck interne http://localhost:8080/health.
var pathBase = Environment.GetEnvironmentVariable("PATH_BASE")?.Trim();
if (!string.IsNullOrWhiteSpace(pathBase))
{
    if (!pathBase.StartsWith('/')) pathBase = "/" + pathBase;
    app.UsePathBase(pathBase);
}

// Init DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseInitializer.InitialiserAsync(db);
    await Lp2mSanteSchema.AppliquerAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Accueil/Erreur");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Accueil}/{action=Index}/{id?}");

app.MapGet("/health", () => Results.Ok(new { statut = "ok", heure = DateTime.UtcNow }));

app.Run();
