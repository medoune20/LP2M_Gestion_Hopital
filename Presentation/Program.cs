using GestionHopital.Infrastructure.Donnees;
using GestionHopital.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Environment.GetEnvironmentVariable("DATA_DIR")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionHopital");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "hopital.db");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IEmailService, SmtpEmailService>();

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

// Hébergement LP2M : permet à l'application de fonctionner sous
// https://lp2medoune.com/hopital tout en gardant /health disponible en local.
var pathBase = Environment.GetEnvironmentVariable("PATH_BASE")?.Trim();
if (!string.IsNullOrWhiteSpace(pathBase))
{
    if (!pathBase.StartsWith('/')) pathBase = "/" + pathBase;
    app.UsePathBase(pathBase);
}

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
