using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Proiect_admitere_facultate
{
    internal enum ImportResult
    {
        Imported,
        AlreadyPresent
    }

    internal static class DatabaseManager
    {
        private const string DatabaseFileName = "Admitere_database.sqlite";
        private const string ApplicationDataFolderName = "Sortare candidati admitere";
        private static readonly object DatabaseLock = new object();
        private static string resolvedDatabasePath;
        private static bool databaseReady;

        public static string DatabasePath
        {
            get { return ResolveDatabasePath(); }
        }

        public static string connectionString
        {
            get { return BuildConnectionString(DatabasePath); }
        }

        public static IDbDataParameter CreateParameter(string name, object value)
        {
            return new SQLiteParameter(name, value ?? DBNull.Value);
        }

        private static string ResolveDatabasePath()
        {
            lock (DatabaseLock)
            {
                if (!string.IsNullOrEmpty(resolvedDatabasePath) && databaseReady)
                    return resolvedDatabasePath;

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDatabase = Path.Combine(baseDirectory, DatabaseFileName);
                if (File.Exists(outputDatabase))
                    return SetReadyDatabase(outputDatabase);

                string projectDatabase = Path.GetFullPath(
                    Path.Combine(baseDirectory, "..", "..", DatabaseFileName));
                if (File.Exists(projectDatabase))
                    return SetReadyDatabase(projectDatabase);

                string writableDatabase = Path.Combine(
                    ResolveWritableDatabaseDirectory(), DatabaseFileName);
                return SetReadyDatabase(writableDatabase);
            }
        }

        private static string SetReadyDatabase(string databasePath)
        {
            resolvedDatabasePath = databasePath;
            EnsureDatabaseReady(databasePath);
            databaseReady = true;
            return resolvedDatabasePath;
        }

        private static string BuildConnectionString(string databasePath)
        {
            SQLiteConnectionStringBuilder builder =
                new SQLiteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Version = 3,
                    ForeignKeys = true,
                    Pooling = true
                };
            return builder.ToString();
        }

        private static string ResolveWritableDatabaseDirectory()
        {
            string configuredDirectory = ConfigurationManager.AppSettings["DatabaseDirectory"];
            if (string.IsNullOrWhiteSpace(configuredDirectory))
                configuredDirectory = Environment.GetEnvironmentVariable("ADMITERE_DATABASE_DIR");

            if (!string.IsNullOrWhiteSpace(configuredDirectory))
            {
                configuredDirectory = Environment.ExpandEnvironmentVariables(configuredDirectory);
                if (!Path.IsPathRooted(configuredDirectory))
                    configuredDirectory = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, configuredDirectory);

                return Path.GetFullPath(configuredDirectory);
            }

            string localAppData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localAppData))
                localAppData = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(localAppData, ApplicationDataFolderName);
        }

        private static SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            EnablePragmas(connection);
            return connection;
        }

        private static void EnablePragmas(SQLiteConnection connection)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureDatabaseReady(string databasePath)
        {
            string databaseDirectory = Path.GetDirectoryName(databasePath);
            if (string.IsNullOrWhiteSpace(databaseDirectory))
                throw new InvalidOperationException(
                    "Nu se poate determina folderul pentru baza de date.");

            Directory.CreateDirectory(databaseDirectory);

            using (SQLiteConnection connection =
                   new SQLiteConnection(BuildConnectionString(databasePath)))
            using (SQLiteCommand command = connection.CreateCommand())
            {
                connection.Open();
                EnablePragmas(connection);
                command.CommandText = InitialSchemaSql;
                command.ExecuteNonQuery();
            }

            RunMigrations(databasePath);
        }

        private static void RunMigrations(string databasePath)
        {
            using (SQLiteConnection connection =
                   new SQLiteConnection(BuildConnectionString(databasePath)))
            {
                connection.Open();
                EnablePragmas(connection);

                ExecuteNonQuery(connection,
                    "INSERT OR IGNORE INTO Esantioane (IdEsantion, Nume, EsteImplicit) VALUES (1, 'Eșantion principal', 1)");

                ExecuteNonQuery(connection,
                    "INSERT OR IGNORE INTO EsantioaneOptiuni (IdEsantionOptiuni, Nume, EsteImplicit) VALUES (1, 'ASE 2023', 1)");
                ExecuteNonQuery(connection,
                    "UPDATE EsantioaneOptiuni SET Nume = 'ASE 2023' WHERE IdEsantionOptiuni = 1 AND Nume = 'Optiuni principale' AND NOT EXISTS (SELECT 1 FROM EsantioaneOptiuni WHERE Nume = 'ASE 2023' AND IdEsantionOptiuni <> 1)");
                ExecuteNonQuery(connection,
                    "INSERT OR IGNORE INTO AsocieriEsantioane (IdEsantion, IdEsantionOptiuni, EsteImplicit) SELECT IdEsantion, 1, CASE WHEN EsteImplicit = 1 THEN 1 ELSE 0 END FROM Esantioane");
                ExecuteNonQuery(connection,
                    "UPDATE AsocieriEsantioane SET EsteImplicit = 1 WHERE IdEsantionOptiuni = 1 AND NOT EXISTS (SELECT 1 FROM AsocieriEsantioane A2 WHERE A2.IdEsantion = AsocieriEsantioane.IdEsantion AND A2.EsteImplicit = 1)");
                ExecuteNonQuery(connection,
                    "INSERT OR IGNORE INTO LocuriEsantionOptiuni (IdEsantionOptiuni, IdSpecializare, NrLocuri) SELECT 1, IdSpecializare, NrLocuri FROM Specializari");

                EnsureColumn(connection, "Candidati", "IdEsantion",
                    "INTEGER NOT NULL DEFAULT 1");
                EnsureColumn(connection, "AdmitereFinala", "IdEsantion",
                    "INTEGER NOT NULL DEFAULT 1");
                EnsureColumn(connection, "AdmitereFinala", "IdEsantionOptiuni",
                    "INTEGER NOT NULL DEFAULT 1");
                EnsureColumn(connection, "AdmitereFinala", "Algoritm",
                    "TEXT NOT NULL DEFAULT 'weighted'");
                EnsureColumn(connection, "ImporturiWeb", "CreatLaFormular",
                    "TEXT NULL");
                EnsureColumn(connection, "LocuriEsantionOptiuni", "NumeFacultateCustom",
                    "TEXT NULL");
                EnsureColumn(connection, "LocuriEsantionOptiuni", "CodFacultateCustom",
                    "TEXT NULL");
                EnsureColumn(connection, "LocuriEsantionOptiuni", "NumeSpecializareCustom",
                    "TEXT NULL");
            }
        }

        private static void EnsureColumn(
            SQLiteConnection connection, string tableName,
            string columnName, string definition)
        {
            using (SQLiteCommand command = new SQLiteCommand(
                "PRAGMA table_info(" + tableName + ")", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader["name"].ToString(), columnName,
                        StringComparison.OrdinalIgnoreCase))
                        return;
                }
            }

            ExecuteNonQuery(connection,
                "ALTER TABLE " + tableName + " ADD COLUMN " +
                columnName + " " + definition);
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string query)
        {
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
                command.ExecuteNonQuery();
        }

        private const string InitialSchemaSql = @"
            CREATE TABLE IF NOT EXISTS Esantioane
            (
                IdEsantion INTEGER PRIMARY KEY AUTOINCREMENT,
                Nume TEXT NOT NULL UNIQUE,
                CreatLa TEXT NOT NULL DEFAULT (datetime('now')),
                EsteImplicit INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS EsantioaneOptiuni
            (
                IdEsantionOptiuni INTEGER PRIMARY KEY AUTOINCREMENT,
                Nume TEXT NOT NULL UNIQUE,
                CreatLa TEXT NOT NULL DEFAULT (datetime('now')),
                EsteImplicit INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Facultati
            (
                IdFacultate INTEGER PRIMARY KEY AUTOINCREMENT,
                NumeFacultate TEXT NOT NULL,
                Abreviere TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Specializari
            (
                IdSpecializare INTEGER PRIMARY KEY AUTOINCREMENT,
                NumeSpecializare TEXT NOT NULL UNIQUE,
                NrLocuri INTEGER NOT NULL CHECK (NrLocuri > 0),
                IdFacultate INTEGER NOT NULL,
                FOREIGN KEY (IdFacultate) REFERENCES Facultati(IdFacultate)
            );

            CREATE TABLE IF NOT EXISTS LocuriEsantionOptiuni
            (
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

            CREATE TABLE IF NOT EXISTS AsocieriEsantioane
            (
                IdEsantion INTEGER NOT NULL,
                IdEsantionOptiuni INTEGER NOT NULL,
                EsteImplicit INTEGER NOT NULL DEFAULT 0,
                PRIMARY KEY (IdEsantion, IdEsantionOptiuni),
                FOREIGN KEY (IdEsantion) REFERENCES Esantioane(IdEsantion),
                FOREIGN KEY (IdEsantionOptiuni) REFERENCES EsantioaneOptiuni(IdEsantionOptiuni)
            );

            CREATE TABLE IF NOT EXISTS Candidati
            (
                IdCandidat INTEGER PRIMARY KEY AUTOINCREMENT,
                IdEsantion INTEGER NOT NULL DEFAULT 1,
                Nume TEXT NOT NULL,
                Prenume TEXT NOT NULL,
                Adresa TEXT NULL,
                Varsta INTEGER CHECK (Varsta > 0),
                Sex TEXT CHECK (Sex IN ('Feminin', 'Masculin')),
                CNP TEXT NOT NULL UNIQUE,
                MedieBAC REAL CHECK (MedieBAC BETWEEN 1 AND 10),
                MedieLiceu REAL CHECK (MedieLiceu BETWEEN 1 AND 10),
                Status TEXT NOT NULL DEFAULT 'Nedefinit'
                    CHECK (Status IN ('Nedefinit', 'Respins', 'Admis')),
                FOREIGN KEY (IdEsantion) REFERENCES Esantioane(IdEsantion)
            );

            CREATE TABLE IF NOT EXISTS OptiuniCandidat
            (
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

            CREATE TABLE IF NOT EXISTS AdmitereFinala
            (
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

            CREATE TABLE IF NOT EXISTS ImporturiWeb
            (
                IdImport INTEGER PRIMARY KEY AUTOINCREMENT,
                ExternalId INTEGER NOT NULL UNIQUE,
                CodInscriere TEXT NOT NULL,
                IdCandidat INTEGER NOT NULL,
                CreatLaFormular TEXT NULL,
                ImportatLa TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat)
            );

            INSERT OR IGNORE INTO Esantioane (IdEsantion, Nume, EsteImplicit)
            VALUES (1, 'Eșantion principal', 1);

            INSERT OR IGNORE INTO EsantioaneOptiuni (IdEsantionOptiuni, Nume, EsteImplicit)
            VALUES (1, 'ASE 2023', 1);

            INSERT OR IGNORE INTO Facultati (NumeFacultate, Abreviere)
            VALUES
                ('Facultatea de Cibernetică, Statistică și Informatică Economică', 'CSIE'),
                ('Facultatea de Management', 'MAN'),
                ('Facultatea de Contabilitate și Informatică de Gestiune', 'CIG'),
                ('Facultatea de Marketing', 'MK'),
                ('Facultatea de Finanțe, Asigurări, Bănci și Burse de Valori', 'FABBV'),
                ('Facultatea de Relații Economice Internaționale', 'REI'),
                ('Facultatea de Economie Teoretică și Aplicată', 'ETA');

            INSERT OR IGNORE INTO Specializari (NumeSpecializare, NrLocuri, IdFacultate)
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
                ('Economie și comunicare economică în afaceri', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = 'ETA'));";

        public static void ValidateDatabase()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name IN ('Candidati', 'Facultati', 'Specializari',
                               'OptiuniCandidat', 'AdmitereFinala',
                               'ImporturiWeb', 'Esantioane',
                               'EsantioaneOptiuni', 'AsocieriEsantioane',
                               'LocuriEsantionOptiuni')";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                int tableCount = Convert.ToInt32(command.ExecuteScalar());
                if (tableCount != 10)
                    throw new InvalidOperationException(
                        "Structura bazei de date este incompletă. Sunt necesare 7 tabele.");
            }
        }

        public static DataTable ExecuteQuery(
            string query, params IDbDataParameter[] parameters)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
            {
                AddParameters(command, parameters);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static int InsertUpdateOrDelete(
            string query, params IDbDataParameter[] parameters)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                AddParameters(command, parameters);
                return command.ExecuteNonQuery();
            }
        }

        public static DataTable GetSamples()
        {
            return ExecuteQuery(@"
                SELECT IdEsantion, Nume
                FROM Esantioane
                ORDER BY EsteImplicit DESC, Nume ASC");
        }

        public static int GetDefaultSampleId()
        {
            object result = ExecuteQuery(@"
                SELECT IdEsantion
                FROM Esantioane
                ORDER BY EsteImplicit DESC, IdEsantion ASC
                LIMIT 1").Rows[0]["IdEsantion"];
            return Convert.ToInt32(result);
        }

        public static int CreateSample(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Numele eșantionului este obligatoriu.", "name");

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO Esantioane (Nume, EsteImplicit)
                        VALUES (@Nume, 0)",
                        connection, transaction))
                    {
                        AddParameter(command, "@Nume", name.Trim());
                        command.ExecuteNonQuery();
                    }

                    int sampleId = Convert.ToInt32(connection.LastInsertRowId);
                    AssociateOptionSampleInternal(
                        connection, transaction, sampleId, 1, true);
                    transaction.Commit();
                    return sampleId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static DataTable GetOptionSamples()
        {
            return ExecuteQuery(@"
                SELECT
                    O.IdEsantionOptiuni,
                    O.Nume,
                    COUNT(DISTINCT A.IdEsantion) AS EsantioaneStudenti,
                    COUNT(DISTINCT L.IdSpecializare) AS Specializari
                FROM EsantioaneOptiuni O
                LEFT JOIN AsocieriEsantioane A
                    ON A.IdEsantionOptiuni = O.IdEsantionOptiuni
                LEFT JOIN LocuriEsantionOptiuni L
                    ON L.IdEsantionOptiuni = O.IdEsantionOptiuni
                GROUP BY O.IdEsantionOptiuni, O.Nume, O.EsteImplicit
                ORDER BY O.EsteImplicit DESC, O.Nume ASC");
        }

        public static DataTable GetAssociatedOptionSamples(int sampleId)
        {
            EnsureSampleExists(sampleId);
            DataTable data = ExecuteQuery(@"
                SELECT O.IdEsantionOptiuni, O.Nume
                FROM EsantioaneOptiuni O
                INNER JOIN AsocieriEsantioane A
                    ON A.IdEsantionOptiuni = O.IdEsantionOptiuni
                WHERE A.IdEsantion = @IdEsantion
                ORDER BY A.EsteImplicit DESC, O.EsteImplicit DESC, O.Nume ASC",
                CreateParameter("@IdEsantion", sampleId));

            if (data.Rows.Count == 0)
            {
                AssociateOptionSample(sampleId, 1);
                data = ExecuteQuery(@"
                    SELECT O.IdEsantionOptiuni, O.Nume
                    FROM EsantioaneOptiuni O
                    INNER JOIN AsocieriEsantioane A
                        ON A.IdEsantionOptiuni = O.IdEsantionOptiuni
                    WHERE A.IdEsantion = @IdEsantion
                    ORDER BY A.EsteImplicit DESC, O.EsteImplicit DESC, O.Nume ASC",
                    CreateParameter("@IdEsantion", sampleId));
            }

            return data;
        }

        public static int GetDefaultOptionSampleId(int sampleId)
        {
            DataTable data = GetAssociatedOptionSamples(sampleId);
            return data.Rows.Count == 0
                ? 1
                : Convert.ToInt32(data.Rows[0]["IdEsantionOptiuni"]);
        }

        public static int CreateOptionSample(string name, int studentSampleId)
        {
            return CreateOptionSampleFromTemplate(
                name, GetDefaultOptionSampleId(studentSampleId), studentSampleId);
        }

        public static int CreateEmptyOptionSample(string name, int studentSampleId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Numele esantionului de optiuni este obligatoriu.", "name");

            EnsureSampleExists(studentSampleId);

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO EsantioaneOptiuni (Nume, EsteImplicit)
                        VALUES (@Nume, 0)",
                        connection, transaction))
                    {
                        AddParameter(command, "@Nume", name.Trim());
                        command.ExecuteNonQuery();
                    }

                    int optionSampleId = Convert.ToInt32(connection.LastInsertRowId);
                    AssociateOptionSampleInternal(
                        connection, transaction, studentSampleId, optionSampleId, false);
                    transaction.Commit();
                    return optionSampleId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static int CreateOptionSampleFromTemplate(
            string name, int templateOptionSampleId, int studentSampleId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Numele esantionului de optiuni este obligatoriu.", "name");

            EnsureSampleExists(studentSampleId);

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(
                        connection, transaction, templateOptionSampleId);
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO EsantioaneOptiuni (Nume, EsteImplicit)
                        VALUES (@Nume, 0)",
                        connection, transaction))
                    {
                        AddParameter(command, "@Nume", name.Trim());
                        command.ExecuteNonQuery();
                    }

                    int optionSampleId = Convert.ToInt32(connection.LastInsertRowId);
                    CopyOptionSeatsInternal(
                        connection, transaction, templateOptionSampleId, optionSampleId);
                    AssociateOptionSampleInternal(
                        connection, transaction, studentSampleId, optionSampleId, false);
                    transaction.Commit();
                    return optionSampleId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void AssociateOptionSample(int studentSampleId, int optionSampleId)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureSampleExists(connection, transaction, studentSampleId);
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    AssociateOptionSampleInternal(
                        connection, transaction, studentSampleId, optionSampleId, false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void SetDefaultOptionSampleForStudent(
            int studentSampleId, int optionSampleId)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureSampleExists(connection, transaction, studentSampleId);
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    using (SQLiteCommand reset = new SQLiteCommand(@"
                        UPDATE AsocieriEsantioane
                        SET EsteImplicit = 0
                        WHERE IdEsantion = @IdEsantion",
                        connection, transaction))
                    {
                        AddParameter(reset, "@IdEsantion", studentSampleId);
                        reset.ExecuteNonQuery();
                    }

                    using (SQLiteCommand insert = new SQLiteCommand(@"
                        INSERT OR IGNORE INTO AsocieriEsantioane
                            (IdEsantion, IdEsantionOptiuni, EsteImplicit)
                        VALUES
                            (@IdEsantion, @IdEsantionOptiuni, 1)",
                        connection, transaction))
                    {
                        AddParameter(insert, "@IdEsantion", studentSampleId);
                        AddParameter(insert, "@IdEsantionOptiuni", optionSampleId);
                        insert.ExecuteNonQuery();
                    }

                    using (SQLiteCommand update = new SQLiteCommand(@"
                        UPDATE AsocieriEsantioane
                        SET EsteImplicit = 1
                        WHERE IdEsantion = @IdEsantion
                          AND IdEsantionOptiuni = @IdEsantionOptiuni",
                        connection, transaction))
                    {
                        AddParameter(update, "@IdEsantion", studentSampleId);
                        AddParameter(update, "@IdEsantionOptiuni", optionSampleId);
                        update.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static DataTable GetSampleAssociations()
        {
            return ExecuteQuery(@"
                SELECT
                    E.IdEsantion AS [ID studenti],
                    E.Nume AS [Esantion studenti],
                    O.IdEsantionOptiuni AS [ID optiuni],
                    O.Nume AS [Esantion optiuni],
                    CASE WHEN A.EsteImplicit = 1 THEN 'Da' ELSE 'Nu' END AS [Implicit]
                FROM AsocieriEsantioane A
                INNER JOIN Esantioane E ON A.IdEsantion = E.IdEsantion
                INNER JOIN EsantioaneOptiuni O
                    ON A.IdEsantionOptiuni = O.IdEsantionOptiuni
                ORDER BY E.Nume ASC, A.EsteImplicit DESC, O.Nume ASC");
        }

        public static DataTable GetOptionCatalog(int optionSampleId)
        {
            EnsureOptionSampleExists(optionSampleId);
            return ExecuteQuery(@"
                SELECT
                    F.IdFacultate AS [ID facultate],
                    COALESCE(NULLIF(L.NumeFacultateCustom, ''), F.NumeFacultate)
                        AS [Facultate],
                    COALESCE(NULLIF(L.CodFacultateCustom, ''), F.Abreviere)
                        AS [Cod],
                    S.IdSpecializare AS [ID specializare],
                    COALESCE(NULLIF(L.NumeSpecializareCustom, ''), S.NumeSpecializare)
                        AS [Specializare],
                    L.NrLocuri AS [Locuri]
                FROM LocuriEsantionOptiuni L
                INNER JOIN Specializari S ON L.IdSpecializare = S.IdSpecializare
                INNER JOIN Facultati F ON S.IdFacultate = F.IdFacultate
                WHERE L.IdEsantionOptiuni = @IdEsantionOptiuni
                ORDER BY F.NumeFacultate ASC, S.NumeSpecializare ASC",
                CreateParameter("@IdEsantionOptiuni", optionSampleId));
        }

        public static DataTable GetFaculties()
        {
            return ExecuteQuery(@"
                SELECT IdFacultate, NumeFacultate, Abreviere
                FROM Facultati
                ORDER BY NumeFacultate ASC");
        }

        public static int SaveFaculty(string facultyName, string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(facultyName))
                throw new ArgumentException("Numele facultatii este obligatoriu.", "facultyName");
            if (string.IsNullOrWhiteSpace(abbreviation))
                throw new ArgumentException("Codul facultatii este obligatoriu.", "abbreviation");

            using (SQLiteConnection connection = OpenConnection())
            {
                string code = abbreviation.Trim().ToUpperInvariant();
                using (SQLiteCommand insert = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Facultati (NumeFacultate, Abreviere)
                    VALUES (@NumeFacultate, @Abreviere)",
                    connection))
                {
                    AddParameter(insert, "@NumeFacultate", facultyName.Trim());
                    AddParameter(insert, "@Abreviere", code);
                    insert.ExecuteNonQuery();
                }

                using (SQLiteCommand select = new SQLiteCommand(@"
                    SELECT IdFacultate
                    FROM Facultati
                    WHERE Abreviere = @Abreviere",
                    connection))
                {
                    AddParameter(select, "@Abreviere", code);
                    return Convert.ToInt32(select.ExecuteScalar());
                }
            }
        }

        public static int SaveSpecializationInOptionSample(
            int optionSampleId, int facultyId, string specializationName, int seats)
        {
            if (string.IsNullOrWhiteSpace(specializationName))
                throw new ArgumentException("Numele specializarii este obligatoriu.", "specializationName");
            if (seats <= 0)
                throw new ArgumentException("Numarul de locuri trebuie sa fie pozitiv.", "seats");

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    EnsureFacultyExists(connection, transaction, facultyId);

                    int specializationId = FindSpecializationId(
                        connection, transaction, specializationName.Trim());
                    if (specializationId == 0)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(@"
                            INSERT INTO Specializari
                                (NumeSpecializare, NrLocuri, IdFacultate)
                            VALUES
                                (@NumeSpecializare, @NrLocuri, @IdFacultate)",
                            connection, transaction))
                        {
                            AddParameter(command, "@NumeSpecializare",
                                specializationName.Trim());
                            AddParameter(command, "@NrLocuri", seats);
                            AddParameter(command, "@IdFacultate", facultyId);
                            command.ExecuteNonQuery();
                        }
                        specializationId = Convert.ToInt32(connection.LastInsertRowId);
                    }

                    SetOptionSeatInternal(
                        connection, transaction, optionSampleId, specializationId, seats);
                    transaction.Commit();
                    return specializationId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void UpdateOptionSeat(
            int optionSampleId, int specializationId, int seats)
        {
            if (seats <= 0)
                throw new ArgumentException("Numarul de locuri trebuie sa fie pozitiv.", "seats");

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    EnsureSpecializationExists(connection, transaction, specializationId);
                    SetOptionSeatInternal(
                        connection, transaction, optionSampleId, specializationId, seats);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static int AddSpecializationToOptionSample(
            int optionSampleId, string facultyName, string facultyCode,
            string specializationName, int seats)
        {
            ValidateCatalogInput(facultyName, facultyCode, specializationName, seats);

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    int facultyId = SaveFacultyInternal(
                        connection, transaction, facultyName, facultyCode);
                    int specializationId = EnsureSpecializationInternal(
                        connection, transaction, specializationName, facultyId);

                    using (SQLiteCommand exists = new SQLiteCommand(@"
                        SELECT COUNT(*)
                        FROM LocuriEsantionOptiuni
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni
                          AND IdSpecializare = @IdSpecializare",
                        connection, transaction))
                    {
                        AddParameter(exists, "@IdEsantionOptiuni", optionSampleId);
                        AddParameter(exists, "@IdSpecializare", specializationId);
                        if (Convert.ToInt32(exists.ExecuteScalar()) > 0)
                            throw new InvalidOperationException(
                                "Specializarea exista deja in suita selectata.");
                    }

                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO LocuriEsantionOptiuni
                            (IdEsantionOptiuni, IdSpecializare, NrLocuri,
                             NumeFacultateCustom, CodFacultateCustom,
                             NumeSpecializareCustom)
                        VALUES
                            (@IdEsantionOptiuni, @IdSpecializare, @NrLocuri,
                             @NumeFacultate, @CodFacultate, @NumeSpecializare)",
                        connection, transaction))
                    {
                        AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                        AddParameter(command, "@IdSpecializare", specializationId);
                        AddParameter(command, "@NrLocuri", seats);
                        AddParameter(command, "@NumeFacultate", facultyName.Trim());
                        AddParameter(command, "@CodFacultate",
                            facultyCode.Trim().ToUpperInvariant());
                        AddParameter(command, "@NumeSpecializare",
                            specializationName.Trim());
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return specializationId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void UpdateOptionSampleCatalogItem(
            int optionSampleId, int facultyId, int specializationId,
            string facultyName, string facultyCode, string specializationName,
            int seats)
        {
            ValidateCatalogInput(facultyName, facultyCode, specializationName, seats);

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    EnsureFacultyExists(connection, transaction, facultyId);
                    EnsureSpecializationExists(connection, transaction, specializationId);

                    using (SQLiteCommand updateFacultyRows = new SQLiteCommand(@"
                        UPDATE LocuriEsantionOptiuni
                        SET NumeFacultateCustom = @NumeFacultate,
                            CodFacultateCustom = @CodFacultate
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni
                          AND IdSpecializare IN
                          (
                              SELECT IdSpecializare
                              FROM Specializari
                              WHERE IdFacultate = @IdFacultate
                          )",
                        connection, transaction))
                    {
                        AddParameter(updateFacultyRows, "@NumeFacultate",
                            facultyName.Trim());
                        AddParameter(updateFacultyRows, "@CodFacultate",
                            facultyCode.Trim().ToUpperInvariant());
                        AddParameter(updateFacultyRows, "@IdEsantionOptiuni",
                            optionSampleId);
                        AddParameter(updateFacultyRows, "@IdFacultate", facultyId);
                        updateFacultyRows.ExecuteNonQuery();
                    }

                    using (SQLiteCommand updateSpecialization = new SQLiteCommand(@"
                        UPDATE LocuriEsantionOptiuni
                        SET NumeSpecializareCustom = @NumeSpecializare,
                            NrLocuri = @NrLocuri
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni
                          AND IdSpecializare = @IdSpecializare",
                        connection, transaction))
                    {
                        AddParameter(updateSpecialization, "@NumeSpecializare",
                            specializationName.Trim());
                        AddParameter(updateSpecialization, "@NrLocuri", seats);
                        AddParameter(updateSpecialization, "@IdEsantionOptiuni",
                            optionSampleId);
                        AddParameter(updateSpecialization, "@IdSpecializare",
                            specializationId);
                        if (updateSpecialization.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                "Specializarea nu exista in suita selectata.");
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void DeleteSpecializationFromOptionSample(
            int optionSampleId, int specializationId)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    ExecuteCatalogDelete(
                        connection, transaction, optionSampleId,
                        "DELETE FROM AdmitereFinala WHERE IdEsantionOptiuni = @IdEsantionOptiuni AND IdSpecializare = @IdSpecializare",
                        specializationId);
                    int deleted = ExecuteCatalogDelete(
                        connection, transaction, optionSampleId,
                        "DELETE FROM LocuriEsantionOptiuni WHERE IdEsantionOptiuni = @IdEsantionOptiuni AND IdSpecializare = @IdSpecializare",
                        specializationId);
                    if (deleted == 0)
                        throw new InvalidOperationException(
                            "Specializarea nu exista in suita selectata.");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void DeleteFacultyFromOptionSample(
            int optionSampleId, int facultyId)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    EnsureFacultyExists(connection, transaction, facultyId);
                    using (SQLiteCommand clearAdmissions = new SQLiteCommand(@"
                        DELETE FROM AdmitereFinala
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni
                          AND IdSpecializare IN
                          (
                              SELECT IdSpecializare
                              FROM Specializari
                              WHERE IdFacultate = @IdFacultate
                          )",
                        connection, transaction))
                    {
                        AddParameter(clearAdmissions, "@IdEsantionOptiuni",
                            optionSampleId);
                        AddParameter(clearAdmissions, "@IdFacultate", facultyId);
                        clearAdmissions.ExecuteNonQuery();
                    }

                    using (SQLiteCommand deleteRows = new SQLiteCommand(@"
                        DELETE FROM LocuriEsantionOptiuni
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni
                          AND IdSpecializare IN
                          (
                              SELECT IdSpecializare
                              FROM Specializari
                              WHERE IdFacultate = @IdFacultate
                          )",
                        connection, transaction))
                    {
                        AddParameter(deleteRows, "@IdEsantionOptiuni",
                            optionSampleId);
                        AddParameter(deleteRows, "@IdFacultate", facultyId);
                        if (deleteRows.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                "Facultatea nu are specializari in suita selectata.");
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static DataRow GetSampleSummary(int sampleId)
        {
            DataTable data = ExecuteQuery(@"
                SELECT
                    E.Nume AS Nume,
                    COUNT(C.IdCandidat) AS Total,
                    SUM(CASE WHEN C.Status = 'Nedefinit' THEN 1 ELSE 0 END) AS Nedefinit,
                    SUM(CASE WHEN C.Status = 'Admis' THEN 1 ELSE 0 END) AS Admisi,
                    SUM(CASE WHEN C.Status = 'Respins' THEN 1 ELSE 0 END) AS Respinsi,
                    (
                        SELECT COUNT(*)
                        FROM ImporturiWeb I
                        INNER JOIN Candidati CI ON I.IdCandidat = CI.IdCandidat
                        WHERE CI.IdEsantion = E.IdEsantion
                    ) AS Importate
                FROM Esantioane E
                LEFT JOIN Candidati C ON C.IdEsantion = E.IdEsantion
                WHERE E.IdEsantion = @IdEsantion
                GROUP BY E.IdEsantion, E.Nume",
                CreateParameter("@IdEsantion", sampleId));

            return data.Rows.Count > 0 ? data.Rows[0] : null;
        }

        public static int SaveApplication(
            Candidat candidate, IList<string> specializationNames)
        {
            return SaveApplication(
                candidate, specializationNames, GetDefaultSampleId());
        }

        public static int SaveApplication(
            Candidat candidate, IList<string> specializationNames, int sampleId)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");
            if (specializationNames == null || specializationNames.Count == 0)
                throw new ArgumentException(
                    "Este necesară cel puțin o opțiune.", "specializationNames");

            EnsureSampleExists(sampleId);
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (CandidateExistsByCnp(connection, transaction, sampleId, candidate.CNP))
                        throw new InvalidOperationException(
                            "Există deja o înscriere pentru acest CNP în eșantionul selectat.");

                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO Candidati
                            (IdEsantion, Nume, Prenume, Adresa, Varsta, Sex, CNP,
                             MedieBAC, MedieLiceu, Status)
                        VALUES
                            (@IdEsantion, @Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP,
                             @MedieBAC, @MedieLiceu, 'Nedefinit')",
                        connection, transaction))
                    {
                        AddParameter(command, "@IdEsantion", sampleId);
                        AddParameter(command, "@Nume", candidate.Nume);
                        AddParameter(command, "@Prenume", candidate.Prenume);
                        AddParameter(command, "@Adresa", candidate.Adresa);
                        AddParameter(command, "@Varsta", candidate.Varsta);
                        AddParameter(command, "@Sex", candidate.Sex);
                        AddParameter(command, "@CNP", candidate.CNP);
                        AddParameter(command, "@MedieBAC", candidate.MedieBAC);
                        AddParameter(command, "@MedieLiceu", candidate.MedieLiceu);
                        command.ExecuteNonQuery();
                    }

                    int candidateId = Convert.ToInt32(connection.LastInsertRowId);
                    InsertCandidateChoices(
                        connection, transaction, candidateId, specializationNames);

                    transaction.Commit();
                    return candidateId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static ImportResult ImportWebSubmission(WebSubmission submission)
        {
            return ImportWebSubmission(submission, GetDefaultSampleId());
        }

        public static ImportResult ImportWebSubmission(
            WebSubmission submission, int sampleId)
        {
            if (submission == null)
                throw new ArgumentNullException("submission");
            if (submission.options == null || submission.options.Count == 0)
                throw new InvalidOperationException("Înscrierea nu conține opțiuni.");

            EnsureSampleExists(sampleId);
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand checkImport = new SQLiteCommand(@"
                        SELECT IdCandidat
                        FROM ImporturiWeb
                        WHERE ExternalId = @ExternalId",
                        connection, transaction))
                    {
                        AddParameter(checkImport, "@ExternalId", submission.id);
                        if (checkImport.ExecuteScalar() != null)
                        {
                            transaction.Commit();
                            return ImportResult.AlreadyPresent;
                        }
                    }

                    int candidateId = FindCandidateIdByCnp(
                        connection, transaction, sampleId, submission.cnp);
                    ImportResult result = ImportResult.AlreadyPresent;

                    if (candidateId == 0)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(@"
                            INSERT INTO Candidati
                                (IdEsantion, Nume, Prenume, Adresa, Varsta, Sex, CNP,
                                 MedieBAC, MedieLiceu, Status)
                            VALUES
                                (@IdEsantion, @Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP,
                                 @MedieBAC, @MedieLiceu, 'Nedefinit')",
                            connection, transaction))
                        {
                            AddParameter(command, "@IdEsantion", sampleId);
                            AddParameter(command, "@Nume", submission.nume);
                            AddParameter(command, "@Prenume", submission.prenume);
                            AddParameter(command, "@Adresa", submission.adresa);
                            AddParameter(command, "@Varsta", submission.varsta);
                            AddParameter(command, "@Sex", submission.sex);
                            AddParameter(command, "@CNP", submission.cnp);
                            AddParameter(command, "@MedieBAC", submission.medieBac);
                            AddParameter(command, "@MedieLiceu", submission.medieLiceu);
                            command.ExecuteNonQuery();
                        }

                        candidateId = Convert.ToInt32(connection.LastInsertRowId);
                        InsertCandidateChoices(
                            connection, transaction, candidateId, submission.options);
                        result = ImportResult.Imported;
                    }

                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO ImporturiWeb
                            (ExternalId, CodInscriere, IdCandidat, CreatLaFormular)
                        VALUES
                            (@ExternalId, @CodInscriere, @IdCandidat, @CreatLaFormular)",
                        connection, transaction))
                    {
                        AddParameter(command, "@ExternalId", submission.id);
                        AddParameter(command, "@CodInscriere",
                            submission.submissionCode ?? string.Empty);
                        AddParameter(command, "@IdCandidat", candidateId);
                        AddParameter(command, "@CreatLaFormular",
                            submission.createdAt ?? string.Empty);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static bool DeleteCandidate(int candidateId)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    ExecuteTransactionCommand(connection, transaction,
                        "DELETE FROM AdmitereFinala WHERE IdCandidat = @Id", candidateId);
                    ExecuteTransactionCommand(connection, transaction,
                        "DELETE FROM ImporturiWeb WHERE IdCandidat = @Id", candidateId);
                    ExecuteTransactionCommand(connection, transaction,
                        "DELETE FROM OptiuniCandidat WHERE IdCandidat = @Id", candidateId);
                    int deleted = ExecuteTransactionCommand(connection, transaction,
                        "DELETE FROM Candidati WHERE IdCandidat = @Id", candidateId);
                    transaction.Commit();
                    return deleted > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static bool UpdateCandidateStatus(int candidateId, string status)
        {
            const string query = @"
                UPDATE Candidati
                SET Status = @Status
                WHERE IdCandidat = @Id";

            return InsertUpdateOrDelete(query,
                CreateParameter("@Status", status),
                CreateParameter("@Id", candidateId)) > 0;
        }

        public static int RunAdmission()
        {
            int sampleId = GetDefaultSampleId();
            return RunAdmission(sampleId, GetDefaultOptionSampleId(sampleId), "weighted");
        }

        public static int RunAdmission(int sampleId, string algorithm)
        {
            return RunAdmission(sampleId, GetDefaultOptionSampleId(sampleId), algorithm);
        }

        public static int RunAdmission(
            int sampleId, int optionSampleId, string algorithm)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureSampleExists(connection, transaction, sampleId);
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    EnsureSampleOptionAssociationExists(
                        connection, transaction, sampleId, optionSampleId);
                    string safeAlgorithm = NormalizeAlgorithm(algorithm);
                    string orderExpression = GetAlgorithmExpression(safeAlgorithm);
                    Dictionary<int, int> availableSeats = new Dictionary<int, int>();
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        SELECT IdSpecializare, NrLocuri
                        FROM LocuriEsantionOptiuni
                        WHERE IdEsantionOptiuni = @IdEsantionOptiuni",
                        connection, transaction))
                    {
                        AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                availableSeats[reader.GetInt32(0)] =
                                    reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }
                    }

                    if (availableSeats.Count == 0)
                        throw new InvalidOperationException(
                            "Esantionul de optiuni nu are specializari sau locuri configurate.");

                    string candidatesQuery = @"
                        SELECT C.IdCandidat, C.MedieLiceu, C.MedieBAC,
                               O.IdSpecializare1, O.IdSpecializare2, O.IdSpecializare3
                        FROM Candidati C
                        INNER JOIN
                        (
                            SELECT O1.*
                            FROM OptiuniCandidat O1
                            INNER JOIN
                            (
                                SELECT IdCandidat, MAX(IdOptiune) AS IdOptiune
                                FROM OptiuniCandidat
                                GROUP BY IdCandidat
                            ) Ultima
                                ON O1.IdCandidat = Ultima.IdCandidat
                               AND O1.IdOptiune = Ultima.IdOptiune
                        ) O ON C.IdCandidat = O.IdCandidat
                        WHERE C.IdEsantion = @IdEsantion
                        ORDER BY " + orderExpression + @" DESC,
                                 C.MedieBAC DESC,
                                 C.MedieLiceu DESC,
                                 C.IdCandidat ASC";

                    DataTable candidates = new DataTable();
                    using (SQLiteCommand command = new SQLiteCommand(
                        candidatesQuery, connection, transaction))
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        AddParameter(command, "@IdEsantion", sampleId);
                        adapter.Fill(candidates);
                    }

                    using (SQLiteCommand clear = new SQLiteCommand(@"
                        DELETE FROM AdmitereFinala
                        WHERE IdEsantion = @IdEsantion
                          AND Algoritm = @Algoritm",
                        connection, transaction))
                    {
                        AddParameter(clear, "@IdEsantion", sampleId);
                        AddParameter(clear, "@Algoritm", safeAlgorithm);
                        clear.ExecuteNonQuery();
                    }
                    using (SQLiteCommand reset = new SQLiteCommand(@"
                        UPDATE Candidati
                        SET Status = 'Nedefinit'
                        WHERE IdEsantion = @IdEsantion",
                        connection, transaction))
                    {
                        AddParameter(reset, "@IdEsantion", sampleId);
                        reset.ExecuteNonQuery();
                    }

                    int admitted = 0;
                    foreach (DataRow row in candidates.Rows)
                    {
                        int candidateId = Convert.ToInt32(row["IdCandidat"]);
                        int selectedSpecialization = 0;
                        string[] optionColumns =
                            { "IdSpecializare1", "IdSpecializare2", "IdSpecializare3" };

                        foreach (string column in optionColumns)
                        {
                            if (row[column] == DBNull.Value)
                                continue;

                            int specializationId = Convert.ToInt32(row[column]);
                            int seats;
                            if (availableSeats.TryGetValue(specializationId, out seats) &&
                                seats > 0)
                            {
                                selectedSpecialization = specializationId;
                                availableSeats[specializationId] = seats - 1;
                                break;
                            }
                        }

                        if (selectedSpecialization > 0)
                        {
                            using (SQLiteCommand insert = new SQLiteCommand(@"
                                INSERT INTO AdmitereFinala
                                    (IdEsantion, IdEsantionOptiuni, Algoritm,
                                     IdCandidat, IdSpecializare)
                                VALUES
                                    (@IdEsantion, @IdEsantionOptiuni, @Algoritm,
                                     @IdCandidat, @IdSpecializare)",
                                connection, transaction))
                            {
                                AddParameter(insert, "@IdEsantion", sampleId);
                                AddParameter(insert, "@IdEsantionOptiuni",
                                    optionSampleId);
                                AddParameter(insert, "@Algoritm", safeAlgorithm);
                                AddParameter(insert, "@IdCandidat", candidateId);
                                AddParameter(insert, "@IdSpecializare",
                                    selectedSpecialization);
                                insert.ExecuteNonQuery();
                            }
                            admitted++;
                        }

                        using (SQLiteCommand update = new SQLiteCommand(@"
                            UPDATE Candidati
                            SET Status = @Status
                            WHERE IdCandidat = @IdCandidat",
                            connection, transaction))
                        {
                            AddParameter(update, "@Status",
                                selectedSpecialization > 0 ? "Admis" : "Respins");
                            AddParameter(update, "@IdCandidat", candidateId);
                            update.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return admitted;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void ResetAdmission()
        {
            int sampleId = GetDefaultSampleId();
            ResetAdmission(sampleId, GetDefaultOptionSampleId(sampleId), "weighted");
        }

        public static void ResetAdmission(int sampleId, string algorithm)
        {
            ResetAdmission(sampleId, GetDefaultOptionSampleId(sampleId), algorithm);
        }

        public static void ResetAdmission(
            int sampleId, int optionSampleId, string algorithm)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureSampleExists(connection, transaction, sampleId);
                    EnsureOptionSampleExists(connection, transaction, optionSampleId);
                    string safeAlgorithm = NormalizeAlgorithm(algorithm);
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        DELETE FROM AdmitereFinala
                        WHERE IdEsantion = @IdEsantion
                          AND IdEsantionOptiuni = @IdEsantionOptiuni
                          AND Algoritm = @Algoritm",
                        connection, transaction))
                    {
                        AddParameter(command, "@IdEsantion", sampleId);
                        AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                        AddParameter(command, "@Algoritm", safeAlgorithm);
                        command.ExecuteNonQuery();
                    }
                    using (SQLiteCommand command = new SQLiteCommand(@"
                        UPDATE Candidati
                        SET Status = 'Nedefinit'
                        WHERE IdEsantion = @IdEsantion",
                        connection, transaction))
                    {
                        AddParameter(command, "@IdEsantion", sampleId);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static int GenerateDemoSample(string name, int count)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Eșantion demo " + DateTime.Now.ToString("dd.MM HH:mm");
            if (count <= 0)
                count = 60;

            int sampleId = CreateSample(name);
            string[] firstNames =
            {
                "Andrei", "Maria", "Ioana", "Alexandru", "Elena", "Mihai",
                "Daria", "Radu", "Teodora", "Vlad", "Ana", "Iulia",
                "Matei", "Irina", "David", "Bianca", "Sofia", "Rareș"
            };
            string[] lastNames =
            {
                "Popescu", "Ionescu", "Dumitrescu", "Stan", "Radu",
                "Marin", "Georgescu", "Munteanu", "Stoica", "Nistor",
                "Diaconu", "Tudor", "Barbu", "Enache", "Ilie"
            };

            Random random = new Random();
            int optionSampleId = GetDefaultOptionSampleId(sampleId);
            DataTable specializations = ExecuteQuery(@"
                SELECT S.NumeSpecializare
                FROM LocuriEsantionOptiuni L
                INNER JOIN Specializari S ON L.IdSpecializare = S.IdSpecializare
                WHERE L.IdEsantionOptiuni = @IdEsantionOptiuni
                ORDER BY S.IdSpecializare",
                CreateParameter("@IdEsantionOptiuni", optionSampleId));

            if (specializations.Rows.Count == 0)
                throw new InvalidOperationException(
                    "Nu exista specializari in esantionul de optiuni asociat.");

            for (int i = 0; i < count; i++)
            {
                Candidat candidate = new Candidat
                {
                    Nume = lastNames[random.Next(lastNames.Length)],
                    Prenume = firstNames[random.Next(firstNames.Length)],
                    Adresa = "Adresă demo " + (i + 1),
                    Varsta = random.Next(18, 27),
                    Sex = random.Next(2) == 0 ? "Feminin" : "Masculin",
                    CNP = (1000000000000L + ((long)sampleId * 100000L) + i)
                        .ToString(),
                    MedieBAC = Math.Round(6.0 + random.NextDouble() * 4.0, 2),
                    MedieLiceu = Math.Round(6.0 + random.NextDouble() * 4.0, 2),
                    Status = "Nedefinit"
                };

                List<string> options = new List<string>();
                foreach (int index in Enumerable.Range(0, specializations.Rows.Count)
                    .OrderBy(x => random.Next()).Take(3))
                {
                    options.Add(specializations.Rows[index]["NumeSpecializare"].ToString());
                }

                SaveApplication(candidate, options, sampleId);
            }

            return sampleId;
        }

        private static bool CandidateExistsByCnp(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sampleId, string cnp)
        {
            return FindCandidateIdByCnp(connection, transaction, sampleId, cnp) > 0;
        }

        private static int FindCandidateIdByCnp(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sampleId, string cnp)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT IdCandidat
                FROM Candidati
                WHERE IdEsantion = @IdEsantion
                  AND CNP = @CNP",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantion", sampleId);
                AddParameter(command, "@CNP", cnp);
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }

        private static void EnsureSampleExists(int sampleId)
        {
            DataTable data = ExecuteQuery(@"
                SELECT COUNT(*) AS CountValue
                FROM Esantioane
                WHERE IdEsantion = @IdEsantion",
                CreateParameter("@IdEsantion", sampleId));

            if (Convert.ToInt32(data.Rows[0]["CountValue"]) == 0)
                throw new InvalidOperationException(
                    "Eșantionul selectat nu mai există.");
        }

        private static void EnsureSampleExists(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sampleId)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM Esantioane
                WHERE IdEsantion = @IdEsantion",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantion", sampleId);
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    throw new InvalidOperationException(
                        "Esantionul de studenti selectat nu mai exista.");
            }
        }

        private static void EnsureOptionSampleExists(int optionSampleId)
        {
            DataTable data = ExecuteQuery(@"
                SELECT COUNT(*) AS CountValue
                FROM EsantioaneOptiuni
                WHERE IdEsantionOptiuni = @IdEsantionOptiuni",
                CreateParameter("@IdEsantionOptiuni", optionSampleId));

            if (Convert.ToInt32(data.Rows[0]["CountValue"]) == 0)
                throw new InvalidOperationException(
                    "Esantionul de optiuni selectat nu mai exista.");
        }

        private static void EnsureOptionSampleExists(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int optionSampleId)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM EsantioaneOptiuni
                WHERE IdEsantionOptiuni = @IdEsantionOptiuni",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    throw new InvalidOperationException(
                        "Esantionul de optiuni selectat nu mai exista.");
            }
        }

        private static void EnsureSampleOptionAssociationExists(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sampleId, int optionSampleId)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM AsocieriEsantioane
                WHERE IdEsantion = @IdEsantion
                  AND IdEsantionOptiuni = @IdEsantionOptiuni",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantion", sampleId);
                AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    throw new InvalidOperationException(
                        "Esantionul de optiuni trebuie asociat cu esantionul de studenti inainte de repartizare.");
            }
        }

        private static void EnsureFacultyExists(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int facultyId)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM Facultati
                WHERE IdFacultate = @IdFacultate",
                connection, transaction))
            {
                AddParameter(command, "@IdFacultate", facultyId);
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    throw new InvalidOperationException(
                        "Facultatea selectata nu mai exista.");
            }
        }

        private static void EnsureSpecializationExists(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int specializationId)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM Specializari
                WHERE IdSpecializare = @IdSpecializare",
                connection, transaction))
            {
                AddParameter(command, "@IdSpecializare", specializationId);
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                    throw new InvalidOperationException(
                        "Specializarea selectata nu mai exista.");
            }
        }

        private static int FindSpecializationId(
            SQLiteConnection connection, SQLiteTransaction transaction,
            string specializationName)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT IdSpecializare
                FROM Specializari
                WHERE NumeSpecializare = @NumeSpecializare",
                connection, transaction))
            {
                AddParameter(command, "@NumeSpecializare", specializationName);
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }

        private static void ValidateCatalogInput(
            string facultyName, string facultyCode,
            string specializationName, int seats)
        {
            if (string.IsNullOrWhiteSpace(facultyName))
                throw new ArgumentException("Numele facultatii este obligatoriu.", "facultyName");
            if (string.IsNullOrWhiteSpace(facultyCode))
                throw new ArgumentException("Codul facultatii este obligatoriu.", "facultyCode");
            if (string.IsNullOrWhiteSpace(specializationName))
                throw new ArgumentException("Numele specializarii este obligatoriu.", "specializationName");
            if (seats <= 0)
                throw new ArgumentException("Numarul de locuri trebuie sa fie pozitiv.", "seats");
        }

        private static int SaveFacultyInternal(
            SQLiteConnection connection, SQLiteTransaction transaction,
            string facultyName, string facultyCode)
        {
            string code = facultyCode.Trim().ToUpperInvariant();
            using (SQLiteCommand insert = new SQLiteCommand(@"
                INSERT OR IGNORE INTO Facultati (NumeFacultate, Abreviere)
                VALUES (@NumeFacultate, @Abreviere)",
                connection, transaction))
            {
                AddParameter(insert, "@NumeFacultate", facultyName.Trim());
                AddParameter(insert, "@Abreviere", code);
                insert.ExecuteNonQuery();
            }

            using (SQLiteCommand select = new SQLiteCommand(@"
                SELECT IdFacultate
                FROM Facultati
                WHERE Abreviere = @Abreviere",
                connection, transaction))
            {
                AddParameter(select, "@Abreviere", code);
                return Convert.ToInt32(select.ExecuteScalar());
            }
        }

        private static int EnsureSpecializationInternal(
            SQLiteConnection connection, SQLiteTransaction transaction,
            string specializationName, int facultyId)
        {
            int specializationId = FindSpecializationId(
                connection, transaction, specializationName.Trim());
            if (specializationId > 0)
                return specializationId;

            using (SQLiteCommand command = new SQLiteCommand(@"
                INSERT INTO Specializari
                    (NumeSpecializare, NrLocuri, IdFacultate)
                VALUES
                    (@NumeSpecializare, 1, @IdFacultate)",
                connection, transaction))
            {
                AddParameter(command, "@NumeSpecializare", specializationName.Trim());
                AddParameter(command, "@IdFacultate", facultyId);
                command.ExecuteNonQuery();
            }

            return Convert.ToInt32(connection.LastInsertRowId);
        }

        private static int ExecuteCatalogDelete(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int optionSampleId, string query, int specializationId)
        {
            using (SQLiteCommand command = new SQLiteCommand(
                query, connection, transaction))
            {
                AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                AddParameter(command, "@IdSpecializare", specializationId);
                return command.ExecuteNonQuery();
            }
        }

        private static void AssociateOptionSampleInternal(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sampleId, int optionSampleId, bool makeDefault)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                INSERT OR IGNORE INTO AsocieriEsantioane
                    (IdEsantion, IdEsantionOptiuni, EsteImplicit)
                VALUES
                    (@IdEsantion, @IdEsantionOptiuni, @EsteImplicit)",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantion", sampleId);
                AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                AddParameter(command, "@EsteImplicit", makeDefault ? 1 : 0);
                command.ExecuteNonQuery();
            }
        }

        private static void CopyOptionSeatsInternal(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int sourceOptionSampleId, int targetOptionSampleId)
        {
            using (SQLiteCommand copy = new SQLiteCommand(@"
                INSERT OR IGNORE INTO LocuriEsantionOptiuni
                    (IdEsantionOptiuni, IdSpecializare, NrLocuri)
                SELECT @Target, IdSpecializare, NrLocuri
                FROM LocuriEsantionOptiuni
                WHERE IdEsantionOptiuni = @Source",
                connection, transaction))
            {
                AddParameter(copy, "@Target", targetOptionSampleId);
                AddParameter(copy, "@Source", sourceOptionSampleId);
                copy.ExecuteNonQuery();
            }

            int copiedRows;
            using (SQLiteCommand count = new SQLiteCommand(@"
                SELECT COUNT(*)
                FROM LocuriEsantionOptiuni
                WHERE IdEsantionOptiuni = @Target",
                connection, transaction))
            {
                AddParameter(count, "@Target", targetOptionSampleId);
                copiedRows = Convert.ToInt32(count.ExecuteScalar());
            }

            if (copiedRows > 0)
                return;

            using (SQLiteCommand fallback = new SQLiteCommand(@"
                INSERT OR IGNORE INTO LocuriEsantionOptiuni
                    (IdEsantionOptiuni, IdSpecializare, NrLocuri)
                SELECT @Target, IdSpecializare, NrLocuri
                FROM Specializari",
                connection, transaction))
            {
                AddParameter(fallback, "@Target", targetOptionSampleId);
                fallback.ExecuteNonQuery();
            }
        }

        private static void SetOptionSeatInternal(
            SQLiteConnection connection, SQLiteTransaction transaction,
            int optionSampleId, int specializationId, int seats)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                INSERT OR REPLACE INTO LocuriEsantionOptiuni
                    (IdEsantionOptiuni, IdSpecializare, NrLocuri)
                VALUES
                    (@IdEsantionOptiuni, @IdSpecializare, @NrLocuri)",
                connection, transaction))
            {
                AddParameter(command, "@IdEsantionOptiuni", optionSampleId);
                AddParameter(command, "@IdSpecializare", specializationId);
                AddParameter(command, "@NrLocuri", seats);
                command.ExecuteNonQuery();
            }
        }

        private static string NormalizeAlgorithm(string algorithm)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
                return "weighted";

            switch (algorithm.Trim().ToLowerInvariant())
            {
                case "bac":
                case "liceu":
                case "balanced":
                case "weighted":
                    return algorithm.Trim().ToLowerInvariant();
                default:
                    return "weighted";
            }
        }

        public static string GetAlgorithmExpression(string algorithm)
        {
            switch (NormalizeAlgorithm(algorithm))
            {
                case "bac":
                    return "C.MedieBAC";
                case "liceu":
                    return "C.MedieLiceu";
                case "balanced":
                    return "((C.MedieBAC + C.MedieLiceu) / 2.0)";
                default:
                    return "(C.MedieLiceu * 0.3 + C.MedieBAC * 0.7)";
            }
        }

        private static void InsertCandidateChoices(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            int candidateId,
            IList<string> specializationNames)
        {
            List<int> specializationIds = new List<int>();
            foreach (string specializationName in specializationNames.Take(3))
            {
                using (SQLiteCommand command = new SQLiteCommand(@"
                    SELECT IdSpecializare
                    FROM Specializari
                    WHERE NumeSpecializare = @NumeSpecializare",
                    connection, transaction))
                {
                    AddParameter(command, "@NumeSpecializare", specializationName);
                    object result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        throw new InvalidOperationException(
                            "Specializarea „" + specializationName +
                            "” nu există în baza de date.");
                    specializationIds.Add(Convert.ToInt32(result));
                }
            }

            if (specializationIds.Count == 0)
                throw new InvalidOperationException(
                    "Este necesară cel puțin o opțiune validă.");

            using (SQLiteCommand command = new SQLiteCommand(@"
                INSERT INTO OptiuniCandidat
                    (IdCandidat, IdSpecializare1,
                     IdSpecializare2, IdSpecializare3)
                VALUES
                    (@IdCandidat, @Optiune1, @Optiune2, @Optiune3)",
                connection, transaction))
            {
                AddParameter(command, "@IdCandidat", candidateId);
                AddParameter(command, "@Optiune1", specializationIds[0]);
                AddParameter(command, "@Optiune2",
                    specializationIds.Count > 1
                        ? (object)specializationIds[1]
                        : DBNull.Value);
                AddParameter(command, "@Optiune3",
                    specializationIds.Count > 2
                        ? (object)specializationIds[2]
                        : DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        private static int ExecuteTransactionCommand(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string query,
            int candidateId)
        {
            using (SQLiteCommand command = new SQLiteCommand(query, connection, transaction))
            {
                AddParameter(command, "@Id", candidateId);
                return command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(
            SQLiteCommand command, IEnumerable<IDbDataParameter> parameters)
        {
            if (parameters == null)
                return;

            foreach (IDbDataParameter parameter in parameters)
            {
                if (parameter == null)
                    continue;

                SQLiteParameter sqliteParameter = parameter as SQLiteParameter;
                if (sqliteParameter != null)
                {
                    command.Parameters.Add(sqliteParameter);
                }
                else
                {
                    AddParameter(command, parameter.ParameterName, parameter.Value);
                }
            }
        }

        private static void AddParameter(
            SQLiteCommand command, string name, object value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }
    }
}
