using GestionHopital.Domaine;
using GestionHopital.Domaine.Models;
using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class UtilisateurController : BaseController
{
    public UtilisateurController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        if (!EstAdmin) return AccesRefuse();

        var utilisateurs = await _db.Utilisateurs
            .Include(u => u.Medecin)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prénom)
            .ToListAsync();

        return View(utilisateurs);
    }

    [HttpGet]
    public async Task<IActionResult> Nouveau()
    {
        if (!EstAdmin) return AccesRefuse();
        ViewBag.Medecins = await _db.Medecins.Where(m => m.Actif).OrderBy(m => m.Nom).ToListAsync();
        return View(new Utilisateur { Actif = true, Role = RoleUtilisateur.Réceptionniste });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nouveau(Utilisateur utilisateur, string motDePasseClair)
    {
        if (!EstAdmin) return AccesRefuse();

        utilisateur.Login = (utilisateur.Login ?? string.Empty).Trim();
        utilisateur.Nom = (utilisateur.Nom ?? string.Empty).Trim();
        utilisateur.Prénom = (utilisateur.Prénom ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(utilisateur.Login) || string.IsNullOrWhiteSpace(utilisateur.Nom) || string.IsNullOrWhiteSpace(utilisateur.Prénom))
            ModelState.AddModelError(string.Empty, "Login, nom et prénom sont obligatoires.");

        if (string.IsNullOrWhiteSpace(motDePasseClair))
            ModelState.AddModelError(string.Empty, "Le mot de passe est obligatoire.");

        if (await _db.Utilisateurs.AnyAsync(u => u.Login == utilisateur.Login))
            ModelState.AddModelError(nameof(utilisateur.Login), "Ce login existe déjà.");

        if (!ModelState.IsValid)
        {
            ViewBag.Medecins = await _db.Medecins.Where(m => m.Actif).OrderBy(m => m.Nom).ToListAsync();
            return View(utilisateur);
        }

        utilisateur.MotDePasse = HashMdp(motDePasseClair);
        utilisateur.DateCréation = DateTime.Now;
        _db.Utilisateurs.Add(utilisateur);
        await _db.SaveChangesAsync();

        Succès("Utilisateur créé avec succès.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Modifier(int id)
    {
        if (!EstAdmin) return AccesRefuse();

        var utilisateur = await _db.Utilisateurs.FindAsync(id);
        if (utilisateur == null) return NotFound();

        ViewBag.Medecins = await _db.Medecins.Where(m => m.Actif).OrderBy(m => m.Nom).ToListAsync();
        return View(utilisateur);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modifier(Utilisateur form, string? nouveauMotDePasse)
    {
        if (!EstAdmin) return AccesRefuse();

        var utilisateur = await _db.Utilisateurs.FindAsync(form.Id);
        if (utilisateur == null) return NotFound();

        var login = (form.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(form.Nom) || string.IsNullOrWhiteSpace(form.Prénom))
            ModelState.AddModelError(string.Empty, "Login, nom et prénom sont obligatoires.");

        if (await _db.Utilisateurs.AnyAsync(u => u.Id != form.Id && u.Login == login))
            ModelState.AddModelError(nameof(form.Login), "Ce login existe déjà.");

        if (!ModelState.IsValid)
        {
            ViewBag.Medecins = await _db.Medecins.Where(m => m.Actif).OrderBy(m => m.Nom).ToListAsync();
            return View(form);
        }

        utilisateur.Login = login;
        utilisateur.Nom = form.Nom.Trim();
        utilisateur.Prénom = form.Prénom.Trim();
        utilisateur.Email = form.Email;
        utilisateur.Role = form.Role;
        utilisateur.MedecinId = form.MedecinId;
        utilisateur.Actif = form.Actif;

        if (!string.IsNullOrWhiteSpace(nouveauMotDePasse))
            utilisateur.MotDePasse = HashMdp(nouveauMotDePasse);

        await _db.SaveChangesAsync();
        Succès("Utilisateur mis à jour.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BasculerActif(int id)
    {
        if (!EstAdmin) return AccesRefuse();

        var utilisateur = await _db.Utilisateurs.FindAsync(id);
        if (utilisateur == null) return NotFound();

        if (utilisateur.Id == UtilisateurId)
        {
            Avertissement("Vous ne pouvez pas désactiver votre propre compte.");
            return RedirectToAction(nameof(Index));
        }

        utilisateur.Actif = !utilisateur.Actif;
        await _db.SaveChangesAsync();
        Succès(utilisateur.Actif ? "Utilisateur activé." : "Utilisateur désactivé.");
        return RedirectToAction(nameof(Index));
    }

    private IActionResult AccesRefuse()
    {
        Erreur("Accès réservé aux administrateurs.");
        return RedirectToAction("Index", "Accueil");
    }

    private static string HashMdp(string mdp)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(mdp));
        return Convert.ToHexString(bytes).ToLower();
    }
}
