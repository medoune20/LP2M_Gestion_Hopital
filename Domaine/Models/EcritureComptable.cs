using System.ComponentModel.DataAnnotations;

namespace GestionHopital.Domaine.Models;

public class EcritureComptable
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    [MaxLength(300)]
    public string Libelle { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    [MaxLength(50)]
    public string? ReferenceSource { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }

    public decimal Solde => Credit - Debit;
}
