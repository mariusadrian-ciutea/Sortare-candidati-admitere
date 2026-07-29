IF OBJECT_ID('ImporturiWeb', 'U') IS NOT NULL DROP TABLE ImporturiWeb;
IF OBJECT_ID('AdmitereFinala', 'U') IS NOT NULL DROP TABLE AdmitereFinala;
IF OBJECT_ID('OptiuniCandidat', 'U') IS NOT NULL DROP TABLE OptiuniCandidat;
IF OBJECT_ID('Candidati', 'U') IS NOT NULL DROP TABLE Candidati;
IF OBJECT_ID('Specializari', 'U') IS NOT NULL DROP TABLE Specializari;
IF OBJECT_ID('Facultati', 'U') IS NOT NULL DROP TABLE Facultati;

CREATE TABLE Facultati (
    IdFacultate INT IDENTITY(1,1) PRIMARY KEY,
    NumeFacultate NVARCHAR(100) COLLATE Romanian_CI_AS NOT NULL,
    Abreviere NVARCHAR(10) COLLATE Romanian_CI_AS NOT NULL
);

CREATE TABLE Specializari (
    IdSpecializare INT IDENTITY(1,1) PRIMARY KEY,
    NumeSpecializare NVARCHAR(100) COLLATE Romanian_CI_AS NOT NULL,
    NrLocuri INT NOT NULL CHECK (NrLocuri > 0),
    IdFacultate INT NOT NULL,
    CONSTRAINT FK_Specializari_Facultati
        FOREIGN KEY (IdFacultate) REFERENCES Facultati(IdFacultate)
);

CREATE TABLE Candidati (
    IdCandidat INT IDENTITY(1,1) PRIMARY KEY,
    Nume NVARCHAR(50) COLLATE Romanian_CI_AS NOT NULL,
    Prenume NVARCHAR(50) COLLATE Romanian_CI_AS NOT NULL,
    Adresa NVARCHAR(100) COLLATE Romanian_CI_AS NULL,
    Varsta INT CHECK (Varsta > 0),
    Sex NVARCHAR(10) COLLATE Romanian_CI_AS
        CHECK (Sex IN ('Feminin', 'Masculin')),
    CNP CHAR(13) NOT NULL UNIQUE,
    MedieBAC FLOAT CHECK (MedieBAC BETWEEN 1 AND 10),
    MedieLiceu FLOAT CHECK (MedieLiceu BETWEEN 1 AND 10),
    Status NVARCHAR(20) COLLATE Romanian_CI_AS
        CONSTRAINT DF_Candidati_Status DEFAULT 'Nedefinit'
        CHECK (Status IN ('Nedefinit', 'Respins', 'Admis'))
);

CREATE TABLE OptiuniCandidat (
    IdOptiune INT IDENTITY(1,1) PRIMARY KEY,
    IdCandidat INT NOT NULL,
    IdSpecializare1 INT NOT NULL,
    IdSpecializare2 INT NULL,
    IdSpecializare3 INT NULL,
    CONSTRAINT FK_Optiuni_Candidati
        FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
    CONSTRAINT FK_Optiuni_Specializare1
        FOREIGN KEY (IdSpecializare1) REFERENCES Specializari(IdSpecializare),
    CONSTRAINT FK_Optiuni_Specializare2
        FOREIGN KEY (IdSpecializare2) REFERENCES Specializari(IdSpecializare),
    CONSTRAINT FK_Optiuni_Specializare3
        FOREIGN KEY (IdSpecializare3) REFERENCES Specializari(IdSpecializare)
);

CREATE TABLE AdmitereFinala (
    IdAdmitere INT IDENTITY(1,1) PRIMARY KEY,
    IdCandidat INT NOT NULL,
    IdSpecializare INT NOT NULL,
    CONSTRAINT FK_AdmitereFinala_Candidati
        FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
    CONSTRAINT FK_AdmitereFinala_Specializari
        FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare)
);

CREATE TABLE ImporturiWeb (
    IdImport INT IDENTITY(1,1) PRIMARY KEY,
    ExternalId INT NOT NULL UNIQUE,
    CodInscriere NVARCHAR(40) NOT NULL,
    IdCandidat INT NOT NULL,
    ImportatLa DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ImporturiWeb_Candidati
        FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat)
);

INSERT INTO Facultati (NumeFacultate, Abreviere)
VALUES
    (N'Facultatea de Cibernetică, Statistică și Informatică Economică', N'CSIE'),
    (N'Facultatea de Management', N'MAN'),
    (N'Facultatea de Contabilitate și Informatică de Gestiune', N'CIG'),
    (N'Facultatea de Marketing', N'MK'),
    (N'Facultatea de Finanțe, Asigurări, Bănci și Burse de Valori', N'FABBV'),
    (N'Facultatea de Relații Economice Internaționale', N'REI'),
    (N'Facultatea de Economie Teoretică și Aplicată', N'ETA');

INSERT INTO Specializari (NumeSpecializare, NrLocuri, IdFacultate)
VALUES
    (N'Cibernetică Economică', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'CSIE')),
    (N'Informatică Economică', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'CSIE')),
    (N'Statică economică și data science', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'CSIE')),
    (N'Management', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'MAN')),
    (N'Management (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'MAN')),
    (N'Contabilitate și Informatică de Gestiune', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'CIG')),
    (N'Contabilitate și Informatică de Gestiune (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'CIG')),
    (N'Marketing', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'MK')),
    (N'Marketing (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'MK')),
    (N'Finanțe și Bănci', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'FABBV')),
    (N'Finanțe și Bănci (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'FABBV')),
    (N'Economie și afaceri internaționale', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'REI')),
    (N'Economie și afaceri internaționale (în limba engleză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'REI')),
    (N'Limbi moderne aplicate (engleză, franceză)', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'REI')),
    (N'Economie și comunicare economică în afaceri', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'ETA'));
