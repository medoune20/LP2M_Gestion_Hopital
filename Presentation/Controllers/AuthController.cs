using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) { _db = db; }

    [HttpGet]
    public IActionResult Connexion(string? retour)
    {
        if (HttpContext.Session.GetInt32("UtilisateurId") > 0)
            return Redirect(AppUrl("/Accueil/Index"));

        ViewBag.Retour = retour;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Connexion(string login, string motDePasse, string? retour)
    {
        login = (login ?? string.Empty).Trim();
        motDePasse ??= string.Empty;

        var hash = HashMdp(motDePasse);
        var user = await _db.Utilisateurs.FirstOrDefaultAsync(u => u.Login == login && u.MotDePasse == hash && u.Actif);
        if (user == null)
        {
            ViewBag.Erreur = "Identifiants incorrects.";
            ViewBag.Retour = retour;
            return View();
        }

        user.DernièreConnexion = DateTime.Now;
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UtilisateurId", user.Id);
        HttpContext.Session.SetString("UtilisateurNom", user.NomComplet);
        HttpContext.Session.SetString("UtilisateurLogin", user.Login);
        HttpContext.Session.SetInt32("Role", (int)user.Role);
        if (user.MedecinId.HasValue)
            HttpContext.Session.SetInt32("MedecinId", user.MedecinId.Value);

        var etab = await _db.Etablissements.FirstOrDefaultAsync();
        if (etab != null)
            HttpContext.Session.SetString("CouleurHopital", etab.Couleur ?? "#0ea5e9");

        if (!string.IsNullOrWhiteSpace(retour) && Url.IsLocalUrl(retour) && !retour.Contains("Connexion", StringComparison.OrdinalIgnoreCase))
        {
            var chemin = retour.StartsWith('/') ? retour : "/" + retour;
            if (!chemin.StartsWith(AppBase() + "/", StringComparison.OrdinalIgnoreCase))
                chemin = AppBase() + chemin;
            return Redirect(chemin);
        }

        return Redirect(AppUrl("/Accueil/Index"));
    }

    [HttpGet, HttpPost]
    public IActionResult Deconnexion()
    {
        HttpContext.Session.Clear();
        return Redirect(AppUrl("/Auth/Connexion"));
    }

    private string AppUrl(string path)
    {
        if (!path.StartsWith('/')) path = "/" + path;
        return AppBase() + path;
    }

    private string AppBase()
    {
        var basePath = Request.PathBase.Value;
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = Environment.GetEnvironmentVariable("PATH_BASE") ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(basePath) && !basePath.StartsWith('/'))
            basePath = "/" + basePath;
        return basePath.TrimEnd('/');
    }

    private static string HashMdp(string mdp)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(mdp));
        return Convert.ToHexString(bytes).ToLower();
    }
}
