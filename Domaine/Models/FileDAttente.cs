using System.ComponentModel.DataAnnotations;

namespace GestionHopital.Domaine.Models;

public class FileDAttente
{
    public int Id { get; set; }
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int? MedecinId { get; set; }
    public Medecin? Medecin { get; set; }
    public int NumeroFile { get; set; }
    [MaxLength(150)]
    public string NomVisiteur { get; set; } = "";
    [MaxLength(300)]
    public string MotifVisite { get; set; } = "";
    public DateTime DateArrivee { get; set; } = DateTime.Now;
    public DateTime? DateAppel { get; set; }
    public DateTime? DateFin { get; set; }
    public StatutFileDAttente Statut { get; set; } = StatutFileDAttente.EnAttente;
    public PrioritéFile Priorite { get; set; } = PrioritéFile.Normale;
    [MaxLength(500)]
    public string? Notes { get; set; }

    public string NomAffiche => Patient?.NomComplet ?? NomVisiteur;

    public string PrioriteCssClass => Priorite switch
    {
        PrioritéFile.Critique => "danger",
        PrioritéFile.Urgente => "warning",
        _ => "secondary"
    };

    public string StatutCssClass => Statut switch
    {
        StatutFileDAttente.EnAttente => "secondary",
        StatutFileDAttente.Appelé => "primary",
        StatutFileDAttente.EnConsultation => "info",
        StatutFileDAttente.Terminé => "success",
        StatutFileDAttente.Absent => "danger",
        _ => "secondary"
    };

    public string AttenteFormatée
    {
        get
        {
            var duree = DateTime.Now - DateArrivee;
            if (duree.TotalHours >= 1) return $"{(int)duree.TotalHours}h{duree.Minutes:D2}";
            return $"{(int)duree.TotalMinutes} min";
        }
    }
}
