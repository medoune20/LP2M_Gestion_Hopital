using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using GestionHopital.Domaine;
using GestionHopital.Domaine.Models;
using GestionHopital.Infrastructure.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Presentation.Controllers;

public class Lp2mController : BaseController
{
    public Lp2mController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        ViewBag.Etablissements = await _db.Etablissements.OrderBy(e => e.Nom).ToListAsync();
        ViewBag.Utilisateurs = await _db.Utilisateurs.OrderBy(u => u.Nom).ThenBy(u => u.Prénom).ToListAsync();
        ViewBag.Patients = await _db.Patients.OrderBy(p => p.Nom).ThenBy(p => p.Prénom).Take(200).ToListAsync();
        ViewBag.Medecins = await _db.Medecins.OrderBy(m => m.Nom).ThenBy(m => m.Prénom).ToListAsync();

        ViewBag.Profils = await QueryAsync("SELECT Id, Nom, Code, RoleBase, Permissions, Actif FROM ProfilsSante ORDER BY Nom");
        ViewBag.FileAttente = await QueryAsync("""
SELECT f.Id, f.Numero, f.Service, f.Motif, f.Priorite, f.Statut, f.DateArrivee,
       COALESCE(p.Prénom || ' ' || p.Nom, 'Patient') AS Patient,
       COALESCE(m.Prénom || ' ' || m.Nom, '') AS Medecin
FROM FileAttenteSante f
LEFT JOIN Patients p ON p.Id = f.PatientId
LEFT JOIN Medecins m ON m.Id = f.MedecinId
WHERE f.Statut <> 'Clôturé'
ORDER BY f.Priorite ASC, f.DateArrivee ASC
LIMIT 30
""");
        ViewBag.Teleconsultations = await QueryAsync("""
SELECT t.Id, t.DateHeure, t.Motif, t.Statut, t.LienVideo, t.NotificationEnvoyee,
       COALESCE(p.Prénom || ' ' || p.Nom, 'Patient') AS Patient,
       COALESCE(m.Prénom || ' ' || m.Nom, '') AS Medecin
FROM TeleconsultationsSante t
LEFT JOIN Patients p ON p.Id = t.PatientId
LEFT JOIN Medecins m ON m.Id = t.MedecinId
ORDER BY t.DateHeure DESC
LIMIT 30
""");
        ViewBag.Paiements = await QueryAsync("""
SELECT pa.Id, pa.Reference, pa.Libelle, pa.Montant, pa.ModePaiement, pa.Statut, pa.DatePaiement,
       COALESCE(p.Prénom || ' ' || p.Nom, 'Patient comptoir') AS Patient
FROM PaiementsSante pa
LEFT JOIN Patients p ON p.Id = pa.PatientId
ORDER BY pa.DatePaiement DESC
LIMIT 30
""");
        ViewBag.Ecritures = await QueryAsync("SELECT Id, DateEcriture, Type, Categorie, Libelle, Montant, Reference FROM EcrituresComptablesSante ORDER BY DateEcriture DESC LIMIT 20");
        ViewBag.Alertes = await QueryAsync("SELECT Id, Niveau, Module, Message, Traitee, DateCreation FROM AlertesSante ORDER BY Traitee ASC, DateCreation DESC LIMIT 30");
        ViewBag.AccesPatients = await QueryAsync("""
SELECT a.Id, a.Identifiant, a.Email, a.Telephone, a.Actif, a.DateCreation,
       COALESCE(p.Prénom || ' ' || p.Nom, 'Patient') AS Patient
FROM AccesPatientsSante a
LEFT JOIN Patients p ON p.Id = a.PatientId
ORDER BY a.DateCreation DESC
LIMIT 30
""");
        ViewBag.Emails = await QueryAsync("SELECT Id, Destinataire, Sujet, Envoye, Erreur, DateCreation FROM EmailsJournalSante ORDER BY DateCreation DESC LIMIT 20");

