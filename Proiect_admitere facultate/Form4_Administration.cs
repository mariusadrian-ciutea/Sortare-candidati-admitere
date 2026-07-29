using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Proiect_admitere_facultate
{
    public partial class Form4_Administration : AeroForm
    {
        private readonly Questioning questioningForm;

        public Form4_Administration(Questioning formTrimis)
        {
            InitializeComponent();
            questioningForm = formTrimis;
            AeroTheme.ApplyAdministration(this, dataGridView1, afiseaza_studenti,
                startCandidateAdmitionSequence, arata_candidati_admisi,
                reseteaza_lista_candidati_admisi, prevForm, label1,
                cod_unic_pt_eliminare, stergeCandidat, label2, alegeStatut,
                modificaCandidat, cod_unic_pt_modificare, label3, alegeCriteriu,
                valoare_criteriu, cautaCandidat);
            FormClosed += delegate
            {
                if (!questioningForm.IsDisposed && !questioningForm.Visible)
                    questioningForm.Show();
            };
        }

        private void Form4_Administration_Load(object sender, EventArgs e)
        {
            alegeCriteriu.Items.Clear();
            alegeCriteriu.Items.AddRange(new object[]
            {
                "CNP", "ID", "Status: Admis", "Status: Respins", "Status: Nedefinit"
            });

            alegeStatut.Items.Clear();
            alegeStatut.Items.AddRange(new object[] { "Admis", "Respins", "Nedefinit" });
            alegeCriteriu.SelectedIndex = 0;
            alegeStatut.SelectedIndex = 2;
            ShowAllCandidates();
        }

        private void ShowAllCandidates()
        {
            const string query = @"
                SELECT
                    C.IdCandidat AS [cod unic],
                    C.Nume || ' ' || C.Prenume AS [Nume complet],
                    C.Adresa AS [Adresa],
                    C.MedieLiceu,
                    C.MedieBAC,
                    S1.NumeSpecializare AS [Optiunea 1],
                    S2.NumeSpecializare AS [Optiunea 2],
                    S3.NumeSpecializare AS [Optiunea 3],
                    C.Varsta AS [Vârstă],
                    C.Sex AS [Sex],
                    C.CNP AS [Cod numeric personal],
                    C.Status AS [Status]
                FROM Candidati C
                LEFT JOIN
                (
                    SELECT O.*
                    FROM OptiuniCandidat O
                    INNER JOIN
                    (
                        SELECT IdCandidat, MAX(IdOptiune) AS IdOptiune
                        FROM OptiuniCandidat
                        GROUP BY IdCandidat
                    ) Ultima
                        ON O.IdCandidat = Ultima.IdCandidat
                       AND O.IdOptiune = Ultima.IdOptiune
                ) O ON C.IdCandidat = O.IdCandidat
                LEFT JOIN Specializari S1 ON O.IdSpecializare1 = S1.IdSpecializare
                LEFT JOIN Specializari S2 ON O.IdSpecializare2 = S2.IdSpecializare
                LEFT JOIN Specializari S3 ON O.IdSpecializare3 = S3.IdSpecializare
                ORDER BY C.IdCandidat";

            try
            {
                dataGridView1.DataSource = DatabaseManager.ExecuteQuery(query);
                dataGridView1.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Baza de date nu a putut fi încărcată:\n" + ex.Message,
                    "Eroare bază de date", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void afiseaza_studenti_Click(object sender, EventArgs e)
        {
            ShowAllCandidates();
        }

        private void stergeCandidat_Click(object sender, EventArgs e)
        {
            int candidateId;
            if (!int.TryParse(cod_unic_pt_eliminare.Text.Trim(), out candidateId) ||
                candidateId <= 0)
            {
                MessageBox.Show("Codul introdus trebuie să fie un număr valid.");
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                "Ștergi definitiv candidatul cu ID " + candidateId + " și opțiunile sale?",
                "Confirmare ștergere", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmation != DialogResult.Yes)
                return;

            try
            {
                bool deleted = DatabaseManager.DeleteCandidate(candidateId);
                MessageBox.Show(deleted
                    ? "Candidatul a fost șters."
                    : "Nu există un candidat cu acest ID.");
                ShowAllCandidates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la ștergere: " + ex.Message);
            }
        }

        private void cautaCandidat_Click(object sender, EventArgs e)
        {
            if (alegeCriteriu.SelectedItem == null)
            {
                MessageBox.Show("Selectează un criteriu.");
                return;
            }

            string criterion = alegeCriteriu.SelectedItem.ToString();
            string value = valoare_criteriu.Text.Trim();
            string query;
            IDbDataParameter[] parameters = new IDbDataParameter[0];

            if (criterion == "CNP")
            {
                if (value.Length != 13 || !value.All(char.IsDigit))
                {
                    MessageBox.Show("Introdu un CNP valid, format din 13 cifre.");
                    return;
                }

                query = "SELECT * FROM Candidati WHERE CNP = @Valoare";
                parameters = new IDbDataParameter[]
                {
                    DatabaseManager.CreateParameter("@Valoare", value)
                };
            }
            else if (criterion == "ID")
            {
                int id;
                if (!int.TryParse(value, out id) || id <= 0)
                {
                    MessageBox.Show("Introdu un ID numeric valid.");
                    return;
                }

                query = "SELECT * FROM Candidati WHERE IdCandidat = @Valoare";
                parameters = new IDbDataParameter[]
                {
                    DatabaseManager.CreateParameter("@Valoare", id)
                };
            }
            else
            {
                string status = criterion.Replace("Status: ", string.Empty);
                query = "SELECT * FROM Candidati WHERE Status = @Valoare";
                parameters = new IDbDataParameter[]
                {
                    DatabaseManager.CreateParameter("@Valoare", status)
                };
            }

            try
            {
                dataGridView1.DataSource = DatabaseManager.ExecuteQuery(query, parameters);
                dataGridView1.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la căutare: " + ex.Message);
            }
        }

        private void modificaCandidat_Click(object sender, EventArgs e)
        {
            int candidateId;
            if (!int.TryParse(cod_unic_pt_modificare.Text.Trim(), out candidateId) ||
                candidateId <= 0 || alegeStatut.SelectedItem == null)
            {
                MessageBox.Show("Introdu un ID valid și selectează noul statut.");
                return;
            }

            try
            {
                bool updated = DatabaseManager.UpdateCandidateStatus(
                    candidateId, alegeStatut.SelectedItem.ToString());
                MessageBox.Show(updated
                    ? "Statusul a fost actualizat cu succes."
                    : "Nu există un candidat cu acest ID.");
                ShowAllCandidates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la actualizare: " + ex.Message);
            }
        }

        private void startCandidateAdmitionSequence_Click(object sender, EventArgs e)
        {
            try
            {
                int admittedCount = DatabaseManager.RunAdmission();
                MessageBox.Show(
                    "Repartizarea a fost refăcută cu succes.\nNumăr candidați admiși: " +
                    admittedCount,
                    "Admitere finalizată", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowAdmittedCandidates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la rularea admiterii: " + ex.Message,
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAdmittedCandidates()
        {
            const string query = @"
                SELECT
                    C.IdCandidat,
                    C.Nume,
                    C.Prenume,
                    F.NumeFacultate AS Facultate,
                    S.NumeSpecializare AS Specializare,
                    ROUND(C.MedieLiceu * 0.3 + C.MedieBAC * 0.7, 2)
                        AS MedieFinala
                FROM AdmitereFinala A
                INNER JOIN Candidati C ON A.IdCandidat = C.IdCandidat
                INNER JOIN Specializari S ON A.IdSpecializare = S.IdSpecializare
                INNER JOIN Facultati F ON S.IdFacultate = F.IdFacultate
                ORDER BY MedieFinala DESC";

            try
            {
                dataGridView1.DataSource = DatabaseManager.ExecuteQuery(query);
                dataGridView1.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la afișarea candidaților admiși: " + ex.Message);
            }
        }

        private void arata_candidati_admisi_Click(object sender, EventArgs e)
        {
            ShowAdmittedCandidates();
        }

        private void reseteaza_lista_candidati_admisi_Click(object sender, EventArgs e)
        {
            DialogResult confirmation = MessageBox.Show(
                "Resetezi rezultatele și readuci toate statusurile la „Nedefinit”?",
                "Confirmare resetare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmation != DialogResult.Yes)
                return;

            try
            {
                DatabaseManager.ResetAdmission();
                MessageBox.Show("Lista candidaților admiși a fost resetată.");
                ShowAllCandidates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la resetare: " + ex.Message);
            }
        }

        private void prevForm_Click(object sender, EventArgs e)
        {
            questioningForm.Show();
            Close();
        }

        private void FormName_FormClosed(object sender, FormClosedEventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void cod_unic_pt_eliminare_TextChanged(object sender, EventArgs e) { }
        private void cod_unic_pt_modificare_TextChanged(object sender, EventArgs e) { }
        private void alegeCriteriu_SelectedIndexChanged(object sender, EventArgs e) { }
        private void valoare_criteriu_TextChanged(object sender, EventArgs e) { }
        private void alegeStatut_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
