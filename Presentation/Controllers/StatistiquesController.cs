using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class StatistiquesController : BaseController
{
    public StatistiquesController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var aujourd_hui = DateTime.Today;

        // KPIs globaux
        ViewBag.TotalPatients = await _db.Patients.CountAsync(p => p.Statut != StatutPatient.Décédé);
        ViewBag.NouveauxMois = await _db.Patients
            .CountAsync(p => p.DateEnregistrement >= new DateTime(aujourd_hui.Year, aujourd_hui.Month, 1));
        ViewBag.ConsultationsMois = await _db.Consultations
            .CountAsync(c => c.DateHeure >= new DateTime(aujourd_hui.Year, aujourd_hui.Month, 1));
        ViewBag.RevenuMois = await _db.Paiements
            .Where(p => p.DatePaiement >= new DateTime(aujourd_hui.Year, aujourd_hui.Month, 1) && p.Statut == StatutPaiement.Payé)
            .SumAsync(p => (decimal?)p.Montant) ?? 0;
        ViewBag.PatientsHospitalisés = await _db.Patients.CountAsync(p => p.Statut == StatutPatient.Hospitalisé);
        ViewBag.TauxOccupation = await CalculerTauxOccupation();

        // Consultations 12 mois
        var labMois = new List<string>();
        var dataMois = new List<int>();
        var revenuMois = new List<decimal>();
        for (int i = 11; i >= 0; i--)
        {
            var m = aujourd_hui.AddMonths(-i);
            var d1 = new DateTime(m.Year, m.Month, 1);
            var d2 = d1.AddMonths(1);
            labMois.Add(m.ToString("MMM yy"));
            dataMois.Add(await _db.Consultations.CountAsync(c => c.DateHeure >= d1 && c.DateHeure < d2));
            revenuMois.Add(await _db.Paiements.Where(p => p.DatePaiement >= d1 && p.DatePaiement < d2 && p.Statut == StatutPaiement.Payé).SumAsync(p => (decimal?)p.Montant) ?? 0);
        }
        ViewBag.LabMois = labMois;
        ViewBag.DataConsult = dataMois;
        ViewBag.DataRevenu = revenuMois;

        // Répartition par sexe
        ViewBag.NbMasculin = await _db.Patients.CountAsync(p => p.Sexe == SexePatient.Masculin);
        ViewBag.NbFéminin = await _db.Patients.CountAsync(p => p.Sexe == SexePatient.Féminin);

        // Top médecins (nb consultations)
        ViewBag.TopMedecins = await _db.Consultations
            .Include(c => c.Medecin)
            .GroupBy(c => c.MedecinId)
            .Select(g => new { MedecinId = g.Key, Nb = g.Count() })
            .OrderByDescending(x => x.Nb)
            .Take(5)
            .Join(_db.Medecins, x => x.MedecinId, m => m.Id, (x, m) => new { m.NomComplet, x.Nb })
            .ToListAsync();

        // Répartition modes de paiement
        ViewBag.StatsPaiement = await _db.Paiements
            .Where(p => p.Statut == StatutPaiement.Payé)
            .GroupBy(p => p.ModePaiement)
            .Select(g => new { Mode = g.Key, Total = g.Sum(p => p.Montant) })
            .ToListAsync();

        return View();
    }

    private async Task<int> CalculerTauxOccupation()
    {
        var total = await _db.Lits.CountAsync();
        if (total == 0) return 0;
        var occupes = await _db.Lits.CountAsync(l => l.Statut == StatutLit.Occupé);
        return (int)(occupes * 100.0 / total);
    }
}
