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
        }

        private const string InitialSchemaSql = @"
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

            CREATE TABLE IF NOT EXISTS Candidati
            (
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
                IdCandidat INTEGER NOT NULL,
                IdSpecializare INTEGER NOT NULL,
                FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
                FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare)
            );

            CREATE TABLE IF NOT EXISTS ImporturiWeb
            (
                IdImport INTEGER PRIMARY KEY AUTOINCREMENT,
                ExternalId INTEGER NOT NULL UNIQUE,
                CodInscriere TEXT NOT NULL,
                IdCandidat INTEGER NOT NULL,
                ImportatLa TEXT NOT NULL DEFAULT (datetime('now')),
                FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat)
            );

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
                               'OptiuniCandidat', 'AdmitereFinala', 'ImporturiWeb')";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                int tableCount = Convert.ToInt32(command.ExecuteScalar());
                if (tableCount != 6)
                    throw new InvalidOperationException(
                        "Structura bazei de date este incompletă. Sunt necesare 6 tabele.");
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

        public static int SaveApplication(
            Candidat candidate, IList<string> specializationNames)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");
            if (specializationNames == null || specializationNames.Count == 0)
                throw new ArgumentException(
                    "Este necesară cel puțin o opțiune.", "specializationNames");

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (CandidateExistsByCnp(connection, transaction, candidate.CNP))
                        throw new InvalidOperationException(
                            "Există deja o înscriere pentru acest CNP.");

                    using (SQLiteCommand command = new SQLiteCommand(@"
                        INSERT INTO Candidati
                            (Nume, Prenume, Adresa, Varsta, Sex, CNP,
                             MedieBAC, MedieLiceu, Status)
                        VALUES
                            (@Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP,
                             @MedieBAC, @MedieLiceu, 'Nedefinit')",
                        connection, transaction))
                    {
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
            if (submission == null)
                throw new ArgumentNullException("submission");
            if (submission.options == null || submission.options.Count == 0)
                throw new InvalidOperationException("Înscrierea nu conține opțiuni.");

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
                        connection, transaction, submission.cnp);
                    ImportResult result = ImportResult.AlreadyPresent;

                    if (candidateId == 0)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(@"
                            INSERT INTO Candidati
                                (Nume, Prenume, Adresa, Varsta, Sex, CNP,
                                 MedieBAC, MedieLiceu, Status)
                            VALUES
                                (@Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP,
                                 @MedieBAC, @MedieLiceu, 'Nedefinit')",
                            connection, transaction))
                        {
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
                            (ExternalId, CodInscriere, IdCandidat)
                        VALUES
                            (@ExternalId, @CodInscriere, @IdCandidat)",
                        connection, transaction))
                    {
                        AddParameter(command, "@ExternalId", submission.id);
                        AddParameter(command, "@CodInscriere",
                            submission.submissionCode ?? string.Empty);
                        AddParameter(command, "@IdCandidat", candidateId);
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
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    Dictionary<int, int> availableSeats = new Dictionary<int, int>();
                    using (SQLiteCommand command = new SQLiteCommand(
                        "SELECT IdSpecializare, NrLocuri FROM Specializari",
                        connection, transaction))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            availableSeats[reader.GetInt32(0)] =
                                reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }

                    const string candidatesQuery = @"
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
                        ORDER BY (C.MedieLiceu * 0.3 + C.MedieBAC * 0.7) DESC,
                                 C.IdCandidat ASC";

                    DataTable candidates = new DataTable();
                    using (SQLiteCommand command = new SQLiteCommand(
                        candidatesQuery, connection, transaction))
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                        adapter.Fill(candidates);

                    using (SQLiteCommand clear = new SQLiteCommand(
                        "DELETE FROM AdmitereFinala", connection, transaction))
                        clear.ExecuteNonQuery();
                    using (SQLiteCommand reset = new SQLiteCommand(
                        "UPDATE Candidati SET Status = 'Nedefinit'",
                        connection, transaction))
                        reset.ExecuteNonQuery();

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
                                    (IdCandidat, IdSpecializare)
                                VALUES
                                    (@IdCandidat, @IdSpecializare)",
                                connection, transaction))
                            {
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
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(
                        "DELETE FROM AdmitereFinala", connection, transaction))
                        command.ExecuteNonQuery();
                    using (SQLiteCommand command = new SQLiteCommand(
                        "UPDATE Candidati SET Status = 'Nedefinit'",
                        connection, transaction))
                        command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static bool CandidateExistsByCnp(
            SQLiteConnection connection, SQLiteTransaction transaction, string cnp)
        {
            return FindCandidateIdByCnp(connection, transaction, cnp) > 0;
        }

        private static int FindCandidateIdByCnp(
            SQLiteConnection connection, SQLiteTransaction transaction, string cnp)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT IdCandidat
                FROM Candidati
                WHERE CNP = @CNP",
                connection, transaction))
            {
                AddParameter(command, "@CNP", cnp);
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
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
