using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class ComptabiliteController : BaseController
{
    public ComptabiliteController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(int mois = 0, int annee = 0)
    {
        if (mois == 0) mois = DateTime.Today.Month;
        if (annee == 0) annee = DateTime.Today.Year;

        var debut = new DateTime(annee, mois, 1);
        var fin = debut.AddMonths(1);

        var ecritures = await _db.EcrituresComptables
            .Where(e => e.Date >= debut && e.Date < fin)
            .OrderBy(e => e.Date)
            .ToListAsync();

        ViewBag.TotalCredits = ecritures.Sum(e => e.Credit);
        ViewBag.TotalDebits = ecritures.Sum(e => e.Debit);
        ViewBag.Solde = ecritures.Sum(e => e.Credit) - ecritures.Sum(e => e.Debit);
        ViewBag.Mois = mois;
        ViewBag.Annee = annee;

        // Évolution sur 6 mois
        var labels = new List<string>();
        var credits = new List<decimal>();
        var debits = new List<decimal>();
        for (int i = 5; i >= 0; i--)
        {
            var m = DateTime.Today.AddMonths(-i);
            var d1 = new DateTime(m.Year, m.Month, 1);
            var d2 = d1.AddMonths(1);
            labels.Add(m.ToString("MMM yyyy"));
            credits.Add(await _db.EcrituresComptables.Where(e => e.Date >= d1 && e.Date < d2).SumAsync(e => (decimal?)e.Credit) ?? 0);
            debits.Add(await _db.EcrituresComptables.Where(e => e.Date >= d1 && e.Date < d2).SumAsync(e => (decimal?)e.Debit) ?? 0);
        }
        ViewBag.GraphLabels = labels;
        ViewBag.GraphCredits = credits;
        ViewBag.GraphDebits = debits;

        return View(ecritures);
    }

    [HttpPost]
    public async Task<IActionResult> AjouterEcriture(EcritureComptable ecriture)
    {
        ecriture.Date = DateTime.Now;
        _db.EcrituresComptables.Add(ecriture);
        await _db.SaveChangesAsync();
        Succès("Écriture comptable enregistrée.");
        return RedirectToAction(nameof(Index));
    }
}
