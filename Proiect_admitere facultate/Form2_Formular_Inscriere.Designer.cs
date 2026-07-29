namespace Proiect_admitere_facultate
{
    partial class Registration_form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Registration_form));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.prevForm = new System.Windows.Forms.Button();
            this.nextForm = new System.Windows.Forms.Button();
            this.Masculin = new System.Windows.Forms.RadioButton();
            this.Sex = new System.Windows.Forms.GroupBox();
            this.Feminin = new System.Windows.Forms.RadioButton();
            this.Varsta = new System.Windows.Forms.NumericUpDown();
            this.Nume = new System.Windows.Forms.TextBox();
            this.Prenume = new System.Windows.Forms.TextBox();
            this.Adresa = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.medieBAC = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CNP = new System.Windows.Forms.TextBox();
            this.medieLiceu = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Sex.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Varsta)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1003, 64);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(25, 143);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Nume ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(375, 143);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Prenume";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(31, 275);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Varsta";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(722, 143);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Adresa de domiciliu";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // prevForm
            // 
            this.prevForm.Location = new System.Drawing.Point(34, 643);
            this.prevForm.Name = "prevForm";
            this.prevForm.Size = new System.Drawing.Size(75, 23);
            this.prevForm.TabIndex = 6;
            this.prevForm.Text = "<-prev";
            this.prevForm.UseVisualStyleBackColor = true;
            this.prevForm.Click += new System.EventHandler(this.prevForm_Click_1);
            // 
            // nextForm
            // 
            this.nextForm.Location = new System.Drawing.Point(981, 643);
            this.nextForm.Name = "nextForm";
            this.nextForm.Size = new System.Drawing.Size(75, 23);
            this.nextForm.TabIndex = 7;
            this.nextForm.Text = "next->";
            this.nextForm.UseVisualStyleBackColor = true;
            this.nextForm.Click += new System.EventHandler(this.nextForm_Click);
            // 
            // Masculin
            // 
            this.Masculin.AutoSize = true;
            this.Masculin.Location = new System.Drawing.Point(16, 32);
            this.Masculin.Name = "Masculin";
            this.Masculin.Size = new System.Drawing.Size(81, 20);
            this.Masculin.TabIndex = 8;
            this.Masculin.TabStop = true;
            this.Masculin.Text = "Masculin";
            this.Masculin.UseVisualStyleBackColor = true;
            this.Masculin.CheckedChanged += new System.EventHandler(this.Masculin_CheckedChanged);
            // 
            // Sex
            // 
            this.Sex.Controls.Add(this.Feminin);
            this.Sex.Controls.Add(this.Masculin);
            this.Sex.Location = new System.Drawing.Point(378, 275);
            this.Sex.Name = "Sex";
            this.Sex.Size = new System.Drawing.Size(261, 85);
            this.Sex.TabIndex = 9;
            this.Sex.TabStop = false;
            this.Sex.Text = "Sex";
            this.Sex.Enter += new System.EventHandler(this.Sex_Enter);
            // 
            // Feminin
            // 
            this.Feminin.AutoSize = true;
            this.Feminin.Location = new System.Drawing.Point(145, 32);
            this.Feminin.Name = "Feminin";
            this.Feminin.Size = new System.Drawing.Size(77, 20);
            this.Feminin.TabIndex = 10;
            this.Feminin.TabStop = true;
            this.Feminin.Text = "Feminin";
            this.Feminin.UseVisualStyleBackColor = true;
            this.Feminin.CheckedChanged += new System.EventHandler(this.Feminin_CheckedChanged);
            // 
            // Varsta
            // 
            this.Varsta.Location = new System.Drawing.Point(32, 313);
            this.Varsta.Name = "Varsta";
            this.Varsta.Size = new System.Drawing.Size(120, 22);
            this.Varsta.TabIndex = 10;
            this.Varsta.ValueChanged += new System.EventHandler(this.Varsta_ValueChanged);
            // 
            // Nume
            // 
            this.Nume.Location = new System.Drawing.Point(28, 170);
            this.Nume.Name = "Nume";
            this.Nume.Size = new System.Drawing.Size(135, 22);
            this.Nume.TabIndex = 11;
            this.Nume.TextChanged += new System.EventHandler(this.Nume_TextChanged);
            // 
            // Prenume
            // 
            this.Prenume.Location = new System.Drawing.Point(378, 171);
            this.Prenume.Name = "Prenume";
            this.Prenume.Size = new System.Drawing.Size(222, 22);
            this.Prenume.TabIndex = 12;
            this.Prenume.TextChanged += new System.EventHandler(this.Prenume_TextChanged);
            // 
            // Adresa
            // 
            this.Adresa.Location = new System.Drawing.Point(725, 171);
            this.Adresa.Name = "Adresa";
            this.Adresa.Size = new System.Drawing.Size(294, 22);
            this.Adresa.TabIndex = 13;
            this.Adresa.TextChanged += new System.EventHandler(this.Adresa_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(25, 453);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(250, 16);
            this.label6.TabIndex = 14;
            this.label6.Text = "Media de la examenul de bacalaureat";
            // 
            // medieBAC
            // 
            this.medieBAC.Location = new System.Drawing.Point(28, 493);
            this.medieBAC.Name = "medieBAC";
            this.medieBAC.Size = new System.Drawing.Size(118, 22);
            this.medieBAC.TabIndex = 15;
            this.medieBAC.TextChanged += new System.EventHandler(this.medieBAC_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(722, 275);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 16);
            this.label7.TabIndex = 16;
            this.label7.Text = "CNP";
            // 
            // CNP
            // 
            this.CNP.Location = new System.Drawing.Point(725, 305);
            this.CNP.Name = "CNP";
            this.CNP.Size = new System.Drawing.Size(145, 22);
            this.CNP.TabIndex = 17;
            this.CNP.TextChanged += new System.EventHandler(this.CNP_TextChanged);
            // 
            // medieLiceu
            // 
            this.medieLiceu.Location = new System.Drawing.Point(378, 493);
            this.medieLiceu.Name = "medieLiceu";
            this.medieLiceu.Size = new System.Drawing.Size(118, 22);
            this.medieLiceu.TabIndex = 18;
            this.medieLiceu.TextChanged += new System.EventHandler(this.medieLiceu_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(375, 453);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 16);
            this.label8.TabIndex = 19;
            this.label8.Text = "Medie liceu";
            // 
            // Registration_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 695);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.medieLiceu);
            this.Controls.Add(this.CNP);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.medieBAC);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.Adresa);
            this.Controls.Add(this.Prenume);
            this.Controls.Add(this.Nume);
            this.Controls.Add(this.Varsta);
            this.Controls.Add(this.Sex);
            this.Controls.Add(this.nextForm);
            this.Controls.Add(this.prevForm);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Registration_form";
            this.Text = "Înscriere";
            this.Load += new System.EventHandler(this.Registration_form_Load);
            this.Sex.ResumeLayout(false);
            this.Sex.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Varsta)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button prevForm;
        private System.Windows.Forms.Button nextForm;
        private System.Windows.Forms.RadioButton Masculin;
        private System.Windows.Forms.GroupBox Sex;
        private System.Windows.Forms.RadioButton Feminin;
        private System.Windows.Forms.NumericUpDown Varsta;
        private System.Windows.Forms.TextBox Nume;
        private System.Windows.Forms.TextBox Prenume;
        private System.Windows.Forms.TextBox Adresa;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox medieBAC;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox CNP;
        private System.Windows.Forms.TextBox medieLiceu;
        private System.Windows.Forms.Label label8;
    }
}