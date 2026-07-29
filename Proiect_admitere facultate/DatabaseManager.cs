using System;
using System.Collections.Generic;
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

        public static string DatabasePath
        {
            get { return ResolveDatabasePath(); }
        }

        public static string connectionString
        {
            get
            {
                return string.Format(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True;Connect Timeout=15",
                    DatabasePath);
            }
        }

        private static string ResolveDatabasePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string outputDatabase = Path.Combine(baseDirectory, DatabaseFileName);
            if (File.Exists(outputDatabase))
                return outputDatabase;

            // Permite pornirea direct din Visual Studio chiar înainte de prima compilare.
            string projectDatabase = Path.GetFullPath(
                Path.Combine(baseDirectory, "..", "..", DatabaseFileName));
            if (File.Exists(projectDatabase))
                return projectDatabase;

            throw new FileNotFoundException(
                "Baza de date nu a fost găsită lângă aplicație.", outputDatabase);
        }

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
