namespace Proiect_admitere_facultate
{
    partial class Questioning
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Questioning));
            this.label2 = new System.Windows.Forms.Label();
            this.sendToRegistration = new System.Windows.Forms.Button();
            this.sendToAdministring = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(731, 125);
            this.label2.TabIndex = 1;
            this.label2.Text = resources.GetString("label2.Text");
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // sendToRegistration
            // 
            this.sendToRegistration.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendToRegistration.Location = new System.Drawing.Point(217, 304);
            this.sendToRegistration.Name = "sendToRegistration";
            this.sendToRegistration.Size = new System.Drawing.Size(284, 71);
            this.sendToRegistration.TabIndex = 2;
            this.sendToRegistration.Text = "Formular de admitere";
            this.sendToRegistration.UseVisualStyleBackColor = true;
            this.sendToRegistration.Click += new System.EventHandler(this.sendToRegistration_Click);
            // 
            // sendToAdministring
            // 
            this.sendToAdministring.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sendToAdministring.Location = new System.Drawing.Point(534, 304);
            this.sendToAdministring.Name = "sendToAdministring";
            this.sendToAdministring.Size = new System.Drawing.Size(284, 71);
            this.sendToAdministring.TabIndex = 3;
            this.sendToAdministring.Text = "Panou Administrare";
            this.sendToAdministring.UseVisualStyleBackColor = true;
            this.sendToAdministring.Click += new System.EventHandler(this.sendToAdministring_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(455, 37);
            this.label1.TabIndex = 4;
            this.label1.Text = "Bine ai venit în aplicația de admitere!";
            // 
            // Questioning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1091, 519);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sendToAdministring);
            this.Controls.Add(this.sendToRegistration);
            this.Controls.Add(this.label2);
            this.Name = "Questioning";
            this.Text = "Questioning";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button sendToRegistration;
        private System.Windows.Forms.Button sendToAdministring;
        private System.Windows.Forms.Label label1;
    }
}

