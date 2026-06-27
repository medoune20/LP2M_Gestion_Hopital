using GestionHopital.Domaine.Models;
using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class EtablissementController : BaseController
{
    public EtablissementController(AppDbContext db) : base(db) { }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!EstAdmin) return AccesRefuse();

        var etablissement = await _db.Etablissements.OrderBy(e => e.Id).FirstOrDefaultAsync();
        if (etablissement == null)
        {
            etablissement = new Etablissement
            {
                Nom = "Hôpital LP2M",
                Couleur = "#0ea5e9",
                DateCréation = DateTime.Now
            };
            _db.Etablissements.Add(etablissement);
            await _db.SaveChangesAsync();
        }

        return View(etablissement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Etablissement form)
    {
        if (!EstAdmin) return AccesRefuse();

        if (string.IsNullOrWhiteSpace(form.Nom))
            ModelState.AddModelError(nameof(form.Nom), "Le nom de l'établissement est obligatoire.");

        if (!ModelState.IsValid)
            return View(form);

        var etablissement = await _db.Etablissements.OrderBy(e => e.Id).FirstOrDefaultAsync();
        if (etablissement == null)
        {
            etablissement = new Etablissement { DateCréation = DateTime.Now };
            _db.Etablissements.Add(etablissement);
        }

        etablissement.Nom = form.Nom.Trim();
        etablissement.Adresse = form.Adresse;
        etablissement.Téléphone = form.Téléphone;
        etablissement.Email = form.Email;
        etablissement.SiteWeb = form.SiteWeb;
        etablissement.Directeur = form.Directeur;
        etablissement.Couleur = string.IsNullOrWhiteSpace(form.Couleur) ? "#0ea5e9" : form.Couleur;
        etablissement.Logo = form.Logo;

        await _db.SaveChangesAsync();
        HttpContext.Session.SetString("CouleurHopital", etablissement.Couleur ?? "#0ea5e9");
        Succès("Paramètres de l'établissement enregistrés.");
        return RedirectToAction(nameof(Index));
    }

    private IActionResult AccesRefuse()
    {
        Erreur("Accès réservé aux administrateurs.");
        return RedirectToAction("Index", "Accueil");
    }
}
