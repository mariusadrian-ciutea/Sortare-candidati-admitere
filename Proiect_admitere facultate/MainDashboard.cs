using System;
using System.Data;
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

    internal sealed class SelectItem
    {
        public string Text { get; private set; }
        public string Value { get; private set; }

        public SelectItem(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    internal sealed class DashboardOverviewPanel : Control
    {
        public string SampleName { get; set; }
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Admitted { get; set; }
        public int Rejected { get; set; }
        public int Imported { get; set; }

        public DashboardOverviewPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Rectangle face = new Rectangle(4, 4, Width - 10, Height - 12);
            using (GraphicsPath path = ModernPalette.Rounded(face, 34))
            using (LinearGradientBrush fill = new LinearGradientBrush(
                face, Color.FromArgb(226, 255, 255, 255),
                Color.FromArgb(158, 214, 246, 255), 25f))
            using (Pen outline = new Pen(Color.FromArgb(210, 255, 255, 255), 2f))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(outline, path);
            }

            using (GraphicsPath wave = new GraphicsPath())
            {
                wave.AddBezier(face.Left - 20, face.Bottom - 80,
                    face.Left + face.Width * .25f, face.Bottom - 170,
                    face.Left + face.Width * .58f, face.Bottom - 25,
                    face.Right + 30, face.Bottom - 118);
                wave.AddLine(face.Right + 30, face.Bottom - 118,
                    face.Right + 30, face.Bottom + 30);
                wave.AddLine(face.Right + 30, face.Bottom + 30,
                    face.Left - 20, face.Bottom + 30);
                wave.CloseFigure();
                using (LinearGradientBrush waveFill = new LinearGradientBrush(
                    face, Color.FromArgb(92, 7, 180, 207),
                    Color.FromArgb(25, 42, 224, 164), 0f))
                    e.Graphics.FillPath(waveFill, wave);
            }

            TextRenderer.DrawText(e.Graphics, "EȘANTION ACTIV", 
                new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                new Rectangle(38, 30, Width - 80, 22), ModernPalette.Blue,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(e.Graphics,
                string.IsNullOrWhiteSpace(SampleName) ? "—" : SampleName,
                new Font("Segoe UI", 24F, FontStyle.Bold),
                new Rectangle(36, 56, Width - 76, 50), ModernPalette.Ink,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);

            int slotWidth = Math.Max(145, (Width - 80) / 5);
            DrawNumber(e.Graphics, "Candidați", Total, 40, 136, slotWidth);
            DrawNumber(e.Graphics, "Nedefiniți", Pending, 40 + slotWidth, 136, slotWidth);
            DrawNumber(e.Graphics, "Admiși", Admitted, 40 + slotWidth * 2, 136, slotWidth);
            DrawNumber(e.Graphics, "Respinși", Rejected, 40 + slotWidth * 3, 136, slotWidth);
            DrawNumber(e.Graphics, "Din formular", Imported, 40 + slotWidth * 4, 136, slotWidth);
        }

        private static void DrawNumber(
            Graphics graphics, string caption, int value, int left, int top, int width)
        {
            TextRenderer.DrawText(graphics, value.ToString(),
                new Font("Segoe UI", 28F, FontStyle.Bold),
                new Rectangle(left, top, width - 8, 48), ModernPalette.Ink,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(graphics, caption,
                new Font("Segoe UI Semibold", 9.3F, FontStyle.Bold),
                new Rectangle(left + 2, top + 52, width - 10, 24), ModernPalette.Muted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
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
        private readonly Image backgroundImage;
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
            string backgroundPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "aero-background.png");
            if (System.IO.File.Exists(backgroundPath))
                backgroundImage = Image.FromFile(backgroundPath);

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
            if (backgroundImage != null)
            {
                Rectangle source = CoverSourceRectangle(backgroundImage, area);
                e.Graphics.DrawImage(backgroundImage, area, source, GraphicsUnit.Pixel);
                using (SolidBrush veil = new SolidBrush(Color.FromArgb(80, 238, 252, 255)))
                    e.Graphics.FillRectangle(veil, area);
            }
            else
            {
                using (LinearGradientBrush baseFill = new LinearGradientBrush(
                    area, Color.FromArgb(199, 240, 248),
                    Color.FromArgb(238, 252, 254), 90f))
                    e.Graphics.FillRectangle(baseFill, area);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && backgroundImage != null)
                backgroundImage.Dispose();
            base.Dispose(disposing);
        }

        private static Rectangle CoverSourceRectangle(Image image, Rectangle target)
        {
            float imageRatio = image.Width / (float)image.Height;
            float targetRatio = target.Width / (float)target.Height;
            if (imageRatio > targetRatio)
            {
                int sourceWidth = (int)(image.Height * targetRatio);
                return new Rectangle((image.Width - sourceWidth) / 2, 0,
                    sourceWidth, image.Height);
            }

            int sourceHeight = (int)(image.Width / targetRatio);
            return new Rectangle(0, (image.Height - sourceHeight) / 2,
                image.Width, sourceHeight);
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
                "Lucrează pe eșantioane separate și testează rapid scenarii de admitere.");

            ComboBox sampleCombo = CreateSampleCombo();
            sampleCombo.SetBounds(page.Width - 570, 32, 245, 36);
            sampleCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            page.Controls.Add(sampleCombo);

            SmoothButton newSample = new SmoothButton
            {
                Text = "Eșantion nou",
                Style = ActionButtonStyle.Light,
                Bounds = new Rectangle(page.Width - 315, 24, 132, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            page.Controls.Add(newSample);

            SmoothButton demo = new SmoothButton
            {
                Text = "Generează demo",
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(page.Width - 174, 24, 150, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            page.Controls.Add(demo);

            DashboardOverviewPanel overview = new DashboardOverviewPanel
            {
                Bounds = new Rectangle(40, 105, page.Width - 80, 245),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(overview);

            GlassPanel syncCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 374, page.Width - 80, 136),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(syncCard);

            Label syncTitle = MakeLabel("Preluare din formularul online", 15.5F,
                FontStyle.Bold, ModernPalette.Ink);
            syncTitle.SetBounds(28, 24, 420, 32);
            syncCard.Controls.Add(syncTitle);
            Label syncText = MakeLabel(
                "Alege eșantionul în care intră înscrierile și preia candidatii trimiși prin site.",
                9.6F, FontStyle.Regular, ModernPalette.Muted);
            syncText.SetBounds(30, 59, 680, 27);
            syncCard.Controls.Add(syncText);

            Label syncStatus = MakeLabel(
                WebSyncService.IsConfigured
                    ? "Pregătit pentru preluare"
                    : "Preluarea nu este configurată",
                9.3F, FontStyle.Bold,
                WebSyncService.IsConfigured ? ModernPalette.Green : Color.FromArgb(191, 119, 18));
            syncStatus.SetBounds(30, 91, 690, 28);
            syncCard.Controls.Add(syncStatus);

            ComboBox importSample = CreateSampleCombo();
            importSample.SetBounds(syncCard.Width - 470, 48, 240, 36);
            importSample.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            syncCard.Controls.Add(importSample);
            SetComboSelectedValue(importSample, SelectedSampleId(sampleCombo));

            SmoothButton sync = new SmoothButton
            {
                Text = "Preia acum",
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(syncCard.Width - 215, 40, 185, 52),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            syncCard.Controls.Add(sync);

            Action refreshOverview = delegate
            {
                LoadDashboardOverview(overview, SelectedSampleId(sampleCombo));
            };
            refreshOverview();

            sampleCombo.SelectedIndexChanged += delegate
            {
                SetComboSelectedValue(importSample, SelectedSampleId(sampleCombo));
                refreshOverview();
            };

            newSample.Click += delegate
            {
                string name = PromptForText("Eșantion nou",
                    "Numele eșantionului:", "Eșantion " +
                    DateTime.Now.ToString("dd.MM HH:mm"));
                if (string.IsNullOrWhiteSpace(name))
                    return;

                try
                {
                    int id = DatabaseManager.CreateSample(name);
                    ReloadSampleCombo(sampleCombo);
                    ReloadSampleCombo(importSample);
                    SetComboSelectedValue(sampleCombo, id);
                    SetComboSelectedValue(importSample, id);
                    refreshOverview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Eșantion nou");
                }
            };

            demo.Click += delegate
            {
                string name = PromptForText("Eșantion demo",
                    "Numele eșantionului fictiv:", "Demo " +
                    DateTime.Now.ToString("dd.MM HH:mm"));
                if (string.IsNullOrWhiteSpace(name))
                    return;

                try
                {
                    int id = DatabaseManager.GenerateDemoSample(name, 80);
                    ReloadSampleCombo(sampleCombo);
                    ReloadSampleCombo(importSample);
                    SetComboSelectedValue(sampleCombo, id);
                    SetComboSelectedValue(importSample, id);
                    refreshOverview();
                    MessageBox.Show("Am generat 80 de candidați fictivi.",
                        "Eșantion demo", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Generare eșantion demo");
                }
            };

            sync.Click += async delegate
            {
                sync.Enabled = false;
                sync.Text = "Se preiau…";
                sync.Invalidate();
                try
                {
                    int sampleId = SelectedSampleId(importSample);
                    SyncResult result = await WebSyncService.SynchronizeAsync(sampleId);
                    syncStatus.Text = string.Format(
                        "Importate: {0}   •   Deja existente: {1}   •   Erori: {2}",
                        result.Imported, result.AlreadyPresent, result.Failed);
                    syncStatus.ForeColor = result.Failed == 0
                        ? ModernPalette.Green : ModernPalette.Coral;
                    SetComboSelectedValue(sampleCombo, sampleId);
                    refreshOverview();
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
                "Vezi datele complete primite din formular, pe eșantionul selectat.");

            ComboBox sampleCombo = CreateSampleCombo();
            sampleCombo.SetBounds(page.Width - 315, 32, 275, 36);
            sampleCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            page.Controls.Add(sampleCombo);

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
                LoadCandidatesGrid(grid, SelectedSampleId(sampleCombo), null, null);
            };
            loadAll();
            sampleCombo.SelectedIndexChanged += delegate { loadAll(); };
            refresh.Click += delegate { value.Clear(); loadAll(); };
            search.Click += delegate
            {
                string entered = value.Text.Trim();
                if (string.IsNullOrEmpty(entered))
                {
                    loadAll();
                    return;
                }
                LoadCandidatesGrid(grid, SelectedSampleId(sampleCombo),
                    criterion.SelectedItem.ToString(), entered);
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
                "Alege eșantionul și algoritmul, apoi generează rezultatele separat.");

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

            ComboBox sampleCombo = CreateSampleCombo();
            sampleCombo.SetBounds(30, 94, 245, 34);
            controlsCard.Controls.Add(sampleCombo);

            ComboBox algorithm = CreateAlgorithmCombo();
            algorithm.SetBounds(292, 94, 265, 34);
            controlsCard.Controls.Add(algorithm);

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

            Action loadResults = delegate
            {
                LoadAdmissionResults(results, SelectedSampleId(sampleCombo),
                    SelectedAlgorithm(algorithm));
            };
            loadResults();
            sampleCombo.SelectedIndexChanged += delegate { loadResults(); };
            algorithm.SelectedIndexChanged += delegate { loadResults(); };
            run.Click += delegate
            {
                try
                {
                    int count = DatabaseManager.RunAdmission(
                        SelectedSampleId(sampleCombo), SelectedAlgorithm(algorithm));
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
                    DatabaseManager.ResetAdmission(
                        SelectedSampleId(sampleCombo), SelectedAlgorithm(algorithm));
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
            if (items.Length > 0)
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

        private static ComboBox CreateSampleCombo()
        {
            ComboBox combo = StyledCombo(new string[0]);
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantion";
            ReloadSampleCombo(combo);
            return combo;
        }

        private static void ReloadSampleCombo(ComboBox combo)
        {
            int selectedId = SelectedSampleId(combo);
            combo.DataSource = DatabaseManager.GetSamples();
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantion";
            if (selectedId > 0)
                SetComboSelectedValue(combo, selectedId);
        }

        private static int SelectedSampleId(ComboBox combo)
        {
            if (combo == null)
                return DatabaseManager.GetDefaultSampleId();
            if (combo.SelectedValue == null)
                return DatabaseManager.GetDefaultSampleId();

            DataRowView row = combo.SelectedItem as DataRowView;
            if (row != null)
                return Convert.ToInt32(row["IdEsantion"]);

            int id;
            if (int.TryParse(combo.SelectedValue.ToString(), out id))
                return id;

            return DatabaseManager.GetDefaultSampleId();
        }

        private static void SetComboSelectedValue(ComboBox combo, int id)
        {
            if (combo == null || id <= 0)
                return;

            combo.SelectedValue = id;
        }

        private static ComboBox CreateAlgorithmCombo()
        {
            ComboBox combo = StyledCombo(new string[0]);
            combo.Items.Add(new SelectItem("Standard: 70% BAC + 30% liceu", "weighted"));
            combo.Items.Add(new SelectItem("Prioritate BAC", "bac"));
            combo.Items.Add(new SelectItem("Prioritate liceu", "liceu"));
            combo.Items.Add(new SelectItem("Medie egală BAC/Liceu", "balanced"));
            combo.SelectedIndex = 0;
            return combo;
        }

        private static string SelectedAlgorithm(ComboBox combo)
        {
            SelectItem item = combo.SelectedItem as SelectItem;
            return item == null ? "weighted" : item.Value;
        }

        private static void LoadDashboardOverview(
            DashboardOverviewPanel overview, int sampleId)
        {
            DataRow summary = DatabaseManager.GetSampleSummary(sampleId);
            if (summary == null)
            {
                overview.SampleName = "—";
                overview.Total = overview.Pending = overview.Admitted =
                    overview.Rejected = overview.Imported = 0;
            }
            else
            {
                overview.SampleName = summary["Nume"].ToString();
                overview.Total = ToInt(summary["Total"]);
                overview.Pending = ToInt(summary["Nedefinit"]);
                overview.Admitted = ToInt(summary["Admisi"]);
                overview.Rejected = ToInt(summary["Respinsi"]);
                overview.Imported = ToInt(summary["Importate"]);
            }
            overview.Invalidate();
        }

        private static int ToInt(object value)
        {
            return value == null || value == DBNull.Value
                ? 0
                : Convert.ToInt32(value);
        }

        private static string PromptForText(
            string title, string labelText, string defaultValue)
        {
            using (Form prompt = new Form())
            using (Label label = new Label())
            using (TextBox textBox = new TextBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                prompt.Text = title;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.ClientSize = new Size(430, 150);
                prompt.Font = new Font("Segoe UI", 10F);

                label.Text = labelText;
                label.SetBounds(18, 18, 390, 24);
                textBox.Text = defaultValue;
                textBox.SetBounds(18, 50, 392, 30);

                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.SetBounds(214, 102, 92, 32);
                cancel.Text = "Anulează";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.SetBounds(318, 102, 92, 32);

                prompt.Controls.Add(label);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK
                    ? textBox.Text.Trim()
                    : null;
            }
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
            DataGridView grid, int sampleId, string criterion, string value)
        {
            const string baseQuery = @"
                SELECT
                    C.IdCandidat AS [ID],
                    E.Nume AS [Eșantion],
                    C.Nume AS [Nume],
                    C.Prenume AS [Prenume],
                    C.Adresa AS [Adresă],
                    C.Varsta AS [Vârstă],
                    C.Sex AS [Sex],
                    C.CNP AS [CNP],
                    C.MedieBAC AS [BAC],
                    C.MedieLiceu AS [Liceu],
                    ROUND(C.MedieLiceu * 0.3 + C.MedieBAC * 0.7, 2)
                        AS [Medie],
                    S1.NumeSpecializare AS [Opțiunea 1],
                    S2.NumeSpecializare AS [Opțiunea 2],
                    S3.NumeSpecializare AS [Opțiunea 3],
                    C.Status AS [Status],
                    IFNULL(I.CodInscriere, '') AS [Cod formular],
                    IFNULL(I.CreatLaFormular, '') AS [Trimis online],
                    IFNULL(I.ImportatLa, '') AS [Importat în aplicație]
                FROM Candidati C
                INNER JOIN Esantioane E ON C.IdEsantion = E.IdEsantion
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
                LEFT JOIN ImporturiWeb I ON I.IdCandidat = C.IdCandidat";

            string where = " WHERE C.IdEsantion = @SampleId";
            IDbDataParameter parameter = null;
            if (criterion == "Nume")
            {
                where += " AND (C.Nume LIKE @Value OR C.Prenume LIKE @Value)";
                parameter = DatabaseManager.CreateParameter(
                    "@Value", "%" + value + "%");
            }
            else if (criterion == "CNP")
            {
                where += " AND C.CNP = @Value";
                parameter = DatabaseManager.CreateParameter("@Value", value);
            }
            else if (criterion == "ID")
            {
                int id;
                if (!int.TryParse(value, out id))
                {
                    MessageBox.Show("ID-ul trebuie să fie numeric.");
                    return;
                }
                where += " AND C.IdCandidat = @Value";
                parameter = DatabaseManager.CreateParameter("@Value", id);
            }
            else if (criterion == "Status")
            {
                where += " AND C.Status = @Value";
                parameter = DatabaseManager.CreateParameter("@Value", value);
            }

            try
            {
                grid.DataSource = parameter == null
                    ? DatabaseManager.ExecuteQuery(
                        baseQuery + where + " ORDER BY C.IdCandidat",
                        DatabaseManager.CreateParameter("@SampleId", sampleId))
                    : DatabaseManager.ExecuteQuery(
                        baseQuery + where + " ORDER BY C.IdCandidat",
                        DatabaseManager.CreateParameter("@SampleId", sampleId),
                        parameter);
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

        private static void LoadAdmissionResults(
            DataGridView grid, int sampleId, string algorithm)
        {
            string scoreExpression = DatabaseManager.GetAlgorithmExpression(algorithm);
            string query = @"
                SELECT
                    C.IdCandidat AS [ID],
                    C.Nume || ' ' || C.Prenume AS [Nume complet],
                    F.NumeFacultate AS [Facultate],
                    S.NumeSpecializare AS [Specializare],
                    ROUND(" + scoreExpression + @", 2)
                        AS [Medie finală]
                FROM AdmitereFinala A
                INNER JOIN Candidati C ON A.IdCandidat = C.IdCandidat
                INNER JOIN Specializari S ON A.IdSpecializare = S.IdSpecializare
                INNER JOIN Facultati F ON S.IdFacultate = F.IdFacultate
                WHERE A.IdEsantion = @IdEsantion
                  AND A.Algoritm = @Algoritm
                ORDER BY [Medie finală] DESC";
            try
            {
                grid.DataSource = DatabaseManager.ExecuteQuery(query,
                    DatabaseManager.CreateParameter("@IdEsantion", sampleId),
                    DatabaseManager.CreateParameter("@Algoritm", algorithm));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Rezultate indisponibile");
            }
        }
    }
}
