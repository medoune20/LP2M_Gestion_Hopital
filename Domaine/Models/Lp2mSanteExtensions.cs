using System.ComponentModel.DataAnnotations;
using GestionHopital.Domaine;

namespace GestionHopital.Domaine.Models;

public class ProfilSante
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Nom { get; set; } = "";
    [MaxLength(80)] public string Code { get; set; } = "";
    public RoleUtilisateur RoleBase { get; set; } = RoleUtilisateur.Réceptionniste;
    [MaxLength(2000)] public string Permissions { get; set; } = "dashboard,patients,rendezvous";
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
}

public class FileAttenteSante
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int? MedecinId { get; set; }
    public Medecin? Medecin { get; set; }
    [MaxLength(80)] public string Numero { get; set; } = "";
    [MaxLength(120)] public string Service { get; set; } = "Accueil";
    [MaxLength(500)] public string Motif { get; set; } = "";
    public int Priorite { get; set; } = 3;
    [MaxLength(30)] public string Statut { get; set; } = "En attente";
    public DateTime DateArrivee { get; set; } = DateTime.Now;
    public DateTime? DateAppel { get; set; }
    public DateTime? DateCloture { get; set; }
}

public class TeleconsultationSante
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int? MedecinId { get; set; }
    public Medecin? Medecin { get; set; }
    public DateTime DateHeure { get; set; } = DateTime.Now.AddHours(1);
    [MaxLength(500)] public string Motif { get; set; } = "";
    [MaxLength(500)] public string LienVideo { get; set; } = "";
    [MaxLength(30)] public string Statut { get; set; } = "Planifiée";
    [MaxLength(200)] public string EmailPatient { get; set; } = "";
    [MaxLength(50)] public string TelephonePatient { get; set; } = "";
    public bool NotificationEnvoyee { get; set; }
}

public class PaiementSante
{
    public int Id { get; set; }
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
    [MaxLength(80)] public string Reference { get; set; } = "";
    [MaxLength(120)] public string Libelle { get; set; } = "Consultation";
    public decimal Montant { get; set; }
    [MaxLength(40)] public string ModePaiement { get; set; } = "Espèces";
    [MaxLength(80)] public string ReferenceOperateur { get; set; } = "";
    [MaxLength(30)] public string Statut { get; set; } = "Validé";
    public DateTime DatePaiement { get; set; } = DateTime.Now;
    [MaxLength(120)] public string Caissier { get; set; } = "";
}

public class EcritureComptableSante
{
    public int Id { get; set; }
    public DateTime DateEcriture { get; set; } = DateTime.Now;
    [MaxLength(40)] public string Type { get; set; } = "Recette";
    [MaxLength(120)] public string Categorie { get; set; } = "Consultations";
    [MaxLength(300)] public string Libelle { get; set; } = "";
    public decimal Montant { get; set; }
    [MaxLength(80)] public string Reference { get; set; } = "";
}

public class AlerteSante
{
    public int Id { get; set; }
    [MaxLength(40)] public string Niveau { get; set; } = "Info";
    [MaxLength(120)] public string Module { get; set; } = "Système";
    [MaxLength(300)] public string Message { get; set; } = "";
    public bool Traitee { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime? DateTraitement { get; set; }
}

public class AccesPatientSante
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    [MaxLength(120)] public string Identifiant { get; set; } = "";
    [MaxLength(200)] public string MotDePasseHash { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(50)] public string Telephone { get; set; } = "";
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
}

public class EmailJournalSante
{
    public int Id { get; set; }
    [MaxLength(200)] public string Destinataire { get; set; } = "";
    [MaxLength(200)] public string Sujet { get; set; } = "";
    public string Corps { get; set; } = "";
    public bool Envoye { get; set; }
    [MaxLength(500)] public string Erreur { get; set; } = "";
    public DateTime DateCreation { get; set; } = DateTime.Now;
}
