using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proiect_admitere_facultate
{
    internal enum ActionButtonStyle
    {
        Primary,
        Secondary,
        Danger,
        Light
    }

    internal static class ModernPalette
    {
        public static readonly Color Ink = Color.FromArgb(16, 54, 72);
        public static readonly Color Muted = Color.FromArgb(75, 111, 125);
        public static readonly Color Blue = Color.FromArgb(18, 137, 203);
        public static readonly Color Green = Color.FromArgb(17, 164, 91);
        public static readonly Color Coral = Color.FromArgb(225, 78, 70);
        public static readonly Color Pale = Color.FromArgb(239, 250, 253);

        public static GraphicsPath Rounded(Rectangle rectangle, int radius)
        {
            int safeRadius = Math.Max(1,
                Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2));
            int diameter = safeRadius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class SmoothButton : Control
    {
        private bool hovering;
        private bool pressed;

        public ActionButtonStyle Style { get; set; }

        public SmoothButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            Cursor = Cursors.Hand;
            TabStop = true;
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            Size = new Size(170, 48);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pressed = true;
                Focus();
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        public void PerformClick()
        {
            if (Enabled)
                OnClick(EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Rectangle shadowRect = new Rectangle(4, 6, Width - 8, Height - 10);
            Rectangle faceRect = new Rectangle(4, pressed ? 5 : 3, Width - 8, Height - 10);

            Color top;
            Color bottom;
            Color border;
            Color text;
            switch (Style)
            {
                case ActionButtonStyle.Secondary:
                    top = hovering ? Color.FromArgb(45, 167, 228) : Color.FromArgb(27, 149, 215);
                    bottom = hovering ? Color.FromArgb(20, 128, 194) : Color.FromArgb(12, 111, 176);
                    border = Color.FromArgb(8, 91, 150);
                    text = Color.White;
                    break;
                case ActionButtonStyle.Danger:
                    top = hovering ? Color.FromArgb(239, 105, 92) : Color.FromArgb(229, 84, 75);
                    bottom = hovering ? Color.FromArgb(208, 68, 61) : Color.FromArgb(189, 52, 49);
                    border = Color.FromArgb(157, 41, 39);
                    text = Color.White;
                    break;
                case ActionButtonStyle.Light:
                    top = hovering ? Color.White : Color.FromArgb(250, 254, 255);
                    bottom = hovering ? Color.FromArgb(232, 247, 252) : Color.FromArgb(220, 240, 247);
                    border = Color.FromArgb(137, 190, 208);
                    text = ModernPalette.Ink;
                    break;
                default:
                    top = hovering ? Color.FromArgb(45, 203, 118) : Color.FromArgb(29, 184, 101);
                    bottom = hovering ? Color.FromArgb(16, 154, 83) : Color.FromArgb(7, 132, 70);
                    border = Color.FromArgb(4, 107, 56);
                    text = Color.White;
                    break;
            }

            using (GraphicsPath shadow = ModernPalette.Rounded(shadowRect, 15))
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(42, 16, 54, 72)))
                e.Graphics.FillPath(shadowBrush, shadow);

            using (GraphicsPath face = ModernPalette.Rounded(faceRect, 15))
            using (LinearGradientBrush fill = new LinearGradientBrush(faceRect, top, bottom, 90f))
            using (Pen outline = new Pen(border, 1.4f))
            {
                e.Graphics.FillPath(fill, face);
                e.Graphics.DrawPath(outline, face);
                using (Pen shine = new Pen(Color.FromArgb(115, 255, 255, 255), 1f))
                {
                    Rectangle highlight = new Rectangle(
                        faceRect.X + 2, faceRect.Y + 2, faceRect.Width - 4, faceRect.Height - 5);
                    using (GraphicsPath highlightPath = ModernPalette.Rounded(highlight, 13))
                        e.Graphics.DrawPath(shine, highlightPath);
                }
            }

            TextRenderer.DrawText(e.Graphics, Text, Font, faceRect, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);

            if (Focused)
            {
                Rectangle focusRect = new Rectangle(
                    faceRect.X + 6, faceRect.Y + 6, faceRect.Width - 12, faceRect.Height - 12);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRect, text, Color.Transparent);
            }
        }
    }

    internal sealed class NavButton : Control
    {
        private bool hovering;
        private bool active;

        public bool Active
        {
            get { return active; }
            set { active = value; Invalidate(); }
        }

        public NavButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            ForeColor = Color.White;
            Size = new Size(196, 50);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle face = new Rectangle(3, 3, Width - 6, Height - 6);
            if (Active || hovering)
            {
                Color fillColor = Active
                    ? Color.FromArgb(235, 255, 255, 255)
                    : Color.FromArgb(35, 255, 255, 255);
                using (GraphicsPath path = ModernPalette.Rounded(face, 14))
                using (SolidBrush fill = new SolidBrush(fillColor))
                    e.Graphics.FillPath(fill, path);
            }

            if (Active)
            {
                using (GraphicsPath marker = ModernPalette.Rounded(
                    new Rectangle(3, 12, 5, Height - 24), 2))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(21, 179, 96)))
                    e.Graphics.FillPath(brush, marker);
            }

            TextRenderer.DrawText(e.Graphics, Text, Font,
                new Rectangle(24, 0, Width - 30, Height),
                Active ? ModernPalette.Ink : Color.FromArgb(231, 249, 255),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    internal class GlassPanel : Panel
    {
        public GlassPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Padding = new Padding(22);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Rectangle shadowRect = new Rectangle(7, 9, Width - 14, Height - 16);
            Rectangle faceRect = new Rectangle(4, 4, Width - 12, Height - 14);

            using (GraphicsPath shadow = ModernPalette.Rounded(shadowRect, 22))
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(34, 7, 57, 75)))
                e.Graphics.FillPath(shadowBrush, shadow);

            using (GraphicsPath face = ModernPalette.Rounded(faceRect, 22))
            using (LinearGradientBrush fill = new LinearGradientBrush(
                faceRect, Color.FromArgb(248, 255, 255, 255),
                Color.FromArgb(232, 246, 251, 255), 90f))
            using (Pen outline = new Pen(Color.FromArgb(210, 255, 255, 255), 1.8f))
            {
                e.Graphics.FillPath(fill, face);
                e.Graphics.DrawPath(outline, face);
            }

            base.OnPaint(e);
        }
    }

    internal sealed class PagePanel : Panel
    {
        public PagePanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }
    }

    public sealed class MainDashboard : Form
    {
        private readonly Panel sidebar;
        private readonly PagePanel pageHost;
        private readonly Label pageKicker;
        private readonly NavButton dashboardNav;
        private readonly NavButton candidatesNav;
        private readonly NavButton admissionNav;
        private Control currentPage;
        private string currentPageKey;

        public MainDashboard()
        {
            Text = "Admitere - Repartizare candidați";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 760);
            ClientSize = new Size(1360, 820);
            WindowState = FormWindowState.Maximized;
            Font = new Font("Segoe UI", 10F);
            ForeColor = ModernPalette.Ink;
            BackColor = Color.FromArgb(191, 234, 248);
            AutoScaleMode = AutoScaleMode.None;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);

            sidebar = new Panel
            {
                Width = 238,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(12, 82, 116),
                Padding = new Padding(20)
            };
            Controls.Add(sidebar);

            Label brand = MakeLabel("ADMITERE", 19F, FontStyle.Bold, Color.White);
            brand.SetBounds(24, 28, 185, 36);
            sidebar.Controls.Add(brand);
            Label brandSubtitle = MakeLabel("REPARTIZARE CANDIDAȚI", 8.5F, FontStyle.Bold,
                Color.FromArgb(151, 220, 240));
            brandSubtitle.SetBounds(26, 65, 185, 25);
            sidebar.Controls.Add(brandSubtitle);

            dashboardNav = CreateNav("Panou general", 126);
            candidatesNav = CreateNav("Candidați", 184);
            admissionNav = CreateNav("Repartizare", 242);
            dashboardNav.Click += delegate { Navigate("dashboard"); };
            candidatesNav.Click += delegate { Navigate("candidates"); };
            admissionNav.Click += delegate { Navigate("admission"); };

            Panel topBar = new Panel
            {
                Height = 76,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(226, 247, 253)
            };
            Controls.Add(topBar);
            topBar.BringToFront();

            pageKicker = MakeLabel("PANOU GENERAL", 9F, FontStyle.Bold, ModernPalette.Blue);
            pageKicker.SetBounds(32, 27, 300, 28);
            topBar.Controls.Add(pageKicker);

            pageHost = new PagePanel { Dock = DockStyle.Fill };
            Controls.Add(pageHost);
            pageHost.BringToFront();

            Shown += MainDashboard_Shown;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle area = new Rectangle(sidebar.Width, 76,
                ClientSize.Width - sidebar.Width, ClientSize.Height - 76);
            if (area.Width <= 0 || area.Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (LinearGradientBrush baseFill = new LinearGradientBrush(
                area, Color.FromArgb(199, 240, 248),
                Color.FromArgb(238, 252, 254), 90f))
                e.Graphics.FillRectangle(baseFill, area);

            using (GraphicsPath aquaBand = new GraphicsPath())
            {
                aquaBand.AddBezier(sidebar.Width - 70, ClientSize.Height - 120,
                    ClientSize.Width * .32f, ClientSize.Height - 280,
                    ClientSize.Width * .58f, ClientSize.Height - 70,
                    ClientSize.Width + 80, ClientSize.Height - 210);
                aquaBand.AddLine(ClientSize.Width + 80, ClientSize.Height + 70,
                    sidebar.Width - 70, ClientSize.Height + 70);
                aquaBand.CloseFigure();
                using (LinearGradientBrush fill = new LinearGradientBrush(
                    area, Color.FromArgb(118, 34, 174, 197),
                    Color.FromArgb(48, 20, 135, 181), 0f))
                    e.Graphics.FillPath(fill, aquaBand);
            }

            using (GraphicsPath glassArc = new GraphicsPath())
            {
                glassArc.AddBezier(sidebar.Width - 40, ClientSize.Height - 255,
                    ClientSize.Width * .34f, ClientSize.Height - 375,
                    ClientSize.Width * .55f, ClientSize.Height - 205,
                    ClientSize.Width + 70, ClientSize.Height - 320);
                using (Pen bright = new Pen(Color.FromArgb(150, 255, 255, 255), 3.5f))
                    e.Graphics.DrawPath(bright, glassArc);
                using (Pen blueLine = new Pen(Color.FromArgb(95, 9, 143, 193), 1.4f))
                    e.Graphics.DrawPath(blueLine, glassArc);
            }

            using (GraphicsPath pane = ModernPalette.Rounded(
                new Rectangle(sidebar.Width + 64, 118, 330, ClientSize.Height - 215), 26))
            using (LinearGradientBrush glass = new LinearGradientBrush(
                area, Color.FromArgb(62, 255, 255, 255),
                Color.FromArgb(10, 255, 255, 255), 45f))
            using (Pen paneLine = new Pen(Color.FromArgb(92, 255, 255, 255), 1.2f))
            {
                e.Graphics.FillPath(glass, pane);
                e.Graphics.DrawPath(paneLine, pane);
            }

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(82, 255, 255, 255)))
                e.Graphics.FillEllipse(glow, ClientSize.Width - 350, 110, 260, 260);
        }

        private void MainDashboard_Shown(object sender, EventArgs e)
        {
            try
            {
                DatabaseManager.ValidateDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Datele aplicației nu pot fi accesate:\n" + ex.Message,
                    "Date indisponibile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Navigate("dashboard");
        }

        private NavButton CreateNav(string text, int top)
        {
            NavButton button = new NavButton
            {
                Text = text,
                Left = 20,
                Top = top,
                Width = 198
            };
            sidebar.Controls.Add(button);
            return button;
        }

        private void Navigate(string key)
        {
            if (key == currentPageKey)
                return;

            Control nextPage;
            if (key == "candidates")
            {
                nextPage = BuildCandidatesPage();
                pageKicker.Text = "CANDIDAȚI";
            }
            else if (key == "admission")
            {
                nextPage = BuildAdmissionPage();
                pageKicker.Text = "REPARTIZARE";
            }
            else
            {
                nextPage = BuildDashboardPage();
                pageKicker.Text = "PANOU GENERAL";
                key = "dashboard";
            }

            currentPageKey = key;
            dashboardNav.Active = key == "dashboard";
            candidatesNav.Active = key == "candidates";
            admissionNav.Active = key == "admission";

            nextPage.Size = pageHost.ClientSize;
            nextPage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                              AnchorStyles.Left | AnchorStyles.Right;
            pageHost.Controls.Add(nextPage);
            nextPage.BringToFront();

            Control previousPage = currentPage;
            nextPage.Left = 0;
            currentPage = nextPage;
            if (previousPage != null)
            {
                pageHost.Controls.Remove(previousPage);
                previousPage.Dispose();
            }
        }

        private Control BuildDashboardPage()
        {
            PagePanel page = NewPage();
            AddPageHeading(page, "Panou general",
                "Urmărește înscrierile și pornește repartizarea candidaților.");

            int cardWidth = 245;
            GlassPanel totalCard = CreateMetricCard(page, 40, 108, cardWidth,
                "CANDIDAȚI ÎN BAZĂ", "—", ModernPalette.Blue);
            GlassPanel pendingCard = CreateMetricCard(page, 305, 108, cardWidth,
                "ÎN AȘTEPTARE", "—", Color.FromArgb(225, 139, 29));
            GlassPanel admittedCard = CreateMetricCard(page, 570, 108, cardWidth,
                "ADMIȘI", "—", ModernPalette.Green);

            totalCard.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pendingCard.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            admittedCard.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            LoadMetricCards(totalCard, pendingCard, admittedCard);

            GlassPanel syncCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 272, page.Width - 80, 170),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(syncCard);
            Label syncTitle = MakeLabel("Preia înscrieri", 17F,
                FontStyle.Bold, ModernPalette.Ink);
            syncTitle.SetBounds(28, 27, 520, 38);
            syncCard.Controls.Add(syncTitle);
            Label syncText = MakeLabel(
                "Adu în aplicație înscrierile trimise prin formular.",
                10F, FontStyle.Regular, ModernPalette.Muted);
            syncText.SetBounds(30, 73, 690, 52);
            syncCard.Controls.Add(syncText);

            Label syncStatus = MakeLabel(
                WebSyncService.IsConfigured
                    ? "Pregătit pentru preluare"
                    : "Preluarea nu este configurată",
                9.3F, FontStyle.Bold,
                WebSyncService.IsConfigured ? ModernPalette.Green : Color.FromArgb(191, 119, 18));
            syncStatus.SetBounds(30, 118, 420, 28);
            syncCard.Controls.Add(syncStatus);

            SmoothButton sync = new SmoothButton
            {
                Text = "Preia acum",
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(syncCard.Width - 252, 82, 220, 56),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            syncCard.Controls.Add(sync);
            sync.Click += async delegate
            {
                sync.Enabled = false;
                sync.Text = "Se preiau…";
                sync.Invalidate();
                try
                {
                    SyncResult result = await WebSyncService.SynchronizeAsync();
                    syncStatus.Text = string.Format(
                        "Importate: {0}   •   Deja existente: {1}   •   Erori: {2}",
                        result.Imported, result.AlreadyPresent, result.Failed);
                    syncStatus.ForeColor = result.Failed == 0
                        ? ModernPalette.Green : ModernPalette.Coral;
                    LoadMetricCards(totalCard, pendingCard, admittedCard);
                }
                catch (Exception ex)
                {
                    syncStatus.Text = "Preluarea nu a reușit";
                    syncStatus.ForeColor = ModernPalette.Coral;
                    MessageBox.Show(ex.Message, "Preluare înscrieri",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    sync.Enabled = true;
                    sync.Text = "Preia acum";
                    sync.Invalidate();
                }
            };
            return page;
        }

        private Control BuildCandidatesPage()
        {
            PagePanel page = NewPage();
            AddPageHeading(page, "Lista candidaților",
                "Caută, verifică și actualizează înregistrările importate.");

            GlassPanel toolbar = new GlassPanel
            {
                Bounds = new Rectangle(40, 98, page.Width - 80, 92),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(toolbar);

            ComboBox criterion = StyledCombo(new[] { "Nume", "CNP", "ID", "Status" });
            criterion.SetBounds(24, 25, 150, 36);
            toolbar.Controls.Add(criterion);
            TextBox value = StyledTextBox();
            value.SetBounds(188, 25, 285, 36);
            toolbar.Controls.Add(value);

            SmoothButton search = new SmoothButton
            {
                Text = "Caută",
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(488, 18, 138, 50)
            };
            toolbar.Controls.Add(search);
            SmoothButton refresh = new SmoothButton
            {
                Text = "Afișează tot",
                Style = ActionButtonStyle.Light,
                Bounds = new Rectangle(638, 18, 155, 50)
            };
            toolbar.Controls.Add(refresh);

            GlassPanel gridCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 205, page.Width - 80, page.Height - 335),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(gridCard);
            DataGridView grid = CreateGrid();
            grid.SetBounds(20, 20, gridCard.Width - 40, gridCard.Height - 40);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                          AnchorStyles.Left | AnchorStyles.Right;
            gridCard.Controls.Add(grid);

            GlassPanel actions = new GlassPanel
            {
                Bounds = new Rectangle(40, page.Height - 116, page.Width - 80, 92),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(actions);
            Label selectedHint = MakeLabel(
                "Selectează un rând pentru a modifica statutul sau a șterge candidatul.",
                9.2F, FontStyle.Regular, ModernPalette.Muted);
            selectedHint.SetBounds(24, 18, 430, 24);
            actions.Controls.Add(selectedHint);

            ComboBox status = StyledCombo(new[] { "Nedefinit", "Admis", "Respins" });
            status.SetBounds(470, 25, 145, 36);
            actions.Controls.Add(status);
            SmoothButton applyStatus = new SmoothButton
            {
                Text = "Aplică statut",
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(625, 18, 160, 50)
            };
            actions.Controls.Add(applyStatus);
            SmoothButton delete = new SmoothButton
            {
                Text = "Șterge candidatul",
                Style = ActionButtonStyle.Danger,
                Bounds = new Rectangle(actions.Width - 205, 18, 180, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            actions.Controls.Add(delete);

            Action loadAll = delegate
            {
                LoadCandidatesGrid(grid, null, null);
            };
            loadAll();
            refresh.Click += delegate { value.Clear(); loadAll(); };
            search.Click += delegate
            {
                string entered = value.Text.Trim();
                if (string.IsNullOrEmpty(entered))
                {
                    loadAll();
                    return;
                }
                LoadCandidatesGrid(grid, criterion.SelectedItem.ToString(), entered);
            };
            value.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    search.PerformClick();
                    e.SuppressKeyPress = true;
                }
            };

            applyStatus.Click += delegate
            {
                int candidateId;
                if (!TryGetSelectedCandidateId(grid, out candidateId))
                {
                    MessageBox.Show("Selectează mai întâi un candidat.");
                    return;
                }
                try
                {
                    DatabaseManager.UpdateCandidateStatus(
                        candidateId, status.SelectedItem.ToString());
                    loadAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Actualizare nereușită");
                }
            };

            delete.Click += delegate
            {
                int candidateId;
                if (!TryGetSelectedCandidateId(grid, out candidateId))
                {
                    MessageBox.Show("Selectează mai întâi un candidat.");
                    return;
                }
                if (MessageBox.Show(
                    "Ștergi definitiv candidatul selectat și toate opțiunile sale?",
                    "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                    return;
                try
                {
                    DatabaseManager.DeleteCandidate(candidateId);
                    loadAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ștergere nereușită");
                }
            };
            return page;
        }

        private Control BuildAdmissionPage()
        {
            PagePanel page = NewPage();
            AddPageHeading(page, "Repartizarea candidaților",
                "Candidații sunt ordonați după medie și repartizați în ordinea opțiunilor.");

            GlassPanel controlsCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 100, page.Width - 80, 142),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(controlsCard);
            Label controlsTitle = MakeLabel("Repartizare", 15F,
                FontStyle.Bold, ModernPalette.Ink);
            controlsTitle.SetBounds(28, 31, 300, 34);
            controlsCard.Controls.Add(controlsTitle);
            Label statusText = MakeLabel("Generează rezultatele pentru candidații importați.",
                9.2F, FontStyle.Regular, ModernPalette.Muted);
            statusText.SetBounds(30, 71, 520, 25);
            controlsCard.Controls.Add(statusText);

            SmoothButton run = new SmoothButton
            {
                Text = "Rulează repartizarea",
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(controlsCard.Width - 455, 38, 220, 58),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlsCard.Controls.Add(run);
            SmoothButton reset = new SmoothButton
            {
                Text = "Resetează rezultatele",
                Style = ActionButtonStyle.Danger,
                Bounds = new Rectangle(controlsCard.Width - 225, 38, 200, 58),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlsCard.Controls.Add(reset);

            GlassPanel resultsCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 260, page.Width - 80, page.Height - 285),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(resultsCard);
            Label resultsTitle = MakeLabel("Candidați admiși", 14F,
                FontStyle.Bold, ModernPalette.Ink);
            resultsTitle.SetBounds(24, 18, 300, 30);
            resultsCard.Controls.Add(resultsTitle);
            DataGridView results = CreateGrid();
            results.SetBounds(20, 56, resultsCard.Width - 40, resultsCard.Height - 78);
            results.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                             AnchorStyles.Left | AnchorStyles.Right;
            resultsCard.Controls.Add(results);

            Action loadResults = delegate { LoadAdmissionResults(results); };
            loadResults();
            run.Click += delegate
            {
                try
                {
                    int count = DatabaseManager.RunAdmission();
                    statusText.Text = "Repartizare finalizată • " + count + " candidați admiși";
                    statusText.ForeColor = ModernPalette.Green;
                    loadResults();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Repartizare nereușită");
                }
            };
            reset.Click += delegate
            {
                if (MessageBox.Show("Resetezi toate rezultatele repartizării?",
                    "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                    return;
                try
                {
                    DatabaseManager.ResetAdmission();
                    statusText.Text = "Rezultatele au fost resetate.";
                    statusText.ForeColor = ModernPalette.Muted;
                    loadResults();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Resetare nereușită");
                }
            };
            return page;
        }

        private PagePanel NewPage()
        {
            return new PagePanel
            {
                Size = pageHost.ClientSize
            };
        }

        private static void AddPageHeading(Control parent, string title, string subtitle)
        {
            Label titleLabel = MakeLabel(title, 23F, FontStyle.Bold, ModernPalette.Ink);
            titleLabel.SetBounds(40, 24, 870, 46);
            parent.Controls.Add(titleLabel);
            Label subtitleLabel = MakeLabel(subtitle, 10.3F,
                FontStyle.Regular, ModernPalette.Muted);
            subtitleLabel.SetBounds(43, 67, 900, 28);
            parent.Controls.Add(subtitleLabel);
        }

        private static GlassPanel CreateMetricCard(Control parent, int left, int top,
            int width, string caption, string value, Color accent)
        {
            GlassPanel card = new GlassPanel
            {
                Bounds = new Rectangle(left, top, width, 138)
            };
            parent.Controls.Add(card);
            Label captionLabel = MakeLabel(caption, 8.5F, FontStyle.Bold, accent);
            captionLabel.SetBounds(24, 22, width - 48, 24);
            card.Controls.Add(captionLabel);
            Label valueLabel = MakeLabel(value, 27F, FontStyle.Bold, ModernPalette.Ink);
            valueLabel.Name = "MetricValue";
            valueLabel.SetBounds(23, 52, width - 48, 52);
            card.Controls.Add(valueLabel);
            return card;
        }

        private static void LoadMetricCards(
            GlassPanel totalCard, GlassPanel pendingCard, GlassPanel admittedCard)
        {
            try
            {
                DataTable data = DatabaseManager.ExecuteQuery(@"
                    SELECT
                        COUNT(*) AS Total,
                        SUM(CASE WHEN Status = 'Nedefinit' THEN 1 ELSE 0 END) AS Nedefinit,
                        SUM(CASE WHEN Status = 'Admis' THEN 1 ELSE 0 END) AS Admisi
                    FROM Candidati");
                DataRow row = data.Rows[0];
                SetMetric(totalCard, Convert.ToInt32(row["Total"]));
                SetMetric(pendingCard, Convert.ToInt32(row["Nedefinit"]));
                SetMetric(admittedCard, Convert.ToInt32(row["Admisi"]));
            }
            catch
            {
                SetMetric(totalCard, 0);
                SetMetric(pendingCard, 0);
                SetMetric(admittedCard, 0);
            }
        }

        private static void SetMetric(Control card, int value)
        {
            Control[] labels = card.Controls.Find("MetricValue", false);
            if (labels.Length > 0)
                labels[0].Text = value.ToString();
        }

        private static Label MakeLabel(
            string text, float size, FontStyle style, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color
            };
        }

        private static ComboBox StyledCombo(string[] items)
        {
            ComboBox combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ModernPalette.Ink,
                Font = new Font("Segoe UI", 10F)
            };
            combo.Items.AddRange(items);
            combo.SelectedIndex = 0;
            return combo;
        }

        private static TextBox StyledTextBox()
        {
            return new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = ModernPalette.Ink,
                Font = new Font("Segoe UI", 11F)
            };
        }

        private static DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                BackgroundColor = Color.FromArgb(244, 252, 254),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(207, 229, 236),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                EnableHeadersVisualStyles = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(12, 111, 166);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(12, 111, 166);
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI Semibold", 9.2F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 42;
            grid.DefaultCellStyle.BackColor = Color.FromArgb(249, 254, 255);
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(232, 247, 251);
            grid.DefaultCellStyle.ForeColor = ModernPalette.Ink;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(178, 226, 242);
            grid.DefaultCellStyle.SelectionForeColor = ModernPalette.Ink;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.2F);
            grid.RowTemplate.Height = 35;
            return grid;
        }

        private static void LoadCandidatesGrid(
            DataGridView grid, string criterion, string value)
        {
            const string baseQuery = @"
                SELECT
                    C.IdCandidat AS [ID],
                    C.Nume + ' ' + C.Prenume AS [Nume complet],
                    C.MedieBAC AS [BAC],
                    C.MedieLiceu AS [Liceu],
                    CAST(C.MedieLiceu * 0.3 + C.MedieBAC * 0.7 AS DECIMAL(5,2))
                        AS [Medie],
                    S1.NumeSpecializare AS [Opțiunea 1],
                    S2.NumeSpecializare AS [Opțiunea 2],
                    S3.NumeSpecializare AS [Opțiunea 3],
                    C.CNP,
                    C.Status
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
                LEFT JOIN Specializari S3 ON O.IdSpecializare3 = S3.IdSpecializare";

            string where = string.Empty;
            SqlParameter parameter = null;
            if (criterion == "Nume")
            {
                where = " WHERE C.Nume LIKE @Value OR C.Prenume LIKE @Value";
                parameter = new SqlParameter("@Value", SqlDbType.NVarChar, 100)
                { Value = "%" + value + "%" };
            }
            else if (criterion == "CNP")
            {
                where = " WHERE C.CNP = @Value";
                parameter = new SqlParameter("@Value", SqlDbType.Char, 13) { Value = value };
            }
            else if (criterion == "ID")
            {
                int id;
                if (!int.TryParse(value, out id))
                {
                    MessageBox.Show("ID-ul trebuie să fie numeric.");
                    return;
                }
                where = " WHERE C.IdCandidat = @Value";
                parameter = new SqlParameter("@Value", SqlDbType.Int) { Value = id };
            }
            else if (criterion == "Status")
            {
                where = " WHERE C.Status = @Value";
                parameter = new SqlParameter("@Value", SqlDbType.NVarChar, 20)
                { Value = value };
            }

            try
            {
                grid.DataSource = parameter == null
                    ? DatabaseManager.ExecuteQuery(baseQuery + " ORDER BY C.IdCandidat")
                    : DatabaseManager.ExecuteQuery(
                        baseQuery + where + " ORDER BY C.IdCandidat", parameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Încărcare nereușită");
            }
        }

        private static bool TryGetSelectedCandidateId(
            DataGridView grid, out int candidateId)
        {
            candidateId = 0;
            if (grid.CurrentRow == null ||
                grid.CurrentRow.Cells["ID"].Value == null)
                return false;
            return int.TryParse(
                grid.CurrentRow.Cells["ID"].Value.ToString(), out candidateId);
        }

        private static void LoadAdmissionResults(DataGridView grid)
        {
            const string query = @"
                SELECT
                    C.IdCandidat AS [ID],
                    C.Nume + ' ' + C.Prenume AS [Nume complet],
                    F.NumeFacultate AS [Facultate],
                    S.NumeSpecializare AS [Specializare],
                    CAST(C.MedieLiceu * 0.3 + C.MedieBAC * 0.7 AS DECIMAL(5,2))
                        AS [Medie finală]
                FROM AdmitereFinala A
                INNER JOIN Candidati C ON A.IdCandidat = C.IdCandidat
                INNER JOIN Specializari S ON A.IdSpecializare = S.IdSpecializare
                INNER JOIN Facultati F ON S.IdFacultate = F.IdFacultate
                ORDER BY [Medie finală] DESC";
            try
            {
                grid.DataSource = DatabaseManager.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Rezultate indisponibile");
            }
        }
    }
}
