using System.ComponentModel.DataAnnotations;

namespace GestionHopital.Domaine.Models;

public class Alerte
{
    public int Id { get; set; }
    [MaxLength(500)]
    public string Message { get; set; } = "";
    public NiveauAlerte Niveau { get; set; } = NiveauAlerte.Info;
    public StatutAlerte Statut { get; set; } = StatutAlerte.Active;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime? DateLecture { get; set; }
    [MaxLength(200)]
    public string? Lien { get; set; }
    [MaxLength(50)]
    public string? Icone { get; set; }

    public string NiveauCssClass => Niveau switch
    {
        NiveauAlerte.Critique => "danger",
        NiveauAlerte.Avertissement => "warning",
        _ => "info"
    };

    public string NiveauIcone => Icone ?? (Niveau switch
    {
        NiveauAlerte.Critique => "bi-exclamation-octagon-fill",
        NiveauAlerte.Avertissement => "bi-exclamation-triangle-fill",
        _ => "bi-info-circle-fill"
    });
}
