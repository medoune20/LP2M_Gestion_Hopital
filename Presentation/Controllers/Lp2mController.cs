using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class Lp2mController : BaseController
{
    public Lp2mController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        ViewBag.TotalPatients = await _db.Patients.CountAsync();
        ViewBag.TotalUtilisateurs = await _db.Utilisateurs.CountAsync();
        ViewBag.TotalMedecins = await _db.Medecins.CountAsync();
        ViewBag.FileEnAttente = await _db.FileAttente.CountAsync(f => f.Statut == StatutFileDAttente.EnAttente || f.Statut == StatutFileDAttente.Appelé);
        ViewBag.PaiementsJour = await _db.Paiements.Where(p => p.DatePaiement >= DateTime.Today && p.Statut == StatutPaiement.Payé).SumAsync(p => (decimal?)p.Montant) ?? 0;
        ViewBag.PaiementsMois = await _db.Paiements.Where(p => p.DatePaiement >= debutMois && p.Statut == StatutPaiement.Payé).SumAsync(p => (decimal?)p.Montant) ?? 0;
        ViewBag.AlertesActives = await _db.Alertes.CountAsync(a => a.Statut == StatutAlerte.Active);
        ViewBag.MailsEnvoyes = await _db.JournalMails.CountAsync(m => m.Statut == StatutMail.Envoyé);

        ViewBag.PaiementsRecents = await _db.Paiements.Include(p => p.Patient).OrderByDescending(p => p.DatePaiement).Take(8).ToListAsync();
        ViewBag.FileRecente = await _db.FileAttente.Include(f => f.Patient).Include(f => f.Medecin).OrderByDescending(f => f.DateArrivee).Take(8).ToListAsync();
        ViewBag.AlertesRecentes = await _db.Alertes.OrderByDescending(a => a.DateCreation).Take(8).ToListAsync();

        return View();
    }
}
