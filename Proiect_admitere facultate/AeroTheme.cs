using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Proiect_admitere_facultate
{
    public class AeroForm : Form
    {
        public AeroForm()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(210, 241, 255);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle bounds = ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (LinearGradientBrush sky = new LinearGradientBrush(
                bounds, Color.FromArgb(91, 195, 244), Color.FromArgb(237, 252, 255), 90f))
            {
                e.Graphics.FillRectangle(sky, bounds);
            }

            using (SolidBrush sunGlow = new SolidBrush(Color.FromArgb(65, 255, 250, 170)))
                e.Graphics.FillEllipse(sunGlow, bounds.Width - 230, 38, 150, 150);
            using (SolidBrush sun = new SolidBrush(Color.FromArgb(255, 239, 104)))
                e.Graphics.FillEllipse(sun, bounds.Width - 200, 68, 90, 90);

            DrawCloud(e.Graphics, 80, 62, 1.0f);
            DrawCloud(e.Graphics, bounds.Width - 500, 165, 0.72f);

            int horizon = Math.Max(360, bounds.Height - 185);
            using (GraphicsPath backHill = new GraphicsPath())
            {
                backHill.AddBezier(-80, horizon + 45, bounds.Width * 0.25f, horizon - 80,
                    bounds.Width * 0.67f, horizon + 35, bounds.Width + 80, horizon - 35);
                backHill.AddLine(bounds.Width + 80, bounds.Height, -80, bounds.Height);
                backHill.CloseFigure();
                using (LinearGradientBrush hillBrush = new LinearGradientBrush(
                    new Rectangle(0, horizon - 80, bounds.Width, bounds.Height - horizon + 80),
                    Color.FromArgb(139, 220, 74), Color.FromArgb(44, 154, 78), 90f))
                    e.Graphics.FillPath(hillBrush, backHill);
            }

            using (GraphicsPath frontHill = new GraphicsPath())
            {
                frontHill.AddBezier(-50, horizon + 110, bounds.Width * 0.35f, horizon - 5,
                    bounds.Width * 0.72f, horizon + 150, bounds.Width + 50, horizon + 50);
                frontHill.AddLine(bounds.Width + 50, bounds.Height, -50, bounds.Height);
                frontHill.CloseFigure();
                using (SolidBrush grass = new SolidBrush(Color.FromArgb(44, 174, 74)))
                    e.Graphics.FillPath(grass, frontHill);
            }

            using (SolidBrush bubble = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
            using (Pen bubbleLine = new Pen(Color.FromArgb(100, 255, 255, 255), 2f))
            {
                e.Graphics.FillEllipse(bubble, 28, bounds.Height - 130, 48, 48);
                e.Graphics.DrawEllipse(bubbleLine, 28, bounds.Height - 130, 48, 48);
                e.Graphics.FillEllipse(bubble, bounds.Width - 88, bounds.Height - 238, 27, 27);
                e.Graphics.DrawEllipse(bubbleLine, bounds.Width - 88, bounds.Height - 238, 27, 27);
            }
        }

        private static void DrawCloud(Graphics graphics, int x, int y, float scale)
        {
            using (SolidBrush cloud = new SolidBrush(Color.FromArgb(190, 255, 255, 255)))
            {
                graphics.FillEllipse(cloud, x, y + (int)(18 * scale), (int)(86 * scale), (int)(38 * scale));
                graphics.FillEllipse(cloud, x + (int)(20 * scale), y, (int)(54 * scale), (int)(54 * scale));
                graphics.FillEllipse(cloud, x + (int)(53 * scale), y + (int)(10 * scale), (int)(55 * scale), (int)(45 * scale));
            }
        }
    }

    internal sealed class AeroCard : Panel
    {
        public AeroCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Padding = new Padding(22);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(1, 1, Width - 3, Height - 3);
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            using (GraphicsPath path = AeroTheme.RoundedRectangle(rect, 22))
            using (LinearGradientBrush fill = new LinearGradientBrush(
                rect, Color.FromArgb(242, 255, 255, 255), Color.FromArgb(215, 235, 251, 255), 90f))
            using (Pen border = new Pen(Color.FromArgb(180, 255, 255, 255), 2f))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }

            base.OnPaint(e);
        }
    }

    internal static class AeroTheme
    {
        private static readonly Color Ink = Color.FromArgb(24, 71, 91);
        private static readonly Color MutedInk = Color.FromArgb(67, 111, 127);
        private static readonly Color Green = Color.FromArgb(38, 171, 91);
        private static readonly Color GreenHover = Color.FromArgb(30, 145, 76);
        private static readonly Color Blue = Color.FromArgb(28, 142, 207);
        private static readonly Color BlueHover = Color.FromArgb(19, 119, 181);
        private static readonly Color Coral = Color.FromArgb(231, 98, 86);
        private static readonly Color CoralHover = Color.FromArgb(205, 72, 64);

        public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void ConfigureForm(Form form, string title, Size size)
        {
            form.AutoScaleMode = AutoScaleMode.None;
            form.Text = title;
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            form.ClientSize = size;
            form.MinimumSize = new Size(size.Width + 16, size.Height + 39);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ForeColor = Ink;
        }

        private static Label AddLabel(Control parent, string text, Rectangle bounds, float size,
            FontStyle style, Color color)
        {
            Label label = new Label
            {
                Text = text,
                Location = bounds.Location,
                Size = bounds.Size,
                AutoSize = false,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(label);
            label.BringToFront();
            return label;
        }

        private static AeroCard AddCard(Control parent, Rectangle bounds)
        {
            AeroCard card = new AeroCard { Bounds = bounds };
            parent.Controls.Add(card);
            card.BringToFront();
            return card;
        }

        private static void Reparent(Control control, Control parent, Rectangle bounds)
        {
            control.Parent = parent;
            control.Bounds = bounds;
            control.BringToFront();
        }

        private static void StyleField(Control control)
        {
            control.Font = new Font("Segoe UI", 10.5F);
            control.ForeColor = Ink;
            control.BackColor = Color.White;
            control.Margin = new Padding(0);
            if (control is TextBox)
            {
                TextBox textBox = (TextBox)control;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox)
            {
                ComboBox comboBox = (ComboBox)control;
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        private static void StyleCaption(Label label)
        {
            label.AutoSize = false;
            label.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            label.ForeColor = MutedInk;
            label.BackColor = Color.Transparent;
        }

        private static void StyleButton(Button button, string kind)
        {
            Color normal = kind == "danger" ? Coral : kind == "secondary" ? Blue : Green;
            Color hover = kind == "danger" ? CoralHover : kind == "secondary" ? BlueHover : GreenHover;

            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = normal;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            button.Resize += delegate { SetRoundedRegion(button, 13); };
            button.MouseEnter += delegate { button.BackColor = hover; };
            button.MouseLeave += delegate { button.BackColor = normal; };
            SetRoundedRegion(button, 13);
        }

        private static void SetRoundedRegion(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;
            using (GraphicsPath path = RoundedRectangle(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                Region previous = control.Region;
                control.Region = new Region(path);
                if (previous != null)
                    previous.Dispose();
            }
        }

        private static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.FromArgb(232, 249, 255);
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(198, 226, 235);
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 133, 190);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(24, 133, 190);
            grid.ColumnHeadersHeight = 40;
            grid.DefaultCellStyle.BackColor = Color.FromArgb(248, 254, 255);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(231, 247, 252);
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 226, 244);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.RowTemplate.Height = 34;
        }

        public static void ApplyHome(Questioning form, Label title, Label intro,
            Button registration, Button administration)
        {
            ConfigureForm(form, "Admitere ASE • Portal", new Size(1100, 640));

            title.Text = "Viitorul tău începe aici.";
            title.Bounds = new Rectangle(72, 64, 780, 52);
            title.AutoSize = false;
            title.Font = new Font("Segoe UI", 27F, FontStyle.Bold);
            title.ForeColor = Ink;
            title.BackColor = Color.Transparent;

            intro.Text = "Înscrie-te la programul potrivit pentru media ta sau administrează procesul de admitere într-un singur loc.";
            intro.Bounds = new Rectangle(76, 124, 720, 62);
            intro.AutoSize = false;
            intro.Font = new Font("Segoe UI", 12F);
            intro.ForeColor = MutedInk;
            intro.BackColor = Color.Transparent;

            AddLabel(form, "ADMITERE • 2026", new Rectangle(76, 28, 250, 28), 9.5F,
                FontStyle.Bold, Color.FromArgb(16, 126, 175));

            AeroCard card = AddCard(form, new Rectangle(70, 220, 960, 248));
            AddLabel(card, "Alege ce vrei să faci", new Rectangle(38, 28, 500, 34), 16F,
                FontStyle.Bold, Ink);
            AddLabel(card, "Proces clar, rapid și sigur.", new Rectangle(39, 64, 420, 26), 10F,
                FontStyle.Regular, MutedInk);

            Reparent(registration, card, new Rectangle(42, 112, 400, 80));
            registration.Text = "✦  Completează formularul de admitere";
            StyleButton(registration, "primary");

            Reparent(administration, card, new Rectangle(516, 112, 400, 80));
            administration.Text = "⚙  Deschide panoul de administrare";
            StyleButton(administration, "secondary");

            AddLabel(form, "Date locale • Procesare transparentă • Rezultate rapide",
                new Rectangle(72, 500, 700, 28), 9.5F, FontStyle.Regular, Color.White);
        }

        public static void ApplyRegistration(Registration_form form, Label heading,
            Label nameLabel, Label firstNameLabel, Label ageLabel, Label addressLabel,
            Label bacLabel, Label cnpLabel, Label highSchoolLabel, TextBox name, TextBox firstName,
            TextBox address, NumericUpDown age, GroupBox gender, TextBox cnp, TextBox bac,
            TextBox highSchool, Button back, Button next)
        {
            ConfigureForm(form, "Admitere ASE • Date candidat", new Size(1100, 720));

            heading.Text = "Spune-ne câteva lucruri despre tine";
            heading.Bounds = new Rectangle(64, 38, 850, 44);
            heading.AutoSize = false;
            heading.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            heading.ForeColor = Ink;
            heading.BackColor = Color.Transparent;
            AddLabel(form, "Pasul 1 din 2  •  Datele sunt verificate înainte de continuare",
                new Rectangle(67, 86, 720, 28), 10F, FontStyle.Regular, MutedInk);

            AeroCard card = AddCard(form, new Rectangle(60, 132, 980, 458));

            StyleCaption(nameLabel);
            StyleCaption(firstNameLabel);
            StyleCaption(addressLabel);
            StyleCaption(ageLabel);
            StyleCaption(cnpLabel);
            StyleCaption(bacLabel);
            StyleCaption(highSchoolLabel);

            Reparent(nameLabel, card, new Rectangle(42, 35, 210, 25));
            nameLabel.Text = "Nume";
            Reparent(name, card, new Rectangle(42, 62, 270, 34));

            Reparent(firstNameLabel, card, new Rectangle(354, 35, 210, 25));
            Reparent(firstName, card, new Rectangle(354, 62, 270, 34));

            Reparent(addressLabel, card, new Rectangle(666, 35, 240, 25));
            addressLabel.Text = "Adresa de domiciliu";
            Reparent(address, card, new Rectangle(666, 62, 270, 34));

            Reparent(ageLabel, card, new Rectangle(42, 137, 210, 25));
            ageLabel.Text = "Vârstă";
            Reparent(age, card, new Rectangle(42, 164, 150, 34));
            age.Minimum = 16;
            age.Maximum = 100;
            if (age.Value <= 16) age.Value = 18;

            Reparent(gender, card, new Rectangle(354, 132, 270, 76));
            gender.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            gender.ForeColor = MutedInk;
            gender.BackColor = Color.Transparent;

            Reparent(cnpLabel, card, new Rectangle(666, 137, 210, 25));
            Reparent(cnp, card, new Rectangle(666, 164, 270, 34));
            cnp.MaxLength = 13;

            Reparent(bacLabel, card, new Rectangle(42, 248, 270, 25));
            bacLabel.Text = "Media la bacalaureat";
            Reparent(bac, card, new Rectangle(42, 275, 270, 34));

            Reparent(highSchoolLabel, card, new Rectangle(354, 248, 270, 25));
            highSchoolLabel.Text = "Media anilor de liceu";
            Reparent(highSchool, card, new Rectangle(354, 275, 270, 34));

            AddLabel(card, "Media de admitere: 70% bacalaureat + 30% media anilor de liceu",
                new Rectangle(42, 352, 700, 30), 10F, FontStyle.Bold, Color.FromArgb(19, 135, 85));

            StyleField(name);
            StyleField(firstName);
            StyleField(address);
            StyleField(age);
            StyleField(cnp);
            StyleField(bac);
            StyleField(highSchool);
            bac.MaxLength = highSchool.MaxLength = 5;
            bac.TextAlign = HorizontalAlignment.Center;
            highSchool.TextAlign = HorizontalAlignment.Center;

            back.Text = "←  Înapoi";
            back.Bounds = new Rectangle(62, 622, 150, 48);
            StyleButton(back, "secondary");
            next.Text = "Continuă la opțiuni  →";
            next.Bounds = new Rectangle(790, 622, 248, 48);
            StyleButton(next, "primary");
        }

        public static void ApplyChoices(Form3_choice form, Label heading, Label facultyLabel,
            Label specializationLabel, ComboBox faculty, ComboBox specialization, DataGridView grid,
            Button add, Button remove, Button confirm, Button back, Button home)
        {
            ConfigureForm(form, "Admitere ASE • Opțiuni", new Size(1100, 720));

            heading.Text = "Construiește lista ta de opțiuni";
            heading.Bounds = new Rectangle(62, 35, 780, 45);
            heading.AutoSize = false;
            heading.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            heading.ForeColor = Ink;
            heading.BackColor = Color.Transparent;
            AddLabel(form, "Pasul 2 din 2  •  Prima opțiune are prioritatea cea mai mare",
                new Rectangle(65, 82, 720, 28), 10F, FontStyle.Regular, MutedInk);

            AeroCard selectCard = AddCard(form, new Rectangle(58, 132, 390, 410));
            AddLabel(selectCard, "Adaugă o opțiune", new Rectangle(30, 25, 300, 30), 15F,
                FontStyle.Bold, Ink);
            StyleCaption(facultyLabel);
            StyleCaption(specializationLabel);
            Reparent(facultyLabel, selectCard, new Rectangle(30, 78, 320, 24));
            Reparent(faculty, selectCard, new Rectangle(30, 104, 330, 36));
            Reparent(specializationLabel, selectCard, new Rectangle(30, 164, 320, 24));
            Reparent(specialization, selectCard, new Rectangle(30, 190, 330, 36));
            StyleField(faculty);
            StyleField(specialization);
            Reparent(add, selectCard, new Rectangle(30, 258, 330, 48));
            add.Text = "＋  Adaugă în listă";
            StyleButton(add, "primary");
            Reparent(remove, selectCard, new Rectangle(30, 322, 330, 48));
            remove.Text = "−  Elimină ultima opțiune";
            StyleButton(remove, "danger");

            AeroCard listCard = AddCard(form, new Rectangle(472, 132, 570, 410));
            AddLabel(listCard, "Ordinea preferințelor", new Rectangle(28, 25, 350, 30), 15F,
                FontStyle.Bold, Ink);
            Reparent(grid, listCard, new Rectangle(28, 72, 514, 294));
            StyleGrid(grid);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            back.Text = "←  Date candidat";
            back.Bounds = new Rectangle(60, 610, 176, 48);
            StyleButton(back, "secondary");
            home.Text = "⌂  Pagina principală";
            home.Bounds = new Rectangle(252, 610, 190, 48);
            StyleButton(home, "secondary");
            confirm.Text = "✓  Confirmă înscrierea";
            confirm.Bounds = new Rectangle(790, 610, 250, 48);
            StyleButton(confirm, "primary");
        }

        public static void ApplyAdministration(Form4_Administration form, DataGridView grid,
            Button showAll, Button runAdmission, Button showAdmitted, Button reset, Button back,
            Label deleteLabel, TextBox deleteId, Button delete, Label statusLabel, ComboBox status,
            Button update, TextBox updateId, Label searchLabel, ComboBox criterion, TextBox searchValue,
            Button search)
        {
            ConfigureForm(form, "Admitere ASE • Administrare", new Size(1180, 820));
            AddLabel(form, "Panou de administrare", new Rectangle(48, 26, 620, 43), 23F,
                FontStyle.Bold, Ink);
            AddLabel(form, "Vizualizează candidații și gestionează procesul de repartizare.",
                new Rectangle(51, 70, 760, 27), 10F, FontStyle.Regular, MutedInk);

            grid.Bounds = new Rectangle(46, 112, 1088, 390);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            StyleGrid(grid);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            AeroCard findCard = AddCard(form, new Rectangle(46, 524, 360, 206));
            AddLabel(findCard, "Caută și filtrează", new Rectangle(22, 18, 300, 28), 13F,
                FontStyle.Bold, Ink);
            Reparent(searchLabel, findCard, new Rectangle(22, 52, 310, 22));
            searchLabel.Text = "Criteriu și valoare";
            StyleCaption(searchLabel);
            Reparent(criterion, findCard, new Rectangle(22, 80, 145, 34));
            Reparent(searchValue, findCard, new Rectangle(177, 80, 158, 34));
            StyleField(criterion);
            StyleField(searchValue);
            Reparent(search, findCard, new Rectangle(22, 130, 145, 42));
            search.Text = "⌕  Caută";
            StyleButton(search, "secondary");
            Reparent(showAll, findCard, new Rectangle(177, 130, 158, 42));
            showAll.Text = "Toți candidații";
            StyleButton(showAll, "primary");

            AeroCard editCard = AddCard(form, new Rectangle(424, 524, 334, 206));
            AddLabel(editCard, "Editează un candidat", new Rectangle(22, 18, 290, 28), 13F,
                FontStyle.Bold, Ink);
            Reparent(statusLabel, editCard, new Rectangle(22, 52, 290, 22));
            statusLabel.Text = "ID candidat și statut nou";
            StyleCaption(statusLabel);
            Reparent(updateId, editCard, new Rectangle(22, 80, 90, 34));
            Reparent(status, editCard, new Rectangle(122, 80, 188, 34));
            StyleField(updateId);
            StyleField(status);
            Reparent(update, editCard, new Rectangle(22, 130, 138, 42));
            update.Text = "Actualizează";
            StyleButton(update, "secondary");
            Reparent(deleteId, editCard, new Rectangle(170, 130, 70, 42));
            StyleField(deleteId);
            Reparent(delete, editCard, new Rectangle(246, 130, 64, 42));
            delete.Text = "Șterge";
            StyleButton(delete, "danger");
            deleteLabel.Visible = false;

            AeroCard admissionCard = AddCard(form, new Rectangle(776, 524, 358, 206));
            AddLabel(admissionCard, "Repartizare", new Rectangle(22, 18, 290, 28), 13F,
                FontStyle.Bold, Ink);
            Reparent(runAdmission, admissionCard, new Rectangle(22, 58, 150, 48));
            runAdmission.Text = "▶  Rulează admiterea";
            StyleButton(runAdmission, "primary");
            Reparent(showAdmitted, admissionCard, new Rectangle(184, 58, 150, 48));
            showAdmitted.Text = "Vezi admișii";
            StyleButton(showAdmitted, "secondary");
            Reparent(reset, admissionCard, new Rectangle(22, 122, 312, 42));
            reset.Text = "↻  Resetează rezultatele admiterii";
            StyleButton(reset, "danger");

            back.Text = "←  Pagina principală";
            back.Bounds = new Rectangle(46, 755, 210, 44);
            StyleButton(back, "secondary");
        }
    }
}
