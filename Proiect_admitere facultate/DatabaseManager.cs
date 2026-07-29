using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        private const string DatabaseFileName = "Admitere_database.mdf";
        private const string DatabaseLogFileName = "Admitere_database_log.ldf";
        private const string ApplicationDataFolderName = "Sortare candidati admitere";
        private static readonly object DatabaseLock = new object();
        private static string resolvedDatabasePath;

        public static string DatabasePath
        {
            get { return ResolveDatabasePath(); }
        }

        public static string connectionString
        {
            get
            {
                return BuildAttachConnectionString(DatabasePath);
            }
        }

        private static string ResolveDatabasePath()
        {
            lock (DatabaseLock)
            {
                if (!string.IsNullOrEmpty(resolvedDatabasePath) &&
                    File.Exists(resolvedDatabasePath))
                    return resolvedDatabasePath;

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string outputDatabase = Path.Combine(baseDirectory, DatabaseFileName);
                if (File.Exists(outputDatabase))
                    return resolvedDatabasePath = outputDatabase;

                // Permite pornirea direct din Visual Studio cu o baza existenta in proiect.
                string projectDatabase = Path.GetFullPath(
                    Path.Combine(baseDirectory, "..", "..", DatabaseFileName));
                if (File.Exists(projectDatabase))
                    return resolvedDatabasePath = projectDatabase;

                string writableDatabase = Path.Combine(
                    ResolveWritableDatabaseDirectory(), DatabaseFileName);

                if (!File.Exists(writableDatabase))
                    CreateFreshDatabase(writableDatabase);

                return resolvedDatabasePath = writableDatabase;
            }
        }

        private static string BuildAttachConnectionString(string databasePath)
        {
            return string.Format(
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True;Connect Timeout=15",
                databasePath);
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

        private static void CreateFreshDatabase(string databasePath)
        {
            string databaseDirectory = Path.GetDirectoryName(databasePath);
            if (string.IsNullOrWhiteSpace(databaseDirectory))
                throw new InvalidOperationException(
                    "Nu se poate determina folderul pentru baza de date.");

            Directory.CreateDirectory(databaseDirectory);

            string logPath = Path.Combine(databaseDirectory, DatabaseLogFileName);
            if (File.Exists(logPath))
            {
                logPath = Path.Combine(databaseDirectory,
                    Path.GetFileNameWithoutExtension(DatabaseFileName) + "_" +
                    Guid.NewGuid().ToString("N") + "_log.ldf");
            }

            string databaseName = "Admitere_" + Guid.NewGuid().ToString("N");

            try
            {
                using (SqlConnection connection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=15"))
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 60;
                    command.CommandText = string.Format(@"
                        CREATE DATABASE [{0}]
                        ON PRIMARY
                        (
                            NAME = N'{0}_data',
                            FILENAME = N'{1}'
                        )
                        LOG ON
                        (
                            NAME = N'{0}_log',
                            FILENAME = N'{2}'
                        )",
                        databaseName,
                        EscapeSqlLiteral(databasePath),
                        EscapeSqlLiteral(logPath));

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                InitializeFreshDatabase(databasePath);
            }
            catch (Exception ex)
            {
                TryDropDatabase(databaseName);
                throw new InvalidOperationException(
                    "Baza de date locala nu a putut fi creata automat. " +
                    "Verifica daca Microsoft SQL Server LocalDB este instalat si pornit.",
                    ex);
            }
        }

        private static string EscapeSqlLiteral(string value)
        {
            return value.Replace("'", "''");
        }

        private static void TryDropDatabase(string databaseName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=15"))
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 30;
                    command.CommandText = string.Format(@"
                        IF DB_ID(N'{0}') IS NOT NULL
                        BEGIN
                            ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{0}];
                        END",
                        databaseName);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // Daca initializarea a esuat, eroarea originala este mai utila.
            }
        }

        private static void InitializeFreshDatabase(string databasePath)
        {
            using (SqlConnection connection = new SqlConnection(
                BuildAttachConnectionString(databasePath)))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandTimeout = 60;
                connection.Open();
                command.CommandText = InitialSchemaSql;
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureRuntimeTables(SqlConnection connection)
        {
            const string query = @"
                IF OBJECT_ID('ImporturiWeb', 'U') IS NULL
                BEGIN
                    CREATE TABLE ImporturiWeb
                    (
                        IdImport INT IDENTITY(1,1) PRIMARY KEY,
                        ExternalId INT NOT NULL UNIQUE,
                        CodInscriere NVARCHAR(40) NOT NULL,
                        IdCandidat INT NOT NULL,
                        ImportatLa DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT FK_ImporturiWeb_Candidati
                            FOREIGN KEY (IdCandidat)
                            REFERENCES Candidati(IdCandidat)
                    )
                END";

            using (SqlCommand command = new SqlCommand(query, connection))
                command.ExecuteNonQuery();
        }

        private const string InitialSchemaSql = @"
            CREATE TABLE Facultati
            (
                IdFacultate INT IDENTITY(1,1) PRIMARY KEY,
                NumeFacultate NVARCHAR(100) COLLATE Romanian_CI_AS NOT NULL,
                Abreviere NVARCHAR(10) COLLATE Romanian_CI_AS NOT NULL
            );

            CREATE TABLE Specializari
            (
                IdSpecializare INT IDENTITY(1,1) PRIMARY KEY,
                NumeSpecializare NVARCHAR(100) COLLATE Romanian_CI_AS NOT NULL,
                NrLocuri INT NOT NULL CHECK (NrLocuri > 0),
                IdFacultate INT NOT NULL,
                CONSTRAINT FK_Specializari_Facultati
                    FOREIGN KEY (IdFacultate) REFERENCES Facultati(IdFacultate)
            );

            CREATE TABLE Candidati
            (
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

            CREATE TABLE OptiuniCandidat
            (
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

            CREATE TABLE AdmitereFinala
            (
                IdAdmitere INT IDENTITY(1,1) PRIMARY KEY,
                IdCandidat INT NOT NULL,
                IdSpecializare INT NOT NULL,
                CONSTRAINT FK_AdmitereFinala_Candidati
                    FOREIGN KEY (IdCandidat) REFERENCES Candidati(IdCandidat),
                CONSTRAINT FK_AdmitereFinala_Specializari
                    FOREIGN KEY (IdSpecializare) REFERENCES Specializari(IdSpecializare)
            );

            CREATE TABLE ImporturiWeb
            (
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
                (N'Economie și comunicare economică în afaceri', 30, (SELECT IdFacultate FROM Facultati WHERE Abreviere = N'ETA'));";

        public static void ValidateDatabase()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME IN ('Candidati', 'Facultati', 'Specializari',
                                     'OptiuniCandidat', 'AdmitereFinala')";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                int tableCount = Convert.ToInt32(command.ExecuteScalar());
                if (tableCount != 5)
                    throw new InvalidOperationException(
                        "Structura bazei de date este incompletă. Sunt necesare 5 tabele.");

                EnsureRuntimeTables(connection);
            }
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                DataTable table = new DataTable();
                connection.Open();
                adapter.Fill(table);
                return table;
            }
        }

        public static int InsertUpdateOrDelete(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static int SaveApplication(Candidat candidate, IList<string> specializationNames)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");
            if (specializationNames == null || specializationNames.Count == 0)
                throw new ArgumentException("Este necesară cel puțin o opțiune.", "specializationNames");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string insertCandidate = @"
                            INSERT INTO Candidati
                                (Nume, Prenume, Adresa, Varsta, Sex, CNP, MedieBAC, MedieLiceu)
                            OUTPUT INSERTED.IdCandidat
                            VALUES
                                (@Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP, @MedieBAC, @MedieLiceu)";

                        int candidateId;
                        using (SqlCommand command = new SqlCommand(insertCandidate, connection, transaction))
                        {
                            command.Parameters.Add("@Nume", SqlDbType.NVarChar, 50).Value = candidate.Nume;
                            command.Parameters.Add("@Prenume", SqlDbType.NVarChar, 50).Value = candidate.Prenume;
                            command.Parameters.Add("@Adresa", SqlDbType.NVarChar, 100).Value = candidate.Adresa;
                            command.Parameters.Add("@Varsta", SqlDbType.Int).Value = candidate.Varsta;
                            command.Parameters.Add("@Sex", SqlDbType.NVarChar, 10).Value = candidate.Sex;
                            command.Parameters.Add("@CNP", SqlDbType.Char, 13).Value = candidate.CNP;
                            command.Parameters.Add("@MedieBAC", SqlDbType.Float).Value = candidate.MedieBAC;
                            command.Parameters.Add("@MedieLiceu", SqlDbType.Float).Value = candidate.MedieLiceu;
                            candidateId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        List<int> specializationIds = new List<int>();
                        const string findSpecialization = @"
                            SELECT IdSpecializare
                            FROM Specializari
                            WHERE NumeSpecializare = @NumeSpecializare";

                        foreach (string specializationName in specializationNames.Take(3))
                        {
                            using (SqlCommand command = new SqlCommand(findSpecialization, connection, transaction))
                            {
                                command.Parameters.Add("@NumeSpecializare", SqlDbType.NVarChar, 100)
                                    .Value = specializationName;
                                object result = command.ExecuteScalar();
                                if (result == null)
                                    throw new InvalidOperationException(
                                        "Specializarea „" + specializationName + "” nu există în baza de date.");
                                specializationIds.Add(Convert.ToInt32(result));
                            }
                        }

                        const string insertChoices = @"
                            INSERT INTO OptiuniCandidat
                                (IdCandidat, IdSpecializare1, IdSpecializare2, IdSpecializare3)
                            VALUES
                                (@IdCandidat, @Optiune1, @Optiune2, @Optiune3)";

                        using (SqlCommand command = new SqlCommand(insertChoices, connection, transaction))
                        {
                            command.Parameters.Add("@IdCandidat", SqlDbType.Int).Value = candidateId;
                            command.Parameters.Add("@Optiune1", SqlDbType.Int).Value = specializationIds[0];
                            command.Parameters.Add("@Optiune2", SqlDbType.Int).Value =
                                specializationIds.Count > 1 ? (object)specializationIds[1] : DBNull.Value;
                            command.Parameters.Add("@Optiune3", SqlDbType.Int).Value =
                                specializationIds.Count > 2 ? (object)specializationIds[2] : DBNull.Value;
                            command.ExecuteNonQuery();
                        }

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
        }

        public static ImportResult ImportWebSubmission(WebSubmission submission)
        {
            if (submission == null)
                throw new ArgumentNullException("submission");
            if (submission.options == null || submission.options.Count == 0)
                throw new InvalidOperationException("Înscrierea nu conține opțiuni.");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string createImportTable = @"
                            IF OBJECT_ID('ImporturiWeb', 'U') IS NULL
                            BEGIN
                                CREATE TABLE ImporturiWeb
                                (
                                    IdImport INT IDENTITY(1,1) PRIMARY KEY,
                                    ExternalId INT NOT NULL UNIQUE,
                                    CodInscriere NVARCHAR(40) NOT NULL,
                                    IdCandidat INT NOT NULL,
                                    ImportatLa DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                                    CONSTRAINT FK_ImporturiWeb_Candidati
                                        FOREIGN KEY (IdCandidat)
                                        REFERENCES Candidati(IdCandidat)
                                )
                            END";
                        using (SqlCommand command = new SqlCommand(
                            createImportTable, connection, transaction))
                            command.ExecuteNonQuery();

                        using (SqlCommand checkImport = new SqlCommand(
                            "SELECT IdCandidat FROM ImporturiWeb WHERE ExternalId = @ExternalId",
                            connection, transaction))
                        {
                            checkImport.Parameters.Add("@ExternalId", SqlDbType.Int)
                                .Value = submission.id;
                            if (checkImport.ExecuteScalar() != null)
                            {
                                transaction.Commit();
                                return ImportResult.AlreadyPresent;
                            }
                        }

                        int candidateId = 0;
                        using (SqlCommand checkCnp = new SqlCommand(
                            "SELECT IdCandidat FROM Candidati WHERE CNP = @CNP",
                            connection, transaction))
                        {
                            checkCnp.Parameters.Add("@CNP", SqlDbType.Char, 13)
                                .Value = submission.cnp;
                            object existingCandidate = checkCnp.ExecuteScalar();
                            if (existingCandidate != null)
                                candidateId = Convert.ToInt32(existingCandidate);
                        }

                        ImportResult result = ImportResult.AlreadyPresent;
                        if (candidateId == 0)
                        {
                            const string insertCandidate = @"
                                INSERT INTO Candidati
                                    (Nume, Prenume, Adresa, Varsta, Sex, CNP,
                                     MedieBAC, MedieLiceu, Status)
                                OUTPUT INSERTED.IdCandidat
                                VALUES
                                    (@Nume, @Prenume, @Adresa, @Varsta, @Sex, @CNP,
                                     @MedieBAC, @MedieLiceu, 'Nedefinit')";
                            using (SqlCommand command = new SqlCommand(
                                insertCandidate, connection, transaction))
                            {
                                command.Parameters.Add("@Nume", SqlDbType.NVarChar, 50)
                                    .Value = submission.nume;
                                command.Parameters.Add("@Prenume", SqlDbType.NVarChar, 50)
                                    .Value = submission.prenume;
                                command.Parameters.Add("@Adresa", SqlDbType.NVarChar, 100)
                                    .Value = submission.adresa;
                                command.Parameters.Add("@Varsta", SqlDbType.Int)
                                    .Value = submission.varsta;
                                command.Parameters.Add("@Sex", SqlDbType.NVarChar, 10)
                                    .Value = submission.sex;
                                command.Parameters.Add("@CNP", SqlDbType.Char, 13)
                                    .Value = submission.cnp;
                                command.Parameters.Add("@MedieBAC", SqlDbType.Float)
                                    .Value = submission.medieBac;
                                command.Parameters.Add("@MedieLiceu", SqlDbType.Float)
                                    .Value = submission.medieLiceu;
                                candidateId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            List<int> specializationIds = new List<int>();
                            foreach (string specializationName in submission.options.Take(3))
                            {
                                using (SqlCommand command = new SqlCommand(@"
                                    SELECT IdSpecializare
                                    FROM Specializari
                                    WHERE NumeSpecializare = @Nume",
                                    connection, transaction))
                                {
                                    command.Parameters.Add("@Nume", SqlDbType.NVarChar, 100)
                                        .Value = specializationName;
                                    object specializationId = command.ExecuteScalar();
                                    if (specializationId == null)
                                        throw new InvalidOperationException(
                                            "Specializarea „" + specializationName +
                                            "” nu există în lista de specializări.");
                                    specializationIds.Add(Convert.ToInt32(specializationId));
                                }
                            }

                            using (SqlCommand command = new SqlCommand(@"
                                INSERT INTO OptiuniCandidat
                                    (IdCandidat, IdSpecializare1,
                                     IdSpecializare2, IdSpecializare3)
                                VALUES
                                    (@IdCandidat, @Optiune1, @Optiune2, @Optiune3)",
                                connection, transaction))
                            {
                                command.Parameters.Add("@IdCandidat", SqlDbType.Int)
                                    .Value = candidateId;
                                command.Parameters.Add("@Optiune1", SqlDbType.Int)
                                    .Value = specializationIds[0];
                                command.Parameters.Add("@Optiune2", SqlDbType.Int)
                                    .Value = specializationIds.Count > 1
                                        ? (object)specializationIds[1] : DBNull.Value;
                                command.Parameters.Add("@Optiune3", SqlDbType.Int)
                                    .Value = specializationIds.Count > 2
                                        ? (object)specializationIds[2] : DBNull.Value;
                                command.ExecuteNonQuery();
                            }
                            result = ImportResult.Imported;
                        }

                        using (SqlCommand command = new SqlCommand(@"
                            INSERT INTO ImporturiWeb
                                (ExternalId, CodInscriere, IdCandidat)
                            VALUES
                                (@ExternalId, @CodInscriere, @IdCandidat)",
                            connection, transaction))
                        {
                            command.Parameters.Add("@ExternalId", SqlDbType.Int)
                                .Value = submission.id;
                            command.Parameters.Add("@CodInscriere", SqlDbType.NVarChar, 40)
                                .Value = submission.submissionCode ?? string.Empty;
                            command.Parameters.Add("@IdCandidat", SqlDbType.Int)
                                .Value = candidateId;
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
        }

        public static bool DeleteCandidate(int candidateId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteTransactionCommand(connection, transaction,
                            "DELETE FROM AdmitereFinala WHERE IdCandidat = @Id", candidateId);
                        ExecuteTransactionCommand(connection, transaction,
                            "IF OBJECT_ID('ImporturiWeb', 'U') IS NOT NULL DELETE FROM ImporturiWeb WHERE IdCandidat = @Id",
                            candidateId);
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
        }

        public static bool UpdateCandidateStatus(int candidateId, string status)
        {
            const string query = @"
                UPDATE Candidati
                SET Status = @Status
                WHERE IdCandidat = @Id";

            return InsertUpdateOrDelete(query,
                new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = status },
                new SqlParameter("@Id", SqlDbType.Int) { Value = candidateId }) > 0;
        }

        public static int RunAdmission()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<int, int> availableSeats = new Dictionary<int, int>();
                        using (SqlCommand command = new SqlCommand(
                            "SELECT IdSpecializare, NrLocuri FROM Specializari", connection, transaction))
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                availableSeats[reader.GetInt32(0)] =
                                    reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }

                        const string candidatesQuery = @"
                            SELECT C.IdCandidat, C.MedieLiceu, C.MedieBAC,
                                   O.IdSpecializare1, O.IdSpecializare2, O.IdSpecializare3
                            FROM Candidati C
                            CROSS APPLY
                            (
                                SELECT TOP 1 IdSpecializare1, IdSpecializare2, IdSpecializare3
                                FROM OptiuniCandidat
                                WHERE IdCandidat = C.IdCandidat
                                ORDER BY IdOptiune DESC
                            ) O
                            ORDER BY (C.MedieLiceu * 0.3 + C.MedieBAC * 0.7) DESC,
                                     C.IdCandidat ASC";

                        DataTable candidates = new DataTable();
                        using (SqlCommand command = new SqlCommand(candidatesQuery, connection, transaction))
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            adapter.Fill(candidates);

                        using (SqlCommand clear = new SqlCommand(
                            "DELETE FROM AdmitereFinala; UPDATE Candidati SET Status = 'Nedefinit';",
                            connection, transaction))
                            clear.ExecuteNonQuery();

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
                                if (availableSeats.TryGetValue(specializationId, out seats) && seats > 0)
                                {
                                    selectedSpecialization = specializationId;
                                    availableSeats[specializationId] = seats - 1;
                                    break;
                                }
                            }

                            if (selectedSpecialization > 0)
                            {
                                using (SqlCommand insert = new SqlCommand(@"
                                    INSERT INTO AdmitereFinala (IdCandidat, IdSpecializare)
                                    VALUES (@IdCandidat, @IdSpecializare)", connection, transaction))
                                {
                                    insert.Parameters.Add("@IdCandidat", SqlDbType.Int).Value = candidateId;
                                    insert.Parameters.Add("@IdSpecializare", SqlDbType.Int)
                                        .Value = selectedSpecialization;
                                    insert.ExecuteNonQuery();
                                }
                                admitted++;
                            }

                            using (SqlCommand update = new SqlCommand(@"
                                UPDATE Candidati
                                SET Status = @Status
                                WHERE IdCandidat = @IdCandidat", connection, transaction))
                            {
                                update.Parameters.Add("@Status", SqlDbType.NVarChar, 20)
                                    .Value = selectedSpecialization > 0 ? "Admis" : "Respins";
                                update.Parameters.Add("@IdCandidat", SqlDbType.Int).Value = candidateId;
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
        }

        public static void ResetAdmission()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(@"
                            DELETE FROM AdmitereFinala;
                            UPDATE Candidati SET Status = 'Nedefinit';",
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
        }

        private static int ExecuteTransactionCommand(SqlConnection connection,
            SqlTransaction transaction, string query, int candidateId)
        {
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = candidateId;
                return command.ExecuteNonQuery();
            }
        }
    }
}
