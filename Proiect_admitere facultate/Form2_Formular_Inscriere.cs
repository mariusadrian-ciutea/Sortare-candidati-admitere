using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proiect_admitere_facultate
{
    public partial class Registration_form : AeroForm
    {
        private Questioning questioningForm;



        public Registration_form(Questioning formTrimis)
        {
            InitializeComponent();
            questioningForm = formTrimis;   //practic este o variabila care tine minte o referinta la formu-ul parinte
            AeroTheme.ApplyRegistration(this, label1, label2, label3, label4, label5, label6,
                label7, label8, Nume, Prenume, Adresa, Varsta, Sex, CNP, medieBAC,
                medieLiceu, prevForm, nextForm);
            FormClosed += delegate
            {
                if (!questioningForm.IsDisposed && !questioningForm.Visible)
                    questioningForm.Show();
            };
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Registration_form_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void FormName_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Nume_TextChanged(object sender, EventArgs e)
        {

        }

        private void Prenume_TextChanged(object sender, EventArgs e)
        {

        }

        private void Adresa_TextChanged(object sender, EventArgs e)
        {

        }

        private void Varsta_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Sex_Enter(object sender, EventArgs e)
        {

        }

        private void Masculin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Feminin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CNP_TextChanged(object sender, EventArgs e)
        {

        }

        private void medieBAC_TextChanged(object sender, EventArgs e)
        {

        }

        private void medieLiceu_TextChanged(object sender, EventArgs e)
        {

        }

        private void nextForm_Click(object sender, EventArgs e)
        {
            string nume = Nume.Text;
            string prenume = Prenume.Text;
            string cnp = CNP.Text;
            string adresa = Adresa.Text;
            int varsta = (int)Varsta.Value;
            string sex = "";

            if (Masculin.Checked)
                sex = "Masculin";
            else if (Feminin.Checked)
                sex = "Feminin";

           


            string medieBacText = medieBAC.Text;
            string medieLiceuText = medieLiceu.Text;

            //serie de verificari (campurile trebuie s fie ok inainte sa dam proceed)
            if (string.IsNullOrWhiteSpace(nume))
            {
                MessageBox.Show("Numele nu poate fi gol.");
                return;
            }

            if (nume.Any(char.IsDigit))
            {
                MessageBox.Show("Numele nu poate conține cifre.");
                return;
            }

            if (string.IsNullOrWhiteSpace(prenume))
            {
                MessageBox.Show("Prenumele nu poate fi gol.");
                return;
            }


            if (prenume.Any(char.IsDigit))
            {
                MessageBox.Show("Prenumele nu poate conține cifre.");
                return;
            }

            if (string.IsNullOrWhiteSpace(adresa))
            {
                MessageBox.Show("Adresa nu poate fi goală.");
                return;
            }

            if (Masculin.Checked != true && Feminin.Checked != true)
            {
                MessageBox.Show("Te rugăm să selectezi sexul.");
                return;
            }

            if (varsta <= 0)
            {
                MessageBox.Show("Vârsta trebuie să fie pozitivă.");
                return;
            }

            if (cnp.Length != 13)
            {
                MessageBox.Show("CNP-ul trebuie să aibă exact 13 caractere.");
                return;
            }

            if (!cnp.All(char.IsDigit))
            {
                MessageBox.Show("CNP-ul trebuie să conțină doar cifre.");
                return;
            }

            if (string.IsNullOrWhiteSpace(medieBacText) || string.IsNullOrWhiteSpace(medieLiceuText))
            {
                MessageBox.Show("Completează ambele medii.");
                return;
            }


            double medieLiceuVal;
            double medieBACVal;
            if (!TryParseGrade(medieLiceuText, out medieLiceuVal) ||
                !TryParseGrade(medieBacText, out medieBACVal))
            {
                MessageBox.Show("Mediile trebuie să fie numere între 1 și 10 (exemplu: 9,50).");
                return;
            }

            Candidat candidat = new Candidat
            {
                Nume = nume,
                Prenume = prenume,
                Adresa = adresa,
                Varsta = varsta,
                Sex = sex,
                CNP = cnp,
                MedieLiceu = medieLiceuVal,
                MedieBAC = medieBACVal,
                Status = "Nedefinit"
            };

            Form3_choice nextForm = new Form3_choice(questioningForm, candidat, this);
            nextForm.Show();
            this.Hide();
        }


        

        private void prevForm_Click_1(object sender, EventArgs e)
        {
            questioningForm.Show();
            this.Close();
        }

        private static bool TryParseGrade(string text, out double value)
        {
            bool parsed = double.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
                || double.TryParse(text.Replace(',', '.'), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out value);
            return parsed && value >= 1 && value <= 10;
        }
    }

        
    
}
