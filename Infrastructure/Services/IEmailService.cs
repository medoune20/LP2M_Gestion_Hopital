namespace GestionHopital.Infrastructure.Services;

public interface IEmailService
{
    Task<bool> EnvoyerAsync(string destinataire, string sujet, string corpsHtml, int utilisateurId = 0);
}
