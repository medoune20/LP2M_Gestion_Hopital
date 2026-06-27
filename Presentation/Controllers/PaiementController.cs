using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class PaiementController : BaseController
{
    public PaiementController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? statut, string? mode, DateTime? du, DateTime? au)
    {
        var q = _db.Paiements
            .Include(p => p.Patient)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statut) && Enum.TryParse<StatutPaiement>(statut, out var sp))
            q = q.Where(p => p.Statut == sp);
        if (!string.IsNullOrEmpty(mode) && Enum.TryParse<ModePaiement>(mode, out var mp))
            q = q.Where(p => p.ModePaiement == mp);
        if (du.HasValue) q = q.Where(p => p.DatePaiement >= du.Value);
        if (au.HasValue) q = q.Where(p => p.DatePaiement < au.Value.AddDays(1));

        var liste = await q.OrderByDescending(p => p.DatePaiement).Take(200).ToListAsync();

        ViewBag.TotalJour = await _db.Paiements
            .Where(p => p.DatePaiement >= DateTime.Today && p.Statut == StatutPaiement.Payé)
            .SumAsync(p => (decimal?)p.Montant) ?? 0;
        ViewBag.TotalMois = await _db.Paiements
            .Where(p => p.DatePaiement >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) && p.Statut == StatutPaiement.Payé)
            .SumAsync(p => (decimal?)p.Montant) ?? 0;
        ViewBag.EnAttente = await _db.Paiements.CountAsync(p => p.Statut == StatutPaiement.EnAttente);

        return View(liste);
    }

    [HttpGet]
    public async Task<IActionResult> Nouveau(int? patientId, int? consultationId, int? hospitalisationId)
    {
        ViewBag.Patients = await _db.Patients
            .Where(p => p.Statut != StatutPatient.Décédé)
            .OrderBy(p => p.Nom).ToListAsync();
        ViewBag.PatientId = patientId;
        ViewBag.ConsultationId = consultationId;
        ViewBag.HospitalisationId = hospitalisationId;

        if (patientId.HasValue)
            ViewBag.PatientSelecte = await _db.Patients.FindAsync(patientId);
        if (consultationId.HasValue)
            ViewBag.ConsultationSelecte = await _db.Consultations.Include(c => c.Patient).FirstOrDefaultAsync(c => c.Id == consultationId);

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Nouveau(Paiement paiement)
    {
        paiement.UtilisateurId = UtilisateurId;
        paiement.DatePaiement = DateTime.Now;
        paiement.Reference = $"PAY-{DateTime.Now:yyyyMMdd-HHmmss}";

        _db.Paiements.Add(paiement);

        // Écriture comptable automatique
        if (paiement.Statut == StatutPaiement.Payé && paiement.Montant > 0)
        {
            _db.EcrituresComptables.Add(new EcritureComptable
            {
                Date = DateTime.Now,
                Libelle = string.IsNullOrEmpty(paiement.LibellePrestation)
                    ? $"Recette patient — {paiement.Reference}"
                    : paiement.LibellePrestation,
                Credit = paiement.Montant,
                Debit = 0,
                ReferenceSource = paiement.Reference
            });
        }

        await _db.SaveChangesAsync();
        Succès($"Paiement {paiement.Reference} enregistré ({paiement.Montant:N0} FCFA).");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Paiements
            .Include(x => x.Patient)
            .Include(x => x.Consultation)
            .Include(x => x.Hospitalisation)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost]
    public async Task<IActionResult> Annuler(int id)
    {
        var p = await _db.Paiements.FindAsync(id);
        if (p != null) { p.Statut = StatutPaiement.Annulé; await _db.SaveChangesAsync(); Succès("Paiement annulé."); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RechercherPatient(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new object[0]);
        q = q.ToLower();
        var patients = await _db.Patients
            .Where(p => p.Nom.ToLower().Contains(q) || p.Prénom.ToLower().Contains(q) || p.NuméroDossier.ToLower().Contains(q))
            .Take(8).Select(p => new { p.Id, nom = p.NomComplet, dossier = p.NuméroDossier }).ToListAsync();
        return Json(patients);
    }
}
