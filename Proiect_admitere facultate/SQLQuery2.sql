DROP TABLE IF EXISTS ImporturiWeb;
DROP TABLE IF EXISTS AdmitereFinala;
DROP TABLE IF EXISTS OptiuniCandidat;
DROP TABLE IF EXISTS Candidati;
DROP TABLE IF EXISTS AsocieriEsantioane;
DROP TABLE IF EXISTS LocuriEsantionOptiuni;
DROP TABLE IF EXISTS Specializari;
DROP TABLE IF EXISTS Facultati;
DROP TABLE IF EXISTS EsantioaneOptiuni;
DROP TABLE IF EXISTS Esantioane;

CREATE TABLE Esantioane (
    IdEsantion INTEGER PRIMARY KEY AUTOINCREMENT,
    Nume TEXT NOT NULL UNIQUE,
    CreatLa TEXT NOT NULL DEFAULT (datetime('now')),
    EsteImplicit INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE EsantioaneOptiuni (
    IdEsantionOptiuni INTEGER PRIMARY KEY AUTOINCREMENT,
    Nume TEXT NOT NULL UNIQUE,
    CreatLa TEXT NOT NULL DEFAULT (datetime('now')),
    EsteImplicit INTEGER NOT NULL DEFAULT 0
);

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

CREATE TABLE LocuriEsantionOptiuni (
    IdEsantionOptiuni INTEGER NOT NULL,
    IdSpecializare INTEGER NOT NULL,
    NrLocuri INTEGER NOT NULL CHECK (NrLocuri > 0),
    NumeFacultateCustom TEXT NULL,
    CodFacultateCustom TEXT NULL,
    NumeSpecializareCustom TEXT NULL,
    PRIMARY KEY (IdEsantionOptiuni, IdSpecializare),
    FOREIGN KEY (IdEsantionOptiuni) REFERENCES EsantioaneOptiuni(IdEsantionOptiuni),
    FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare)
);

CREATE TABLE AsocieriEsantioane (
    IdEsantion INTEGER NOT NULL,
    IdEsantionOptiuni INTEGER NOT NULL,
    EsteImplicit INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (IdEsantion, IdEsantionOptiuni),
    FOREIGN KEY (IdEsantion) REFERENCES Esantioane(IdEsantion),
    FOREIGN KEY (IdEsantionOptiuni) REFERENCES EsantioaneOptiuni(IdEsantionOptiuni)
);

CREATE TABLE Candidati (
    IdCandidat INTEGER PRIMARY KEY AUTOINCREMENT,
    IdEsantion INTEGER NOT NULL DEFAULT 1,
    Nume TEXT NOT NULL,
    Prenume TEXT NOT NULL,
    Adresa TEXT NULL,
    Varsta INTEGER CHECK (Varsta > 0),
    Sex TEXT CHECK (Sex IN ('Feminin', 'Masculin')),
    CNP TEXT NOT NULL,
    MedieBAC REAL CHECK (MedieBAC BETWEEN 1 AND 10),
    MedieLiceu REAL CHECK (MedieLiceu BETWEEN 1 AND 10),
    Status TEXT NOT NULL DEFAULT 'Nedefinit'
        CHECK (Status IN ('Nedefinit', 'Respins', 'Admis')),
    FOREIGN KEY (IdEsantion) REFERENCES Esantioane(IdEsantion),
    UNIQUE (IdEsantion, CNP)
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
    IdEsantion INTEGER NOT NULL DEFAULT 1,
    IdEsantionOptiuni INTEGER NOT NULL DEFAULT 1,
    Algoritm TEXT NOT NULL DEFAULT 'weighted',
    IdCandidat INTEGER NOT NULL,
    IdSpecializare INTEGER NOT NULL,
    FOREIGN KEY (IdEsantion) REFERENCES Esantioane(IdEsantion),
    FOREIGN KEY (IdEsantionOptiuni) REFERENCES EsantioaneOptiuni(IdEsantionOptiuni),
    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
    FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare),
    UNIQUE (IdEsantion, Algoritm, IdCandidat)
);

CREATE TABLE ImporturiWeb (
    IdImport INTEGER PRIMARY KEY AUTOINCREMENT,
    ExternalId INTEGER NOT NULL UNIQUE,
    CodInscriere TEXT NOT NULL,
    IdCandidat INTEGER NOT NULL,
    CreatLaFormular TEXT NULL,
    ImportatLa TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat)
);

INSERT INTO Esantioane (IdEsantion, Nume, EsteImplicit)
VALUES (1, 'Esantion principal', 1);

INSERT INTO EsantioaneOptiuni (IdEsantionOptiuni, Nume, EsteImplicit)
VALUES (1, 'ASE 2023', 1);

INSERT INTO Facultati (NumeFacultate, Abreviere)
VALUES
    ('Facultatea de Cibernetica, Statistica si Informatica Economica', 'CSIE'),
    ('Facultatea de Management', 'MAN'),
    ('Facultatea de Contabilitate si Informatica de Gestiune', 'CIG'),
    ('Facultatea de Marketing', 'MK'),
    ('Facultatea de Finante, Asigurari, Banci si Burse de Valori', 'FABBV'),
    ('Facultatea de Relatii Economice Internationale', 'REI'),
    ('Facultatea de Economie Teoretica si Aplicata', 'ETA');

INSERT INTO Specializari (NumeSpecializare, NrLocuri, IdFacultate)
VALUES
    ('Cibernetica Economica', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Informatica Economica', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Statistica economica si data science', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CSIE')),
    ('Management', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MAN')),
    ('Management in English', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MAN')),
    ('Contabilitate si Informatica de Gestiune', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CIG')),
    ('Contabilitate si Informatica de Gestiune in English', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'CIG')),
    ('Marketing', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MK')),
    ('Marketing in English', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'MK')),
    ('Finante si Banci', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'FABBV')),
    ('Finante si Banci in English', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'FABBV')),
    ('Economie si afaceri internationale', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Economie si afaceri internationale in English', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Limbi moderne aplicate', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'REI')),
    ('Economie si comunicare economica in afaceri', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'ETA'));

INSERT INTO AsocieriEsantioane (IdEsantion, IdEsantionOptiuni, EsteImplicit)
VALUES (1, 1, 1);

INSERT INTO LocuriEsantionOptiuni
    (IdEsantionOptiuni, IdSpecializare, NrLocuri)
SELECT 1, IdSpecializare, NrLocuri
FROM Specializari;
