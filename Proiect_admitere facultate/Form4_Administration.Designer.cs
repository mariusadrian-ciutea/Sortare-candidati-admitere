namespace Proiect_admitere_facultate
{
    partial class Form4_Administration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.afiseaza_studenti = new System.Windows.Forms.Button();
            this.startCandidateAdmitionSequence = new System.Windows.Forms.Button();
            this.arata_candidati_admisi = new System.Windows.Forms.Button();
            this.reseteaza_lista_candidati_admisi = new System.Windows.Forms.Button();
            this.prevForm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cod_unic_pt_eliminare = new System.Windows.Forms.TextBox();
            this.stergeCandidat = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.alegeStatut = new System.Windows.Forms.ComboBox();
            this.modificaCandidat = new System.Windows.Forms.Button();
            this.cod_unic_pt_modificare = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.alegeCriteriu = new System.Windows.Forms.ComboBox();
            this.valoare_criteriu = new System.Windows.Forms.TextBox();
            this.cautaCandidat = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 32);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1070, 359);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // afiseaza_studenti
            // 
            this.afiseaza_studenti.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.afiseaza_studenti.Location = new System.Drawing.Point(12, 403);
            this.afiseaza_studenti.Name = "afiseaza_studenti";
            this.afiseaza_studenti.Size = new System.Drawing.Size(351, 33);
            this.afiseaza_studenti.TabIndex = 1;
            this.afiseaza_studenti.Text = "Afișează lista cu toți studenții și alegerile lor";
            this.afiseaza_studenti.UseVisualStyleBackColor = true;
            this.afiseaza_studenti.Click += new System.EventHandler(this.afiseaza_studenti_Click);
            // 
            // startCandidateAdmitionSequence
            // 
            this.startCandidateAdmitionSequence.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startCandidateAdmitionSequence.Location = new System.Drawing.Point(775, 630);
            this.startCandidateAdmitionSequence.Name = "startCandidateAdmitionSequence";
            this.startCandidateAdmitionSequence.Size = new System.Drawing.Size(307, 33);
            this.startCandidateAdmitionSequence.TabIndex = 2;
            this.startCandidateAdmitionSequence.Text = "Porniți secvența de admitere a candidaților\r\n";
            this.startCandidateAdmitionSequence.UseVisualStyleBackColor = true;
            this.startCandidateAdmitionSequence.Click += new System.EventHandler(this.startCandidateAdmitionSequence_Click);
            // 
            // arata_candidati_admisi
            // 
            this.arata_candidati_admisi.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.arata_candidati_admisi.Location = new System.Drawing.Point(775, 669);
            this.arata_candidati_admisi.Name = "arata_candidati_admisi";
            this.arata_candidati_admisi.Size = new System.Drawing.Size(307, 33);
            this.arata_candidati_admisi.TabIndex = 3;
            this.arata_candidati_admisi.Text = "Afișează candidații admiși";
            this.arata_candidati_admisi.UseVisualStyleBackColor = true;
            this.arata_candidati_admisi.Click += new System.EventHandler(this.arata_candidati_admisi_Click);
            // 
            // reseteaza_lista_candidati_admisi
            // 
            this.reseteaza_lista_candidati_admisi.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reseteaza_lista_candidati_admisi.Location = new System.Drawing.Point(775, 708);
            this.reseteaza_lista_candidati_admisi.Name = "reseteaza_lista_candidati_admisi";
            this.reseteaza_lista_candidati_admisi.Size = new System.Drawing.Size(307, 33);
            this.reseteaza_lista_candidati_admisi.TabIndex = 4;
            this.reseteaza_lista_candidati_admisi.Text = "Resetați lista candidaților admiși";
            this.reseteaza_lista_candidati_admisi.UseVisualStyleBackColor = true;
            this.reseteaza_lista_candidati_admisi.Click += new System.EventHandler(this.reseteaza_lista_candidati_admisi_Click);
            // 
            // prevForm
            // 
            this.prevForm.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.prevForm.Location = new System.Drawing.Point(31, 718);
            this.prevForm.Name = "prevForm";
            this.prevForm.Size = new System.Drawing.Size(75, 23);
            this.prevForm.TabIndex = 5;
            this.prevForm.Text = "<-prev";
            this.prevForm.UseVisualStyleBackColor = true;
            this.prevForm.Click += new System.EventHandler(this.prevForm_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(723, 411);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(359, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Eliminați un candidat din competiție pe baza codului său unic";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // cod_unic_pt_eliminare
            // 
            this.cod_unic_pt_eliminare.Location = new System.Drawing.Point(915, 446);
            this.cod_unic_pt_eliminare.Name = "cod_unic_pt_eliminare";
            this.cod_unic_pt_eliminare.Size = new System.Drawing.Size(81, 20);
            this.cod_unic_pt_eliminare.TabIndex = 7;
            this.cod_unic_pt_eliminare.TextChanged += new System.EventHandler(this.cod_unic_pt_eliminare_TextChanged);
            // 
            // stergeCandidat
            // 
            this.stergeCandidat.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stergeCandidat.Location = new System.Drawing.Point(1019, 433);
            this.stergeCandidat.Name = "stergeCandidat";
            this.stergeCandidat.Size = new System.Drawing.Size(63, 33);
            this.stergeCandidat.TabIndex = 8;
            this.stergeCandidat.Text = "Delete";
            this.stergeCandidat.UseVisualStyleBackColor = true;
            this.stergeCandidat.Click += new System.EventHandler(this.stergeCandidat_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 610);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(450, 32);
            this.label2.TabIndex = 9;
            this.label2.Text = "Modificați statutul pe baza ID-ului (de preferat după ce s-a realizat admiterea)\r" +
    "\n\r\n";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // alegeStatut
            // 
            this.alegeStatut.FormattingEnabled = true;
            this.alegeStatut.Location = new System.Drawing.Point(182, 642);
            this.alegeStatut.Name = "alegeStatut";
            this.alegeStatut.Size = new System.Drawing.Size(106, 21);
            this.alegeStatut.TabIndex = 10;
            this.alegeStatut.SelectedIndexChanged += new System.EventHandler(this.alegeStatut_SelectedIndexChanged);
            // 
            // modificaCandidat
            // 
            this.modificaCandidat.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modificaCandidat.Location = new System.Drawing.Point(289, 643);
            this.modificaCandidat.Name = "modificaCandidat";
            this.modificaCandidat.Size = new System.Drawing.Size(63, 33);
            this.modificaCandidat.TabIndex = 11;
            this.modificaCandidat.Text = "Modify";
            this.modificaCandidat.UseVisualStyleBackColor = true;
            this.modificaCandidat.Click += new System.EventHandler(this.modificaCandidat_Click);
            // 
            // cod_unic_pt_modificare
            // 
            this.cod_unic_pt_modificare.Location = new System.Drawing.Point(12, 643);
            this.cod_unic_pt_modificare.Name = "cod_unic_pt_modificare";
            this.cod_unic_pt_modificare.Size = new System.Drawing.Size(164, 20);
            this.cod_unic_pt_modificare.TabIndex = 12;
            this.cod_unic_pt_modificare.TextChanged += new System.EventHandler(this.cod_unic_pt_modificare_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 489);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(296, 16);
            this.label3.TabIndex = 13;
            this.label3.Text = "Afișează candidații pe baza unui criteriu la alegere";
            // 
            // alegeCriteriu
            // 
            this.alegeCriteriu.FormattingEnabled = true;
            this.alegeCriteriu.Location = new System.Drawing.Point(12, 525);
            this.alegeCriteriu.Name = "alegeCriteriu";
            this.alegeCriteriu.Size = new System.Drawing.Size(149, 21);
            this.alegeCriteriu.TabIndex = 14;
            this.alegeCriteriu.SelectedIndexChanged += new System.EventHandler(this.alegeCriteriu_SelectedIndexChanged);
            // 
            // valoare_criteriu
            // 
            this.valoare_criteriu.Location = new System.Drawing.Point(182, 526);
            this.valoare_criteriu.Name = "valoare_criteriu";
            this.valoare_criteriu.Size = new System.Drawing.Size(126, 20);
            this.valoare_criteriu.TabIndex = 15;
            this.valoare_criteriu.TextChanged += new System.EventHandler(this.valoare_criteriu_TextChanged);
            // 
            // cautaCandidat
            // 
            this.cautaCandidat.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cautaCandidat.Location = new System.Drawing.Point(325, 526);
            this.cautaCandidat.Name = "cautaCandidat";
            this.cautaCandidat.Size = new System.Drawing.Size(63, 33);
            this.cautaCandidat.TabIndex = 16;
            this.cautaCandidat.Text = "Caută";
            this.cautaCandidat.UseVisualStyleBackColor = true;
            this.cautaCandidat.Click += new System.EventHandler(this.cautaCandidat_Click);
            // 
            // Form4_Administration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1094, 759);
            this.Controls.Add(this.cautaCandidat);
            this.Controls.Add(this.valoare_criteriu);
            this.Controls.Add(this.alegeCriteriu);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cod_unic_pt_modificare);
            this.Controls.Add(this.modificaCandidat);
            this.Controls.Add(this.alegeStatut);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.stergeCandidat);
            this.Controls.Add(this.cod_unic_pt_eliminare);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.prevForm);
            this.Controls.Add(this.reseteaza_lista_candidati_admisi);
            this.Controls.Add(this.arata_candidati_admisi);
            this.Controls.Add(this.startCandidateAdmitionSequence);
            this.Controls.Add(this.afiseaza_studenti);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form4_Administration";
            this.Text = "Control Panel";
            this.Load += new System.EventHandler(this.Form4_Administration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button afiseaza_studenti;
        private System.Windows.Forms.Button startCandidateAdmitionSequence;
        private System.Windows.Forms.Button arata_candidati_admisi;
        private System.Windows.Forms.Button reseteaza_lista_candidati_admisi;
        private System.Windows.Forms.Button prevForm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox cod_unic_pt_eliminare;
        private System.Windows.Forms.Button stergeCandidat;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox alegeStatut;
        private System.Windows.Forms.Button modificaCandidat;
        private System.Windows.Forms.TextBox cod_unic_pt_modificare;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox alegeCriteriu;
        private System.Windows.Forms.TextBox valoare_criteriu;
        private System.Windows.Forms.Button cautaCandidat;
    }
}