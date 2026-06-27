using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class FileAttenteController : BaseController
{
    public FileAttenteController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var aujourd_hui = DateTime.Today;
        var demain = aujourd_hui.AddDays(1);

        var file = await _db.FileAttente
            .Include(f => f.Patient)
            .Include(f => f.Medecin)
            .Where(f => f.DateArrivee >= aujourd_hui && f.DateArrivee < demain)
            .OrderBy(f => f.Priorite == PrioritéFile.Critique ? 0 : f.Priorite == PrioritéFile.Urgente ? 1 : 2)
            .ThenBy(f => f.NumeroFile)
            .ToListAsync();

        ViewBag.Medecins = await _db.Medecins.Where(m => m.Actif).OrderBy(m => m.Nom).ToListAsync();
        ViewBag.EnAttente = file.Count(f => f.Statut == StatutFileDAttente.EnAttente);
        ViewBag.EnConsultation = file.Count(f => f.Statut == StatutFileDAttente.EnConsultation);
        ViewBag.Terminé = file.Count(f => f.Statut == StatutFileDAttente.Terminé);
        return View(file);
    }

    [HttpPost]
    public async Task<IActionResult> Ajouter(int? patientId, string nomVisiteur, string motifVisite,
        int? medecinId, PrioritéFile priorite)
    {
        var max = await _db.FileAttente
            .Where(f => f.DateArrivee >= DateTime.Today)
            .MaxAsync(f => (int?)f.NumeroFile) ?? 0;

        var entree = new FileDAttente
        {
            PatientId = patientId,
            NomVisiteur = nomVisiteur ?? "",
            MotifVisite = motifVisite ?? "",
            MedecinId = medecinId,
            Priorite = priorite,
            NumeroFile = max + 1,
            DateArrivee = DateTime.Now,
            Statut = StatutFileDAttente.EnAttente
        };
        _db.FileAttente.Add(entree);
        await _db.SaveChangesAsync();
        Succès($"Patient #{entree.NumeroFile} ajouté à la file.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Appeler(int id)
    {
        var f = await _db.FileAttente.FindAsync(id);
        if (f != null) { f.Statut = StatutFileDAttente.Appelé; f.DateAppel = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> EnConsultation(int id)
    {
        var f = await _db.FileAttente.FindAsync(id);
        if (f != null) { f.Statut = StatutFileDAttente.EnConsultation; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Terminer(int id)
    {
        var f = await _db.FileAttente.FindAsync(id);
        if (f != null) { f.Statut = StatutFileDAttente.Terminé; f.DateFin = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Absent(int id)
    {
        var f = await _db.FileAttente.FindAsync(id);
        if (f != null) { f.Statut = StatutFileDAttente.Absent; f.DateFin = DateTime.Now; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RechercherPatient(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new object[0]);
        q = q.ToLower();
        var patients = await _db.Patients
            .Where(p => p.Nom.ToLower().Contains(q) || p.Prénom.ToLower().Contains(q) || p.NuméroDossier.ToLower().Contains(q))
            .Take(8)
            .Select(p => new { p.Id, nom = p.NomComplet, dossier = p.NuméroDossier })
            .ToListAsync();
        return Json(patients);
    }
}