        ViewBag.TotalRecettesJour = await ScalarDecimalAsync("SELECT COALESCE(SUM(Montant), 0) FROM PaiementsSante WHERE DatePaiement >= @p0", today.ToString("yyyy-MM-dd"));
        ViewBag.FileEnAttente = await ScalarIntAsync("SELECT COUNT(*) FROM FileAttenteSante WHERE Statut = 'En attente'");
        ViewBag.TeleconsultationsAvenir = await ScalarIntAsync("SELECT COUNT(*) FROM TeleconsultationsSante WHERE Statut <> 'Terminée'");
        ViewBag.AlertesOuvertes = await ScalarIntAsync("SELECT COUNT(*) FROM AlertesSante WHERE Traitee = 0");

        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerEtablissement(string nom, string? adresse, string? telephone, string? email)
    {
        if (!EstAdmin) return Forbid();
        if (string.IsNullOrWhiteSpace(nom)) { Erreur("Le nom de l'établissement est obligatoire."); return RedirectToAction(nameof(Index)); }
        _db.Etablissements.Add(new Etablissement { Nom = nom.Trim(), Adresse = adresse, Téléphone = telephone, Email = email, Couleur = "#0ea5e9" });
        await _db.SaveChangesAsync();
        Succès("Établissement / tenant créé.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerUtilisateur(string login, string nom, string prenom, string? email, int role, string motDePasse)
    {
        if (!EstAdmin) return Forbid();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(motDePasse)) { Erreur("Login et mot de passe obligatoires."); return RedirectToAction(nameof(Index)); }
        if (await _db.Utilisateurs.AnyAsync(u => u.Login == login.Trim())) { Erreur("Ce login existe déjà."); return RedirectToAction(nameof(Index)); }
        _db.Utilisateurs.Add(new Utilisateur
        {
            Login = login.Trim(), Nom = nom?.Trim() ?? "", Prénom = prenom?.Trim() ?? "", Email = email,
            MotDePasse = DatabaseInitializer.Hash(motDePasse), Role = (RoleUtilisateur)role, Actif = true
        });
        await _db.SaveChangesAsync();
        Succès("Utilisateur créé.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerProfil(string nom, string code, int roleBase, string permissions)
    {
        if (!EstAdmin) return Forbid();
        await ExecAsync("INSERT INTO ProfilsSante (Nom, Code, RoleBase, Permissions, Actif) VALUES (@p0,@p1,@p2,@p3,1)", nom, code, roleBase, permissions);
        Succès("Profil créé.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AjouterFile(int patientId, int? medecinId, string service, string motif, int priorite)
    {
        var numero = $"Q-{DateTime.Now:yyyyMMddHHmmss}";
        await ExecAsync("INSERT INTO FileAttenteSante (PatientId, MedecinId, Numero, Service, Motif, Priorite, Statut) VALUES (@p0,@p1,@p2,@p3,@p4,@p5,'En attente')",
            patientId, medecinId, numero, service, motif, priorite);
        Succès($"Patient ajouté à la file : {numero}.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerFile(int id, string statut)
    {
        var dateCol = statut == "Appelé" ? "DateAppel" : statut == "Clôturé" ? "DateCloture" : null;
        if (dateCol == null) await ExecAsync("UPDATE FileAttenteSante SET Statut=@p0 WHERE Id=@p1", statut, id);
        else await ExecAsync($"UPDATE FileAttenteSante SET Statut=@p0, {dateCol}=CURRENT_TIMESTAMP WHERE Id=@p1", statut, id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerTeleconsultation(int patientId, int? medecinId, DateTime dateHeure, string motif, string? lienVideo, string? emailPatient, string? telephonePatient)
    {
        await ExecAsync("""
INSERT INTO TeleconsultationsSante (PatientId, MedecinId, DateHeure, Motif, LienVideo, Statut, EmailPatient, TelephonePatient)
VALUES (@p0,@p1,@p2,@p3,@p4,'Planifiée',@p5,@p6)
""", patientId, medecinId, dateHeure.ToString("s"), motif, lienVideo ?? "", emailPatient ?? "", telephonePatient ?? "");
        Succès("Téléconsultation planifiée.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnregistrerPaiement(int? patientId, string libelle, decimal montant, string modePaiement, string? referenceOperateur)
    {
        var reference = $"PAY-{DateTime.Now:yyyyMMddHHmmss}";
        await ExecAsync("""
INSERT INTO PaiementsSante (PatientId, Reference, Libelle, Montant, ModePaiement, ReferenceOperateur, Statut, Caissier)
VALUES (@p0,@p1,@p2,@p3,@p4,@p5,'Validé',@p6)
""", patientId, reference, libelle, montant, modePaiement, referenceOperateur ?? "", UtilisateurNom);
        await ExecAsync("INSERT INTO EcrituresComptablesSante (Type, Categorie, Libelle, Montant, Reference) VALUES ('Recette','Paiement patient',@p0,@p1,@p2)", libelle, montant, reference);
        Succès($"Paiement validé : {reference}.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerAlerte(string niveau, string module, string message)
    {
        await ExecAsync("INSERT INTO AlertesSante (Niveau, Module, Message) VALUES (@p0,@p1,@p2)", niveau, module, message);
        Succès("Alerte enregistrée.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TraiterAlerte(int id)
    {
        await ExecAsync("UPDATE AlertesSante SET Traitee=1, DateTraitement=CURRENT_TIMESTAMP WHERE Id=@p0", id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerAccesPatient(int patientId, string identifiant, string motDePasse, string? email, string? telephone)
    {
        await ExecAsync("INSERT INTO AccesPatientsSante (PatientId, Identifiant, MotDePasseHash, Email, Telephone, Actif) VALUES (@p0,@p1,@p2,@p3,@p4,1)",
            patientId, identifiant, DatabaseInitializer.Hash(motDePasse), email ?? "", telephone ?? "");
        Succès("Accès patient créé.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnvoyerMail(string destinataire, string sujet, string corps)
    {
        var (ok, erreur) = EnvoyerEmail(destinataire, sujet, corps);
        await ExecAsync("INSERT INTO EmailsJournalSante (Destinataire, Sujet, Corps, Envoye, Erreur) VALUES (@p0,@p1,@p2,@p3,@p4)", destinataire, sujet, corps, ok ? 1 : 0, erreur);
        if (ok) Succès("Email envoyé."); else Erreur("Email non envoyé : " + erreur);
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Dictionary<string, string>>> QueryAsync(string sql, params object?[] parameters)
    {
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        AddParams(cmd, parameters);
        if (cmd.Connection!.State != ConnectionState.Open) await cmd.Connection.OpenAsync();
        var rows = new List<Dictionary<string, string>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, string>();
            for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? "" : Convert.ToString(reader.GetValue(i)) ?? "";
            rows.Add(row);
        }
        return rows;
    }

    private async Task ExecAsync(string sql, params object?[] parameters)
    {
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        AddParams(cmd, parameters);
        if (cmd.Connection!.State != ConnectionState.Open) await cmd.Connection.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<int> ScalarIntAsync(string sql, params object?[] parameters)
    {
        var value = await ScalarAsync(sql, parameters);
        return Convert.ToInt32(value ?? 0);
    }

    private async Task<decimal> ScalarDecimalAsync(string sql, params object?[] parameters)
    {
        var value = await ScalarAsync(sql, parameters);
        return Convert.ToDecimal(value ?? 0);
    }

    private async Task<object?> ScalarAsync(string sql, params object?[] parameters)
    {
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        AddParams(cmd, parameters);
        if (cmd.Connection!.State != ConnectionState.Open) await cmd.Connection.OpenAsync();
        return await cmd.ExecuteScalarAsync();
    }

    private static void AddParams(IDbCommand cmd, params object?[] parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@p" + i;
            p.Value = parameters[i] ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }

    private static (bool ok, string erreur) EnvoyerEmail(string destinataire, string sujet, string corps)
    {
        try
        {
            var host = Environment.GetEnvironmentVariable("SMTP_HOST");
            var user = Environment.GetEnvironmentVariable("SMTP_USER");
            var pass = Environment.GetEnvironmentVariable("SMTP_PASS");
            var from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? user;
            var port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var p) ? p : 587;
            var ssl = (Environment.GetEnvironmentVariable("SMTP_SSL") ?? "true").ToLowerInvariant() != "false";
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)) return (false, "SMTP non configuré.");

            using var client = new SmtpClient(host, port) { EnableSsl = ssl, Credentials = new NetworkCredential(user, pass), DeliveryMethod = SmtpDeliveryMethod.Network };
            using var msg = new MailMessage { From = new MailAddress(from!), Subject = sujet, Body = corps, IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
            msg.To.Add(destinataire);
            client.Send(msg);
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
