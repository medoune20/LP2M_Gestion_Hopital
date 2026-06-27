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
            return RedirectToAction("Index", "Accueil");

        ViewBag.Retour = retour;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        // Important en hébergement sous /hopital : ne jamais rediriger vers une URL absolue
        // de type /Accueil, sinon le navigateur sort du préfixe LP2M et iOS Safari peut
        // interpréter la réponse comme un fichier à télécharger.
        if (!string.IsNullOrWhiteSpace(retour) && Url.IsLocalUrl(retour) && !retour.Contains("Connexion", StringComparison.OrdinalIgnoreCase))
        {
            var chemin = retour.StartsWith('/') ? retour[1..] : retour;
            return LocalRedirect(Url.Content("~/" + chemin));
        }

        return RedirectToAction("Index", "Accueil");
    }

    [HttpGet]
    public IActionResult Déconnexion() => Deconnexion();

    [HttpGet]
    public IActionResult Deconnexion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Connexion");
    }

    private static string HashMdp(string mdp)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(mdp));
        return Convert.ToHexString(bytes).ToLower();
    }
}
