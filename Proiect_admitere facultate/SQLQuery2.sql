DROP TABLE IF EXISTS ImporturiWeb;
DROP TABLE IF EXISTS AdmitereFinala;
DROP TABLE IF EXISTS OptiuniCandidat;
DROP TABLE IF EXISTS Candidati;
DROP TABLE IF EXISTS Specializari;
DROP TABLE IF EXISTS Facultati;

CREATE TABLE Facultati (
    IdFacultate INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeFacultate TEXT NOT NULL,
    Abreviere TEXT NOT NULL UNIQUE
);

CREATE TABLE Specializari (
    IdSpecializare INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeSpecializare TEXT NOT NULL UNIQUE,
    NrLocuri INTEGER NOT NULL CHECK (NrLocuri > 0),
    IdFacultate INTEGER NOT NULL,
    FOREIGN KEY (IdFacultate) REFERENCES Facultati(IdFacultate)
);

CREATE TABLE Candidati (
    IdCandidat INTEGER PRIMARY KEY AUTOINCREMENT,
    Nume TEXT NOT NULL,
    Prenume TEXT NOT NULL,
    Adresa TEXT NULL,
    Varsta INTEGER CHECK (Varsta > 0),
    Sex TEXT CHECK (Sex IN ('Feminin', 'Masculin')),
    CNP TEXT NOT NULL UNIQUE,
    MedieBAC REAL CHECK (MedieBAC BETWEEN 1 AND 10),
    MedieLiceu REAL CHECK (MedieLiceu BETWEEN 1 AND 10),
    Status TEXT NOT NULL DEFAULT 'Nedefinit'
        CHECK (Status IN ('Nedefinit', 'Respins', 'Admis'))
);

CREATE TABLE OptiuniCandidat (
    IdOptiune INTEGER PRIMARY KEY AUTOINCREMENT,
    IdCandidat INTEGER NOT NULL,
    IdSpecializare1 INTEGER NOT NULL,
    IdSpecializare2 INTEGER NULL,
    IdSpecializare3 INTEGER NULL,
    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
    FOREIGN KEY (IdSpecializare1) REFERENCES Specializari(IdSpecializare),
    FOREIGN KEY (IdSpecializare2) REFERENCES Specializari(IdSpecializare),
    FOREIGN KEY (IdSpecializare3) REFERENCES Specializari(IdSpecializare)
);

CREATE TABLE AdmitereFinala (
    IdAdmitere INTEGER PRIMARY KEY AUTOINCREMENT,
    IdCandidat INTEGER NOT NULL,
    IdSpecializare INTEGER NOT NULL,
    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
    FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare)
);

CREATE TABLE ImporturiWeb (
    IdImport INTEGER PRIMARY KEY AUTOINCREMENT,
    ExternalId INTEGER NOT NULL UNIQUE,
    CodInscriere TEXT NOT NULL,
    IdCandidat INTEGER NOT NULL,
    ImportatLa TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat)
);

INSERT INTO Facultati (NumeFacultate, Abreviere)
VALUES
    ('Facultatea de Cibernetică, Statistică și Informatică Economică', 'CSIE'),
    ('Facultatea de Management', 'MAN'),
    ('Facultatea de Contabilitate și Informatică de Gestiune', 'CIG'),
    ('Facultatea de Marketing', 'MK'),
    ('Facultatea de Finanțe, Asigurări, Bănci și Burse de Valori', 'FABBV'),
    ('Facultatea de Relații Economice Internaționale', 'REI'),
    ('Facultatea de Economie Teoretică și Aplicată', 'ETA');

INSERT INTO Specializari (NumeSpecializare, NrLocuri, IdFacultate)
VALUES
    ('Cibernetică Economică', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Informatică Economică', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Statică economică și data science', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Management', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MAN')),
    ('Management (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MAN')),
    ('Contabilitate și Informatică de Gestiune', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CIG')),
    ('Contabilitate și Informatică de Gestiune (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CIG')),
    ('Marketing', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MK')),
    ('Marketing (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MK')),
    ('Finanțe și Bănci', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'FABBV')),
    ('Finanțe și Bănci (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'FABBV')),
    ('Economie și afaceri internaționale', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Economie și afaceri internaționale (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Limbi moderne aplicate (engleză, franceză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Economie și comunicare economică în afaceri', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'ETA'));
