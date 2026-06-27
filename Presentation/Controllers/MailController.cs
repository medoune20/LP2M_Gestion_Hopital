using GestionHopital.Infrastructure.Donnees;
using GestionHopital.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class MailController : BaseController
{
    private readonly IEmailService _email;

    public MailController(AppDbContext db, IEmailService email) : base(db)
    {
        _email = email;
    }

    [HttpGet]
    public async Task<IActionResult> Composer(string? destinataire, string? sujet, string? corps)
    {
        ViewBag.Destinataire = destinataire ?? "";
        ViewBag.Sujet = sujet ?? "";
        ViewBag.Corps = corps ?? "";
        ViewBag.Patients = await _db.Patients
            .Where(p => !string.IsNullOrEmpty(p.Email))
            .OrderBy(p => p.Nom)
            .Select(p => new { p.Email, p.NomComplet })
            .ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Composer(EnvoiMailVm vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Destinataire) || string.IsNullOrWhiteSpace(vm.Sujet))
        {
            Erreur("Destinataire et sujet sont requis.");
            return RedirectToAction(nameof(Composer));
        }

        var ok = await _email.EnvoyerAsync(vm.Destinataire.Trim(), vm.Sujet.Trim(), vm.Corps ?? "", UtilisateurId);
        if (ok)
            Succès($"Mail envoyé à {vm.Destinataire}.");
        else
            Erreur("Échec de l'envoi. Vérifiez la configuration SMTP.");

        return RedirectToAction(nameof(Journal));
    }

    public async Task<IActionResult> Journal()
    {
        var mails = await _db.JournalMails
            .OrderByDescending(m => m.DateEnvoi)
            .Take(200)
            .ToListAsync();
        return View(mails);
    }

    public async Task<IActionResult> Apercu(int id)
    {
        var m = await _db.JournalMails.FindAsync(id);
        if (m == null) return NotFound();
        return Content(m.Corps, "text/html");
    }
}

public class EnvoiMailVm
{
    public string Destinataire { get; set; } = "";
    public string Sujet { get; set; } = "";
    public string Corps { get; set; } = "";
}
