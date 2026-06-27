using Microsoft.EntityFrameworkCore;

namespace GestionHopital.Infrastructure.Donnees;

public static class Lp2mSanteSchema
{
    public static async Task AppliquerAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS ProfilsSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nom TEXT NOT NULL,
    Code TEXT NOT NULL,
    RoleBase INTEGER NOT NULL DEFAULT 4,
    Permissions TEXT NOT NULL DEFAULT 'dashboard,patients,rendezvous',
    Actif INTEGER NOT NULL DEFAULT 1,
    DateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS FileAttenteSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId INTEGER NOT NULL,
    MedecinId INTEGER NULL,
    Numero TEXT NOT NULL,
    Service TEXT NOT NULL DEFAULT 'Accueil',
    Motif TEXT NOT NULL DEFAULT '',
    Priorite INTEGER NOT NULL DEFAULT 3,
    Statut TEXT NOT NULL DEFAULT 'En attente',
    DateArrivee TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateAppel TEXT NULL,
    DateCloture TEXT NULL
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS TeleconsultationsSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId INTEGER NOT NULL,
    MedecinId INTEGER NULL,
    DateHeure TEXT NOT NULL,
    Motif TEXT NOT NULL DEFAULT '',
    LienVideo TEXT NOT NULL DEFAULT '',
    Statut TEXT NOT NULL DEFAULT 'Planifiée',
    EmailPatient TEXT NOT NULL DEFAULT '',
    TelephonePatient TEXT NOT NULL DEFAULT '',
    NotificationEnvoyee INTEGER NOT NULL DEFAULT 0
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS PaiementsSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId INTEGER NULL,
    Reference TEXT NOT NULL,
    Libelle TEXT NOT NULL DEFAULT 'Consultation',
    Montant REAL NOT NULL DEFAULT 0,
    ModePaiement TEXT NOT NULL DEFAULT 'Espèces',
    ReferenceOperateur TEXT NOT NULL DEFAULT '',
    Statut TEXT NOT NULL DEFAULT 'Validé',
    DatePaiement TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Caissier TEXT NOT NULL DEFAULT ''
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS EcrituresComptablesSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DateEcriture TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Type TEXT NOT NULL DEFAULT 'Recette',
    Categorie TEXT NOT NULL DEFAULT 'Consultations',
    Libelle TEXT NOT NULL DEFAULT '',
    Montant REAL NOT NULL DEFAULT 0,
    Reference TEXT NOT NULL DEFAULT ''
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS AlertesSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Niveau TEXT NOT NULL DEFAULT 'Info',
    Module TEXT NOT NULL DEFAULT 'Système',
    Message TEXT NOT NULL DEFAULT '',
    Traitee INTEGER NOT NULL DEFAULT 0,
    DateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DateTraitement TEXT NULL
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS AccesPatientsSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId INTEGER NOT NULL,
    Identifiant TEXT NOT NULL,
    MotDePasseHash TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Telephone TEXT NOT NULL DEFAULT '',
    Actif INTEGER NOT NULL DEFAULT 1,
    DateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS EmailsJournalSante (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Destinataire TEXT NOT NULL,
    Sujet TEXT NOT NULL,
    Corps TEXT NOT NULL DEFAULT '',
    Envoye INTEGER NOT NULL DEFAULT 0,
    Erreur TEXT NOT NULL DEFAULT '',
    DateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
""");

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO ProfilsSante (Nom, Code, RoleBase, Permissions)
SELECT 'Super administrateur', 'superadmin', 0, 'all'
WHERE NOT EXISTS (SELECT 1 FROM ProfilsSante WHERE Code = 'superadmin');
""");

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO ProfilsSante (Nom, Code, RoleBase, Permissions)
SELECT 'Administrateur établissement', 'admin', 1, 'dashboard,admin,patients,rendezvous,consultations,file_attente,paiements,comptabilite,alertes'
WHERE NOT EXISTS (SELECT 1 FROM ProfilsSante WHERE Code = 'admin');
""");

        await db.Database.ExecuteSqlRawAsync("""
INSERT INTO ProfilsSante (Nom, Code, RoleBase, Permissions)
SELECT 'Médecin', 'medecin', 2, 'dashboard,patients,consultations,teleconsultation,examens'
WHERE NOT EXISTS (SELECT 1 FROM ProfilsSante WHERE Code = 'medecin');
""");
    }
}
