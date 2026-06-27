using System.ComponentModel.DataAnnotations;

namespace GestionHopital.Domaine.Models;

public class JournalMail
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Destinataire { get; set; } = "";
    [MaxLength(300)]
    public string Sujet { get; set; } = "";
    public string Corps { get; set; } = "";
    public StatutMail Statut { get; set; } = StatutMail.EnAttente;
    public DateTime DateEnvoi { get; set; } = DateTime.Now;
    [MaxLength(1000)]
    public string? Erreur { get; set; }
    public int UtilisateurId { get; set; }

    public string StatutCssClass => Statut switch
    {
        StatutMail.Envoyé => "success",
        StatutMail.Echec => "danger",
        _ => "warning"
    };
}
