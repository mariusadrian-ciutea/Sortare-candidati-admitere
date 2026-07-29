namespace Proiect_admitere_facultate
{
    partial class Form3_choice
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
            this.prevForm = new System.Windows.Forms.Button();
            this.facultate_choice = new System.Windows.Forms.ComboBox();
            this.specializare_choice = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.choices_by_importance = new System.Windows.Forms.DataGridView();
            this.eliminaUltimaOptiune = new System.Windows.Forms.Button();
            this.adaugaOptiune = new System.Windows.Forms.Button();
            this.confirmaAlegerile = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.înapoiLaPrimaPagina = new System.Windows.Forms.Button();
            this.Prioritate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coloanaFac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colanaSpec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.choices_by_importance)).BeginInit();
            this.SuspendLayout();
            // 
            // prevForm
            // 
            this.prevForm.Location = new System.Drawing.Point(33, 674);
            this.prevForm.Margin = new System.Windows.Forms.Padding(4);
            this.prevForm.Name = "prevForm";
            this.prevForm.Size = new System.Drawing.Size(115, 25);
            this.prevForm.TabIndex = 0;
            this.prevForm.Text = "<-prev";
            this.prevForm.UseVisualStyleBackColor = true;
            this.prevForm.Click += new System.EventHandler(this.prevForm_Click);
            // 
            // facultate_choice
            // 
            this.facultate_choice.FormattingEnabled = true;
            this.facultate_choice.Location = new System.Drawing.Point(130, 85);
            this.facultate_choice.Margin = new System.Windows.Forms.Padding(4);
            this.facultate_choice.Name = "facultate_choice";
            this.facultate_choice.Size = new System.Drawing.Size(299, 24);
            this.facultate_choice.TabIndex = 1;
            this.facultate_choice.SelectedIndexChanged += new System.EventHandler(this.facultate_choice_SelectedIndexChanged);
            // 
            // specializare_choice
            // 
            this.specializare_choice.FormattingEnabled = true;
            this.specializare_choice.Location = new System.Drawing.Point(130, 128);
            this.specializare_choice.Margin = new System.Windows.Forms.Padding(4);
            this.specializare_choice.Name = "specializare_choice";
            this.specializare_choice.Size = new System.Drawing.Size(299, 24);
            this.specializare_choice.TabIndex = 2;
            this.specializare_choice.SelectedIndexChanged += new System.EventHandler(this.specializare_choice_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 93);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Facultate:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 131);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Specializare:";
            // 
            // choices_by_importance
            // 
            this.choices_by_importance.AllowUserToAddRows = false;
            this.choices_by_importance.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.choices_by_importance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.choices_by_importance.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Prioritate,
            this.coloanaFac,
            this.colanaSpec});
            this.choices_by_importance.Location = new System.Drawing.Point(437, 41);
            this.choices_by_importance.Margin = new System.Windows.Forms.Padding(4);
            this.choices_by_importance.Name = "choices_by_importance";
            this.choices_by_importance.Size = new System.Drawing.Size(594, 160);
            this.choices_by_importance.TabIndex = 5;
            this.choices_by_importance.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.choices_by_importance_CellContentClick);
            // 
            // eliminaUltimaOptiune
            // 
            this.eliminaUltimaOptiune.Location = new System.Drawing.Point(228, 245);
            this.eliminaUltimaOptiune.Margin = new System.Windows.Forms.Padding(4);
            this.eliminaUltimaOptiune.Name = "eliminaUltimaOptiune";
            this.eliminaUltimaOptiune.Size = new System.Drawing.Size(172, 56);
            this.eliminaUltimaOptiune.TabIndex = 6;
            this.eliminaUltimaOptiune.Text = "Elimină ultima opțiune";
            this.eliminaUltimaOptiune.Click += new System.EventHandler(this.eliminaUltimaOptiune_Click);
            // 
            // adaugaOptiune
            // 
            this.adaugaOptiune.Location = new System.Drawing.Point(13, 245);
            this.adaugaOptiune.Margin = new System.Windows.Forms.Padding(4);
            this.adaugaOptiune.Name = "adaugaOptiune";
            this.adaugaOptiune.Size = new System.Drawing.Size(172, 52);
            this.adaugaOptiune.TabIndex = 7;
            this.adaugaOptiune.Text = "Adaugă opțiune";
            this.adaugaOptiune.Click += new System.EventHandler(this.adaugaOptiune_Click);
            // 
            // confirmaAlegerile
            // 
            this.confirmaAlegerile.Location = new System.Drawing.Point(859, 623);
            this.confirmaAlegerile.Margin = new System.Windows.Forms.Padding(4);
            this.confirmaAlegerile.Name = "confirmaAlegerile";
            this.confirmaAlegerile.Size = new System.Drawing.Size(172, 76);
            this.confirmaAlegerile.TabIndex = 8;
            this.confirmaAlegerile.Text = "Confirmă alegerile";
            this.confirmaAlegerile.Click += new System.EventHandler(this.confirmaAlegerile_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 53);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(218, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Alege facultatea și specializarea:";
            // 
            // înapoiLaPrimaPagina
            // 
            this.înapoiLaPrimaPagina.Location = new System.Drawing.Point(283, 674);
            this.înapoiLaPrimaPagina.Margin = new System.Windows.Forms.Padding(4);
            this.înapoiLaPrimaPagina.Name = "înapoiLaPrimaPagina";
            this.înapoiLaPrimaPagina.Size = new System.Drawing.Size(172, 25);
            this.înapoiLaPrimaPagina.TabIndex = 10;
            this.înapoiLaPrimaPagina.Text = "Înapoi la prima pagină";
            this.înapoiLaPrimaPagina.Click += new System.EventHandler(this.înapoiLaPrimaPagina_Click);
            // 
            // Prioritate
            // 
            this.Prioritate.HeaderText = "Prioritate";
            this.Prioritate.Name = "Prioritate";
            this.Prioritate.Width = 91;
            // 
            // coloanaFac
            // 
            this.coloanaFac.HeaderText = "Facultate";
            this.coloanaFac.Name = "coloanaFac";
            this.coloanaFac.Width = 91;
            // 
            // colanaSpec
            // 
            this.colanaSpec.HeaderText = "Specializare";
            this.colanaSpec.Name = "colanaSpec";
            this.colanaSpec.Width = 112;
            // 
            // Form3_choice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 724);
            this.Controls.Add(this.prevForm);
            this.Controls.Add(this.facultate_choice);
            this.Controls.Add(this.specializare_choice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.choices_by_importance);
            this.Controls.Add(this.eliminaUltimaOptiune);
            this.Controls.Add(this.adaugaOptiune);
            this.Controls.Add(this.confirmaAlegerile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.înapoiLaPrimaPagina);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form3_choice";
            this.Text = "Alegeri candidat";
            this.Load += new System.EventHandler(this.Form3_choice_Load);
            ((System.ComponentModel.ISupportInitialize)(this.choices_by_importance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Button prevForm;
        private System.Windows.Forms.ComboBox facultate_choice;
        private System.Windows.Forms.ComboBox specializare_choice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView choices_by_importance;
        private System.Windows.Forms.Button eliminaUltimaOptiune;
        private System.Windows.Forms.Button adaugaOptiune;
        private System.Windows.Forms.Button confirmaAlegerile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button înapoiLaPrimaPagina;
        private System.Windows.Forms.DataGridViewTextBoxColumn Prioritate;
        private System.Windows.Forms.DataGridViewTextBoxColumn coloanaFac;
        private System.Windows.Forms.DataGridViewTextBoxColumn colanaSpec;
    }
}