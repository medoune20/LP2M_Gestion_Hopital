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
            DateEnvoi = DateTime.Now
        };

        try
        {
            var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var user = _config["Smtp:User"] ?? "";
            var pass = _config["Smtp:Password"] ?? "";
            var fromName = _config["Smtp:FromName"] ?? "LP2M Santé";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                throw new InvalidOperationException("Configuration SMTP manquante (Smtp:User / Smtp:Password).");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(user, fromName),
                Subject = sujet,
                Body = corpsHtml,
                IsBodyHtml = true
            };
            msg.To.Add(destinataire);

            await client.SendMailAsync(msg);
            journal.Statut = StatutMail.Envoyé;
        }
        catch (Exception ex)
        {
            journal.Statut = StatutMail.Echec;
            journal.Erreur = ex.Message.Length > 990 ? ex.Message[..990] : ex.Message;
        }

        _db.JournalMails.Add(journal);
        await _db.SaveChangesAsync();
        return journal.Statut == StatutMail.Envoyé;
    }
}
