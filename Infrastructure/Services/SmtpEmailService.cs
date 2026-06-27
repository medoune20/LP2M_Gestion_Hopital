using GestionHopital.Domaine;
using GestionHopital.Domaine.Models;
using GestionHopital.Infrastructure.Donnees;
using System.Net;
using System.Net.Mail;

namespace GestionHopital.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public SmtpEmailService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<bool> EnvoyerAsync(string destinataire, string sujet, string corpsHtml, int utilisateurId = 0)
    {
        var journal = new JournalMail
        {
            Destinataire = destinataire,
            Sujet = sujet,
            Corps = corpsHtml,
            UtilisateurId = utilisateurId,
            DateEnvoi = DateTime.Now,
            Statut = StatutMail.EnAttente
        };

        try
        {
            var host = Lire("Smtp:Host", "SMTP_HOST");
            var port = LireInt("Smtp:Port", "SMTP_PORT", 587);
            var user = Lire("Smtp:User", "SMTP_USER");
            var pass = Lire("Smtp:Password", "SMTP_PASS", "Smtp:Pass", "SMTP_PASSWORD");
            var from = Lire("Smtp:From", "SMTP_FROM");
            var fromName = Lire("Smtp:FromName", "SMTP_FROM_NAME") ?? "LP2M Santé";
            var enableSsl = LireBool("Smtp:Ssl", "SMTP_SSL", true);

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("SMTP_HOST manquant. Renseignez SMTP_HOST dans le fichier .env ou dans docker-compose.yml.");
            if (string.IsNullOrWhiteSpace(user))
                throw new InvalidOperationException("SMTP_USER manquant. Renseignez le compte SMTP.");
            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("SMTP_PASS manquant. Renseignez le mot de passe SMTP ou le mot de passe d'application.");

            from = string.IsNullOrWhiteSpace(from) ? user : from;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = sujet,
                Body = string.IsNullOrWhiteSpace(corpsHtml) ? " " : corpsHtml,
                IsBodyHtml = true
            };
            msg.To.Add(destinataire);

            await client.SendMailAsync(msg);
            journal.Statut = StatutMail.Envoyé;
        }
        catch (Exception ex)
        {
            journal.Statut = StatutMail.Echec;
            var details = ex.InnerException == null ? ex.Message : ex.Message + " | " + ex.InnerException.Message;
            journal.Erreur = details.Length > 990 ? details[..990] : details;
        }

        _db.JournalMails.Add(journal);
        await _db.SaveChangesAsync();
        return journal.Statut == StatutMail.Envoyé;
    }

    private string? Lire(params string[] cles)
    {
        foreach (var cle in cles)
        {
            var valeur = _config[cle];
            if (!string.IsNullOrWhiteSpace(valeur))
                return valeur.Trim();
        }
        return null;
    }

    private int LireInt(string cle1, string cle2, int defaut)
    {
        var valeur = Lire(cle1, cle2);
        return int.TryParse(valeur, out var n) ? n : defaut;
    }

    private bool LireBool(string cle1, string cle2, bool defaut)
    {
        var valeur = Lire(cle1, cle2);
        if (string.IsNullOrWhiteSpace(valeur)) return defaut;
        return valeur.Equals("true", StringComparison.OrdinalIgnoreCase)
            || valeur.Equals("1")
            || valeur.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || valeur.Equals("oui", StringComparison.OrdinalIgnoreCase);
    }
}
