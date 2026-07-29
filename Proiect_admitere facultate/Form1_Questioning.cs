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
    public partial class Questioning : AeroForm
    {
        public Questioning()
        {
            InitializeComponent();
            AeroTheme.ApplyHome(this, label1, label2, sendToRegistration, sendToAdministring);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void sendToRegistration_Click(object sender, EventArgs e)
        {
            this.Hide(); 
            Registration_form regForm = new Registration_form(this);
            regForm.Show();
        }

        private void sendToAdministring_Click(object sender, EventArgs e)
        {
            this.Hide(); 
            Form4_Administration adminForm = new Form4_Administration(this);
            adminForm.Show();
        }

        private void showStatusToCandidate_Click(object sender, EventArgs e)
        {

        }
    }
}
