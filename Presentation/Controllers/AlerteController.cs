using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class AlerteController : BaseController
{
    public AlerteController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var alertes = await _db.Alertes
            .OrderBy(a => a.Statut)
            .ThenByDescending(a => a.Niveau)
            .ThenByDescending(a => a.DateCreation)
            .ToListAsync();

        // Générer les alertes automatiques
        await GenererAlertesAutoAsync();

        return View(alertes);
    }

    [HttpGet]
    public async Task<IActionResult> Badge()
    {
        var n = await _db.Alertes.CountAsync(a => a.Statut == StatutAlerte.Active);
        return Json(new { count = n });
    }

    [HttpPost]
    public async Task<IActionResult> MarquerLue(int id)
    {
        var a = await _db.Alertes.FindAsync(id);
        if (a != null) { a.Statut = StatutAlerte.Lue; a.DateLecture = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Resoudre(int id)
    {
        var a = await _db.Alertes.FindAsync(id);
        if (a != null) { a.Statut = StatutAlerte.Résolue; a.DateLecture ??= DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Supprimer(int id)
    {
        var a = await _db.Alertes.FindAsync(id);
        if (a != null) { _db.Alertes.Remove(a); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenererAlertes()
    {
        await GenererAlertesAutoAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task GenererAlertesAutoAsync()
    {
        var aujourd_hui = DateTime.Today;

        // Lits insuffisants
        var libres = await _db.Lits.CountAsync(l => l.Statut == StatutLit.Libre);
        if (libres < 3 && !await _db.Alertes.AnyAsync(a => a.Lien == "/Departement" && a.Statut == StatutAlerte.Active && a.DateCreation >= aujourd_hui))
        {
            _db.Alertes.Add(new Alerte
            {
                Message = $"Seulement {libres} lit(s) libre(s) disponible(s).",
                Niveau = libres == 0 ? NiveauAlerte.Critique : NiveauAlerte.Avertissement,
                Lien = "/Departement",
                Icone = "bi-hospital"
            });
        }

        // Examens en attente depuis plus de 48h
        var examensAnciens = await _db.Examens
            .CountAsync(e => (e.Statut == StatutExamen.Prescrit || e.Statut == StatutExamen.EnAttente)
                             && e.DatePrescription < aujourd_hui.AddDays(-2));
        if (examensAnciens > 0 && !await _db.Alertes.AnyAsync(a => a.Lien == "/Examen" && a.Statut == StatutAlerte.Active && a.DateCreation >= aujourd_hui))
        {
            _db.Alertes.Add(new Alerte
            {
                Message = $"{examensAnciens} examen(s) en attente depuis plus de 48h.",
                Niveau = NiveauAlerte.Avertissement,
                Lien = "/Examen",
                Icone = "bi-droplet-half"
            });
        }

        await _db.SaveChangesAsync();
    }
}
