using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Infrastructure.Donnees;

public static class Lp2mSanteSchema
{
    public static async Task AppliquerAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_FileAttente_DateArrivee ON FileAttente(DateArrivee)");
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Paiements_DatePaiement ON Paiements(DatePaiement)");
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Alertes_Statut ON Alertes(Statut)");
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_JournalMails_DateEnvoi ON JournalMails(DateEnvoi)");
    }
}
