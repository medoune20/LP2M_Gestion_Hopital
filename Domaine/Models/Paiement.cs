using System.ComponentModel.DataAnnotations;

namespace GestionHopital.Domaine.Models;

public class Paiement
{
    public int Id { get; set; }
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int? ConsultationId { get; set; }
    public Consultation? Consultation { get; set; }
    public int? HospitalisationId { get; set; }
    public Hospitalisation? Hospitalisation { get; set; }
    public decimal Montant { get; set; }
    public ModePaiement ModePaiement { get; set; } = ModePaiement.Especes;
    public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
    [MaxLength(50)]
    public string Reference { get; set; } = "";
    [MaxLength(300)]
    public string LibellePrestation { get; set; } = "";
    [MaxLength(500)]
    public string? Notes { get; set; }
    [MaxLength(30)]
    public string? NumeroPaiementMobile { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.Now;
    public int UtilisateurId { get; set; }

    public string ModePaiementLibelle => ModePaiement switch
    {
        ModePaiement.Especes => "Espèces",
        ModePaiement.MobileMoney => "Mobile Money",
        ModePaiement.CarteBancaire => "Carte bancaire",
        ModePaiement.Cheque => "Chèque",
        ModePaiement.Gratuit => "Gratuit",
        _ => "Inconnu"
    };

    public string StatutCssClass => Statut switch
    {
        StatutPaiement.Payé => "success",
        StatutPaiement.EnAttente => "warning",
        StatutPaiement.Partiel => "info",
        StatutPaiement.Annulé => "danger",
        _ => "secondary"
    };
}
