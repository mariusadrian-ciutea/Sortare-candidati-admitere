using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proiect_admitere_facultate
{
    public partial class Form3_choice : AeroForm
    {
        private Questioning questioningForm;
        private Candidat candidatCurent;
        private Registration_form registrationForm;


       
       

        public Form3_choice(Questioning formTrimis, Candidat c, Registration_form regForm)
        {
            InitializeComponent();
            questioningForm = formTrimis;
            candidatCurent = c;
            registrationForm = regForm;
            AeroTheme.ApplyChoices(this, label3, label1, label2, facultate_choice,
                specializare_choice, choices_by_importance, adaugaOptiune,
                eliminaUltimaOptiune, confirmaAlegerile, prevForm, înapoiLaPrimaPagina);
            FormClosed += delegate
            {
                if (!questioningForm.IsDisposed && !questioningForm.Visible)
                    questioningForm.Show();
            };
        }


        //private void button5_Click(object sender, EventArgs e)
        //{

        //}

        //private void label3_Click(object sender, EventArgs e)
        //{

        //}

        //private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        //{

        //}

        //private void FormName_FormClosed(object sender, FormClosedEventArgs e)
        //{
            
        //}

        private void Form3_choice_Load(object sender, EventArgs e)
        {
            facultate_choice.Items.Add("Facultatea de Cibernetică, Statistică și Informatică Economică");
            facultate_choice.Items.Add("Facultatea de Management");
            facultate_choice.Items.Add("Facultatea de Contabilitate și Informatică de Gestiune");
            facultate_choice.Items.Add("Facultatea de Marketing");
            facultate_choice.Items.Add("Facultatea de Finanțe, Asigurări, Bănci și Burse de Valori");
            facultate_choice.Items.Add("Facultatea de Relații Economice Internaționale");
            facultate_choice.Items.Add("Facultatea de Economie Teoretică și Aplicată");

        }
        /*
                private void Form3_choice_Load_1(object sender, EventArgs e)
                {

                }
        */

        private void confirmaAlegerile_Click(object sender, EventArgs e)
        {
            if (choices_by_importance.Rows.Count == 0)
            {
                MessageBox.Show("Trebuie să adaugi cel puțin o opțiune înainte de a confirma.");
                return;
            }


            List<string> specializari = new List<string>();
            foreach (DataGridViewRow row in choices_by_importance.Rows)
            {
                string numeSpecializare = row.Cells[2].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(numeSpecializare))
                    specializari.Add(numeSpecializare);
            }

            try
            {
                int idCandidat = DatabaseManager.SaveApplication(candidatCurent, specializari);
                MessageBox.Show(
                    "Înscrierea a fost salvată cu succes.\nCodul tău unic este " + idCandidat + ".",
                    "Înscriere finalizată", MessageBoxButtons.OK, MessageBoxIcon.Information);
                questioningForm.Show();
                registrationForm.Close();
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message,
                    "Înscriere nereușită", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Înscrierea nu a putut fi salvată:\n" + ex.Message,
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void înapoiLaPrimaPagina_Click(object sender, EventArgs e)
        {
            questioningForm.Show(); 
            this.Close();
        }

        private void prevForm_Click(object sender, EventArgs e)
        {
            registrationForm.Show();
            this.Hide();
        }

        private void facultate_choice_SelectedIndexChanged(object sender, EventArgs e)
        {

           //basically, la introducerea (de mai sus) a elmentelor in combox, astea au fost introduse intr-o anumita ordine (si au fost indexate). in switch, fiecare case este un index din combox

            specializare_choice.Items.Clear();   //goleste lista ca mai apoi sa o umplem din nou
            switch (facultate_choice.SelectedIndex)    //selected index - proprietate care spune pozitia elementului selectat in lista (spune indexul)
            {
                case 0: 

                    specializare_choice.Items.Add("Cibernetică Economică");
                    specializare_choice.Items.Add("Informatică Economică");
                    specializare_choice.Items.Add("Statistică economică și data science");
                    break;

                case 1:
                    specializare_choice.Items.Add("Management");
                    specializare_choice.Items.Add("Management (în limba engleză)");
                    break;
                case 2: 
                    specializare_choice.Items.Add("Contabilitate și Informatică de Gestiune");
                    specializare_choice.Items.Add("Contabilitate și Informatică de Gestiune (în limba engleză)");
                    break;
                case 3:
                    specializare_choice.Items.Add("Marketing");
                    specializare_choice.Items.Add("Marketing (în limba engleză)");
                    break;

                case 4: 
                    specializare_choice.Items.Add("Finanțe și Bănci");
                    specializare_choice.Items.Add("Finanțe și Bănci (în limba engleză)");
                    break;

                case 5: 
                    specializare_choice.Items.Add("Economie și afaceri internaționale");
                    specializare_choice.Items.Add("Economie și afaceri internaționale (în limba engleză)");
                    break;

                case 6: 
                    specializare_choice.Items.Add("Limbi moderne aplicate (engleză, franceză)");
                    specializare_choice.Items.Add("Economie și comunicare economică în afaceri");
                    break;
            }
        }

        private void specializare_choice_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void choices_by_importance_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void adaugaOptiune_Click(object sender, EventArgs e)
        {

            if (choices_by_importance.Rows.Count >= 3)
            {
                MessageBox.Show("Poți adăuga maximum 3 opțiuni.");
                return;
            }
            if (facultate_choice.SelectedItem == null || specializare_choice.SelectedItem == null)
            {
                MessageBox.Show("Selectează o facultate și o specializare.");
                return;
            }

            string facultate = (string)facultate_choice.SelectedItem;
            string specializare = (string)specializare_choice.SelectedItem;


            foreach (DataGridViewRow row in choices_by_importance.Rows)   //pt comparare. merg prin toate randurile din grid si ma uit sa vad ca nu cumva vreo inregistrare sa fie la fel cu ce incercam sa bagam acum
            {
                //neaparat cu ? pt protectie in caz de null (returneaza null in loc sa dea eroare)
                if (row.Cells[1].Value?.ToString() == facultate && row.Cells[2].Value?.ToString() == specializare)
                {
                    MessageBox.Show("Această opțiune a fost deja adăugată.");
                    return;
                }
            }

            
            int prioritate = choices_by_importance.Rows.Count + 1;

            
            choices_by_importance.Rows.Add(prioritate, facultate, specializare);
        }

        private void eliminaUltimaOptiune_Click(object sender, EventArgs e)
        {
            int totalRanduri = choices_by_importance.Rows.Count;

            if (totalRanduri == 0)
            {
                MessageBox.Show("Nu există opțiuni de eliminat.");
                return;
            }

            
            choices_by_importance.Rows.RemoveAt(totalRanduri - 1);
        }
    }
}
