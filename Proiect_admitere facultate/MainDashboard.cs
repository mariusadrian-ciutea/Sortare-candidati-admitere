using System;
using System.Collections.Generic;
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
        public bool English { get; set; }

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
        private readonly Label brandTitle;
        private readonly Label brandSubtitle;
        private readonly NavButton dashboardNav;
        private readonly NavButton candidatesNav;
        private readonly NavButton samplesNav;
        private readonly NavButton admissionNav;
        private readonly SmoothButton languageButton;
        private readonly Image backgroundImage;
        private Control currentPage;
        private string currentPageKey;
        private bool englishUi;

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

            brandTitle = MakeLabel("ADMITERE", 19F, FontStyle.Bold, Color.White);
            brandTitle.SetBounds(24, 28, 185, 36);
            sidebar.Controls.Add(brandTitle);
            Label brandSubtitle = MakeLabel("REPARTIZARE CANDIDAȚI", 8.5F, FontStyle.Bold,
                Color.FromArgb(151, 220, 240));
            brandSubtitle.SetBounds(26, 65, 185, 25);
            sidebar.Controls.Add(brandSubtitle);
            this.brandSubtitle = brandSubtitle;

            dashboardNav = CreateNav("Panou general", 126);
            candidatesNav = CreateNav("Candidați", 184);
            samplesNav = CreateNav("Esantioane", 242);
            admissionNav = CreateNav("Repartizare", 300);
            dashboardNav.Click += delegate { Navigate("dashboard"); };
            candidatesNav.Click += delegate { Navigate("candidates"); };
            samplesNav.Click += delegate { Navigate("samples"); };
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

            languageButton = new SmoothButton
            {
                Text = "English",
                Style = ActionButtonStyle.Light,
                Bounds = new Rectangle(0, 13, 120, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            languageButton.Left = topBar.Width - 150;
            topBar.Controls.Add(languageButton);
            languageButton.Click += delegate { ToggleLanguage(); };
            topBar.Resize += delegate
            {
                languageButton.Left = topBar.Width - 150;
            };

            pageHost = new PagePanel { Dock = DockStyle.Fill };
            Controls.Add(pageHost);
            pageHost.BringToFront();

            ApplyChromeLanguage();
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

        private string T(string ro, string en)
        {
            return englishUi ? en : ro;
        }

        private void ToggleLanguage()
        {
            string pageKey = string.IsNullOrWhiteSpace(currentPageKey)
                ? "dashboard"
                : currentPageKey;
            englishUi = !englishUi;
            ApplyChromeLanguage();
            currentPageKey = null;
            Navigate(pageKey);
        }

        private void ApplyChromeLanguage()
        {
            Text = T("Admitere - repartizare candidati",
                "Admissions - candidate allocation");
            brandTitle.Text = T("ADMITERE", "ADMISSIONS");
            brandSubtitle.Text = T("REPARTIZARE CANDIDATI", "CANDIDATE ALLOCATION");
            dashboardNav.Text = T("Panou general", "Dashboard");
            candidatesNav.Text = T("Candidati", "Candidates");
            samplesNav.Text = T("Esantioane", "Samples");
            admissionNav.Text = T("Repartizare", "Allocation");
            languageButton.Text = englishUi ? "Romana" : "English";
            SetPageKicker(string.IsNullOrWhiteSpace(currentPageKey)
                ? "dashboard"
                : currentPageKey);
        }

        private void SetPageKicker(string key)
        {
            if (key == "candidates")
                pageKicker.Text = T("CANDIDATI", "CANDIDATES");
            else if (key == "samples")
                pageKicker.Text = T("ESANTIOANE", "SAMPLES");
            else if (key == "admission")
                pageKicker.Text = T("REPARTIZARE", "ALLOCATION");
            else
                pageKicker.Text = T("PANOU GENERAL", "DASHBOARD");
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
            if (key == "samples")
            {
                nextPage = BuildSamplesPage();
                pageKicker.Text = "ESANTIOANE";
            }
            else if (key == "candidates")
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
            SetPageKicker(key);
            dashboardNav.Active = key == "dashboard";
            candidatesNav.Active = key == "candidates";
            samplesNav.Active = key == "samples";
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

            if (englishUi)
            {
                page.Controls[0].Text = "Dashboard";
                page.Controls[1].Text =
                    "Work with separate samples and test admission scenarios quickly.";
            }

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
                LoadDashboardOverview(overview, SelectedSampleId(sampleCombo), englishUi);
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
            if (englishUi)
            {
                newSample.Text = "New sample";
                demo.Text = "Generate demo";
                syncTitle.Text = "Import from online form";
                syncText.Text = "Choose the student sample for web submissions.";
                syncStatus.Text = WebSyncService.IsConfigured
                    ? "Ready to import"
                    : "Import is not configured";
                sync.Text = "Import now";
            }
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
            if (englishUi)
            {
                page.Controls[0].Text = "Candidate list";
                page.Controls[1].Text =
                    "View all data received from the form for the selected sample.";
                criterion.Items.Clear();
                criterion.Items.AddRange(new[] { "Name", "CNP", "ID", "Status" });
                criterion.SelectedIndex = 0;
                search.Text = "Search";
                refresh.Text = "Show all";
                selectedHint.Text =
                    "Select a row to update status or delete the candidate.";
                applyStatus.Text = "Apply status";
                delete.Text = "Delete candidate";
            }
            return page;
        }

        private Control BuildSamplesPage()
        {
            PagePanel page = NewPage();
            AddPageHeading(page,
                T("Esantioane si suite de optiuni",
                  "Student samples and option suites"),
                T("Alege anul/lista de studenti in stanga, apoi selecteaza suita de facultati si specializari pentru acel an.",
                  "Choose the student list on the left, then select the faculty/specialization suite for that year."));

            int top = 100;
            int leftWidth = 330;
            int gap = 20;
            GlassPanel studentsCard = new GlassPanel
            {
                Bounds = new Rectangle(40, top, leftWidth, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            page.Controls.Add(studentsCard);
            Label studentsTitle = MakeLabel(T("1. Esantioane de studenti",
                    "1. Student samples"),
                13.5F, FontStyle.Bold, ModernPalette.Ink);
            studentsTitle.SetBounds(24, 18, 260, 30);
            studentsCard.Controls.Add(studentsTitle);
            Label studentsHint = MakeLabel(T("Click pe anul/lista cu care lucrezi.",
                    "Click the year/list you want to use."),
                9F, FontStyle.Regular, ModernPalette.Muted);
            studentsHint.SetBounds(24, 48, 270, 24);
            studentsCard.Controls.Add(studentsHint);
            DataGridView studentsGrid = CreateGrid();
            studentsGrid.SetBounds(20, 82, studentsCard.Width - 40,
                studentsCard.Height - 104);
            studentsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                  AnchorStyles.Left | AnchorStyles.Right;
            studentsCard.Controls.Add(studentsGrid);

            GlassPanel optionsCard = new GlassPanel
            {
                Bounds = new Rectangle(40 + leftWidth + gap, top,
                    page.Width - leftWidth - gap - 80, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(optionsCard);
            Label optionsTitle = MakeLabel(T("2. Suita de optiuni asociata",
                    "2. Associated option suite"),
                13.5F, FontStyle.Bold, ModernPalette.Ink);
            optionsTitle.SetBounds(24, 18, 360, 30);
            optionsCard.Controls.Add(optionsTitle);
            Label activeAssociation = MakeLabel("", 10F,
                FontStyle.Bold, ModernPalette.Blue);
            activeAssociation.SetBounds(24, 50, 520, 28);
            optionsCard.Controls.Add(activeAssociation);
            Label optionsHint = MakeLabel(T("Click pe o suita de optiuni ca s-o folosesti pentru esantionul selectat.",
                    "Click an option suite to use it for the selected student sample."),
                9F, FontStyle.Regular, ModernPalette.Muted);
            optionsHint.SetBounds(24, 78, 720, 24);
            optionsCard.Controls.Add(optionsHint);
            DataGridView optionsGrid = CreateGrid();
            optionsGrid.SetBounds(20, 108, optionsCard.Width - 40,
                optionsCard.Height - 130);
            optionsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                 AnchorStyles.Left | AnchorStyles.Right;
            optionsCard.Controls.Add(optionsGrid);

            GlassPanel builderCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 382, page.Width - 80,
                    page.Height - 407),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(builderCard);
            Label builderTitle = MakeLabel(T("3. Creeaza un esantion nou de optiuni",
                    "3. Create a new option suite"),
                14F, FontStyle.Bold, ModernPalette.Ink);
            builderTitle.SetBounds(24, 18, 440, 30);
            builderCard.Controls.Add(builderTitle);
            Label builderText = MakeLabel(T("Alege daca pornesti de la zero sau copiezi locurile si specializarile dintr-un sablon.",
                    "Choose whether to start from scratch or copy seats and specializations from a template."),
                9.2F, FontStyle.Regular, ModernPalette.Muted);
            builderText.SetBounds(25, 50, 690, 25);
            builderCard.Controls.Add(builderText);

            Label nameLabel = MakeLabel(T("Nume suita:", "Suite name:"),
                9.2F, FontStyle.Bold, ModernPalette.Muted);
            nameLabel.SetBounds(24, 92, 130, 24);
            builderCard.Controls.Add(nameLabel);
            TextBox optionName = StyledTextBox();
            optionName.SetBounds(24, 118, 270, 32);
            builderCard.Controls.Add(optionName);

            RadioButton fromZero = new RadioButton
            {
                Text = T("De la zero", "From scratch"),
                Checked = true,
                BackColor = Color.Transparent,
                ForeColor = ModernPalette.Ink,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Left = 24,
                Top = 166,
                Width = 150
            };
            builderCard.Controls.Add(fromZero);
            RadioButton fromTemplate = new RadioButton
            {
                Text = T("Din sablon existent", "From existing template"),
                BackColor = Color.Transparent,
                ForeColor = ModernPalette.Ink,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Left = 180,
                Top = 166,
                Width = 220
            };
            builderCard.Controls.Add(fromTemplate);

            Label facultyCountLabel = MakeLabel(T("Nr. facultati:", "Faculty count:"),
                9.2F, FontStyle.Bold, ModernPalette.Muted);
            facultyCountLabel.SetBounds(24, 204, 150, 24);
            builderCard.Controls.Add(facultyCountLabel);
            NumericUpDown facultyCount = StyledNumber(1, 40, 7);
            facultyCount.SetBounds(180, 202, 96, 34);
            builderCard.Controls.Add(facultyCount);

            Label templateLabel = MakeLabel(T("Sablon:", "Template:"),
                9.2F, FontStyle.Bold, ModernPalette.Muted);
            templateLabel.SetBounds(24, 204, 80, 24);
            builderCard.Controls.Add(templateLabel);
            ComboBox templateCombo = CreateOptionCombo();
            templateCombo.SetBounds(110, 202, 360, 34);
            templateCombo.Enabled = false;
            builderCard.Controls.Add(templateCombo);

            SmoothButton createOption = new SmoothButton
            {
                Text = T("Creeaza suita", "Create suite"),
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(24, 252, 190, 54)
            };
            builderCard.Controls.Add(createOption);
            Label creationStatus = MakeLabel("", 9.3F,
                FontStyle.Bold, ModernPalette.Muted);
            creationStatus.SetBounds(230, 266, 430, 25);
            builderCard.Controls.Add(creationStatus);

            Label catalogTitle = MakeLabel(T("Catalogul suitei selectate",
                    "Selected suite catalog"),
                13.5F, FontStyle.Bold, ModernPalette.Ink);
            catalogTitle.SetBounds(680, 88, 320, 28);
            catalogTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(catalogTitle);
            Label editHint = MakeLabel(T("Facultate | Cod | Specializare | Locuri",
                    "Faculty | Code | Specialization | Seats"),
                8.5F, FontStyle.Bold, ModernPalette.Muted);
            editHint.SetBounds(680, 104, 520, 18);
            editHint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(editHint);

            TextBox editFacultyName = StyledTextBox();
            editFacultyName.SetBounds(680, 122, 205, 32);
            editFacultyName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(editFacultyName);
            TextBox editFacultyCode = StyledTextBox();
            editFacultyCode.SetBounds(893, 122, 74, 32);
            editFacultyCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(editFacultyCode);
            TextBox editSpecializationName = StyledTextBox();
            editSpecializationName.SetBounds(975, 122, 210, 32);
            editSpecializationName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(editSpecializationName);
            NumericUpDown editSeats = StyledNumber(1, 1000, 30);
            editSeats.SetBounds(1193, 122, 74, 32);
            editSeats.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            builderCard.Controls.Add(editSeats);

            SmoothButton addCatalogRow = new SmoothButton
            {
                Text = T("Adauga", "Add"),
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(680, 160, 118, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            builderCard.Controls.Add(addCatalogRow);
            SmoothButton saveCatalogRow = new SmoothButton
            {
                Text = T("Salveaza", "Save"),
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(806, 160, 118, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            builderCard.Controls.Add(saveCatalogRow);
            SmoothButton deleteSpecialization = new SmoothButton
            {
                Text = T("Sterge spec.", "Delete spec."),
                Style = ActionButtonStyle.Danger,
                Bounds = new Rectangle(932, 160, 140, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            builderCard.Controls.Add(deleteSpecialization);
            SmoothButton deleteFaculty = new SmoothButton
            {
                Text = T("Sterge facultate", "Delete faculty"),
                Style = ActionButtonStyle.Danger,
                Bounds = new Rectangle(1080, 160, 170, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            builderCard.Controls.Add(deleteFaculty);

            DataGridView catalogGrid = CreateGrid();
            catalogGrid.SetBounds(680, 220, builderCard.Width - 710,
                builderCard.Height - 246);
            catalogGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                 AnchorStyles.Left | AnchorStyles.Right;
            builderCard.Controls.Add(catalogGrid);

            bool loading = false;
            Action updateCreateModeVisibility = delegate
            {
                bool zeroMode = fromZero.Checked;
                facultyCountLabel.Visible = zeroMode;
                facultyCount.Visible = zeroMode;
                templateLabel.Visible = !zeroMode;
                templateCombo.Visible = !zeroMode;
                templateCombo.Enabled = !zeroMode;
            };
            Func<int> selectedOptionId = delegate
            {
                int optionId;
                if (TryGetSelectedGridInt(optionsGrid, "IdEsantionOptiuni", out optionId))
                    return optionId;

                int sampleId;
                if (!TryGetSelectedGridInt(studentsGrid, "IdEsantion", out sampleId))
                    sampleId = DatabaseManager.GetDefaultSampleId();
                return DatabaseManager.GetDefaultOptionSampleId(sampleId);
            };
            Action loadCatalogEditor = delegate
            {
                editFacultyName.Text = CurrentGridCellText(catalogGrid, "Facultate");
                editFacultyCode.Text = CurrentGridCellText(catalogGrid, "Cod");
                editSpecializationName.Text =
                    CurrentGridCellText(catalogGrid, "Specializare");
                int parsedSeats;
                if (int.TryParse(CurrentGridCellText(catalogGrid, "Locuri"),
                    out parsedSeats))
                {
                    editSeats.Value = Math.Max(editSeats.Minimum,
                        Math.Min(editSeats.Maximum, parsedSeats));
                }
            };
            Action<int> selectOptionRow = delegate(int optionId)
            {
                foreach (DataGridViewRow row in optionsGrid.Rows)
                {
                    if (row.Cells["IdEsantionOptiuni"].Value == null)
                        continue;
                    if (Convert.ToInt32(row.Cells["IdEsantionOptiuni"].Value) == optionId)
                    {
                        row.Selected = true;
                        optionsGrid.CurrentCell = row.Cells["Nume"];
                        break;
                    }
                }
            };
            Action refreshOptions = delegate
            {
                int sampleId;
                if (!TryGetSelectedGridInt(studentsGrid, "IdEsantion", out sampleId))
                    sampleId = DatabaseManager.GetDefaultSampleId();

                int optionId = DatabaseManager.GetDefaultOptionSampleId(sampleId);
                loading = true;
                optionsGrid.DataSource = DatabaseManager.GetOptionSamples();
                HideColumn(optionsGrid, "IdEsantionOptiuni");
                HideColumn(optionsGrid, "EsantioaneStudenti");
                selectOptionRow(optionId);
                loading = false;
                string optionNameText = CurrentGridCellText(optionsGrid, "Nume");
                activeAssociation.Text = T("Asociat acum: ", "Currently associated: ") +
                    (string.IsNullOrWhiteSpace(optionNameText) ? "-" : optionNameText);
                LoadCatalogGrid(catalogGrid, optionId);
                loadCatalogEditor();
            };
            Action refreshStudents = delegate
            {
                loading = true;
                studentsGrid.DataSource = DatabaseManager.GetSamples();
                HideColumn(studentsGrid, "IdEsantion");
                loading = false;
                refreshOptions();
            };

            refreshStudents();
            updateCreateModeVisibility();

            studentsGrid.CellClick += delegate { refreshOptions(); };
            studentsGrid.SelectionChanged += delegate
            {
                if (!loading)
                    refreshOptions();
            };
            optionsGrid.CellClick += delegate
            {
                if (loading)
                    return;
                int sampleId;
                int optionId;
                if (!TryGetSelectedGridInt(studentsGrid, "IdEsantion", out sampleId) ||
                    !TryGetSelectedGridInt(optionsGrid, "IdEsantionOptiuni", out optionId))
                    return;

                try
                {
                    DatabaseManager.SetDefaultOptionSampleForStudent(sampleId, optionId);
                    refreshOptions();
                    creationStatus.Text = T("Asocierea a fost actualizata.",
                        "Association updated.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, T("Asociere", "Association"));
                }
            };
            fromZero.CheckedChanged += delegate
            {
                updateCreateModeVisibility();
            };
            fromTemplate.CheckedChanged += delegate
            {
                updateCreateModeVisibility();
            };
            createOption.Click += delegate
            {
                int sampleId;
                if (!TryGetSelectedGridInt(studentsGrid, "IdEsantion", out sampleId))
                {
                    MessageBox.Show(T("Selecteaza intai un esantion de studenti.",
                        "Select a student sample first."));
                    return;
                }

                string name = optionName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show(T("Scrie numele suitei de optiuni.",
                        "Enter the option suite name."));
                    return;
                }

                try
                {
                    int optionId;
                    if (fromTemplate.Checked)
                    {
                        optionId = DatabaseManager.CreateOptionSampleFromTemplate(
                            name, SelectedOptionSampleId(templateCombo), sampleId);
                    }
                    else
                    {
                        List<FacultyDraft> draft;
                        if (!TryCollectZeroOptionSampleDetails(
                            Convert.ToInt32(facultyCount.Value), out draft))
                            return;

                        optionId = DatabaseManager.CreateEmptyOptionSample(name, sampleId);
                        foreach (FacultyDraft faculty in draft)
                        {
                            int facultyId = DatabaseManager.SaveFaculty(
                                faculty.Name, faculty.Code);
                            foreach (SpecializationDraft specialization in faculty.Specializations)
                            {
                                DatabaseManager.SaveSpecializationInOptionSample(
                                    optionId, facultyId, specialization.Name,
                                    specialization.Seats);
                            }
                        }
                    }

                    DatabaseManager.SetDefaultOptionSampleForStudent(sampleId, optionId);
                    optionName.Clear();
                    ReloadOptionCombo(templateCombo);
                    refreshOptions();
                    selectOptionRow(optionId);
                    creationStatus.Text = T("Suita a fost creata si asociata.",
                        "Suite created and associated.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    creationStatus.Text = T("Crearea a esuat.", "Creation failed.");
                    creationStatus.ForeColor = ModernPalette.Coral;
                    MessageBox.Show(ex.Message,
                        T("Creare esantion de optiuni", "Create option suite"));
                }
            };

            catalogGrid.SelectionChanged += delegate
            {
                if (!loading)
                    loadCatalogEditor();
            };

            addCatalogRow.Click += delegate
            {
                try
                {
                    int optionId = selectedOptionId();
                    DatabaseManager.AddSpecializationToOptionSample(
                        optionId, editFacultyName.Text, editFacultyCode.Text,
                        editSpecializationName.Text, Convert.ToInt32(editSeats.Value));
                    refreshOptions();
                    creationStatus.Text = T("Specializarea a fost adaugata.",
                        "Specialization added.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Adaugare specializare", "Add specialization"));
                }
            };

            saveCatalogRow.Click += delegate
            {
                int facultyId;
                int specializationId;
                if (!TryGetSelectedGridInt(catalogGrid, "ID facultate", out facultyId) ||
                    !TryGetSelectedGridInt(catalogGrid, "ID specializare",
                        out specializationId))
                {
                    MessageBox.Show(T("Selecteaza un rand din catalog.",
                        "Select a catalog row."));
                    return;
                }

                try
                {
                    DatabaseManager.UpdateOptionSampleCatalogItem(
                        selectedOptionId(), facultyId, specializationId,
                        editFacultyName.Text, editFacultyCode.Text,
                        editSpecializationName.Text,
                        Convert.ToInt32(editSeats.Value));
                    refreshOptions();
                    creationStatus.Text = T("Modificarile au fost salvate.",
                        "Changes saved.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Modificare catalog", "Edit catalog"));
                }
            };

            deleteSpecialization.Click += delegate
            {
                int specializationId;
                if (!TryGetSelectedGridInt(catalogGrid, "ID specializare",
                    out specializationId))
                {
                    MessageBox.Show(T("Selecteaza specializarea de sters.",
                        "Select the specialization to delete."));
                    return;
                }

                if (MessageBox.Show(T(
                    "Stergi specializarea doar din suita selectata?",
                    "Delete the specialization only from the selected suite?"),
                    T("Confirmare", "Confirm"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                    return;

                try
                {
                    DatabaseManager.DeleteSpecializationFromOptionSample(
                        selectedOptionId(), specializationId);
                    refreshOptions();
                    creationStatus.Text = T("Specializarea a fost stearsa.",
                        "Specialization deleted.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Stergere specializare", "Delete specialization"));
                }
            };

            deleteFaculty.Click += delegate
            {
                int facultyId;
                if (!TryGetSelectedGridInt(catalogGrid, "ID facultate",
                    out facultyId))
                {
                    MessageBox.Show(T("Selecteaza o specializare din facultatea de sters.",
                        "Select a specialization from the faculty to delete."));
                    return;
                }

                if (MessageBox.Show(T(
                    "Stergi facultatea si toate specializarile ei doar din suita selectata?",
                    "Delete the faculty and all its specializations only from the selected suite?"),
                    T("Confirmare", "Confirm"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                    return;

                try
                {
                    DatabaseManager.DeleteFacultyFromOptionSample(
                        selectedOptionId(), facultyId);
                    refreshOptions();
                    creationStatus.Text = T("Facultatea a fost stearsa din suita.",
                        "Faculty deleted from suite.");
                    creationStatus.ForeColor = ModernPalette.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Stergere facultate", "Delete faculty"));
                }
            };

            return page;
        }

        private Control BuildSamplesPageLegacy()
        {
            PagePanel page = NewPage();
            AddPageHeading(page,
                T("Esantioane, optiuni si locuri",
                  "Samples, option sets and seats"),
                T("Administreaza listele de studenti, scenariile de facultati/specializari si legaturile dintre ele.",
                  "Manage student lists, faculty/specialization scenarios and their associations."));

            int cardGap = 20;
            int top = 100;
            int cardHeight = 250;
            int cardWidth = Math.Max(270, (page.Width - 100) / 3);

            GlassPanel studentsCard = new GlassPanel
            {
                Bounds = new Rectangle(40, top, cardWidth, cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            page.Controls.Add(studentsCard);
            Label studentsTitle = MakeLabel(T("Esantioane studenti", "Student samples"),
                13.2F, FontStyle.Bold, ModernPalette.Ink);
            studentsTitle.SetBounds(22, 18, cardWidth - 45, 28);
            studentsCard.Controls.Add(studentsTitle);
            DataGridView studentsGrid = CreateGrid();
            studentsGrid.SetBounds(18, 54, studentsCard.Width - 36, 120);
            studentsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                  AnchorStyles.Right;
            studentsCard.Controls.Add(studentsGrid);
            SmoothButton addStudentSample = new SmoothButton
            {
                Text = T("Esantion nou", "New sample"),
                Style = ActionButtonStyle.Light,
                Bounds = new Rectangle(18, 184, 170, 50)
            };
            studentsCard.Controls.Add(addStudentSample);

            GlassPanel optionsCard = new GlassPanel
            {
                Bounds = new Rectangle(40 + cardWidth + cardGap, top,
                    cardWidth, cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            page.Controls.Add(optionsCard);
            Label optionsTitle = MakeLabel(T("Esantioane optiuni", "Option samples"),
                13.2F, FontStyle.Bold, ModernPalette.Ink);
            optionsTitle.SetBounds(22, 18, cardWidth - 45, 28);
            optionsCard.Controls.Add(optionsTitle);
            ComboBox optionOwner = CreateSampleCombo();
            optionOwner.SetBounds(18, 54, optionsCard.Width - 196, 34);
            optionOwner.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                 AnchorStyles.Right;
            optionsCard.Controls.Add(optionOwner);
            SmoothButton addOptionSample = new SmoothButton
            {
                Text = T("Optiuni nou", "New options"),
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(optionsCard.Width - 172, 46, 150, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            optionsCard.Controls.Add(addOptionSample);
            DataGridView optionsGrid = CreateGrid();
            optionsGrid.SetBounds(18, 104, optionsCard.Width - 36, 130);
            optionsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                AnchorStyles.Right;
            optionsCard.Controls.Add(optionsGrid);

            GlassPanel associationsCard = new GlassPanel
            {
                Bounds = new Rectangle(40 + cardWidth * 2 + cardGap * 2, top,
                    page.Width - (40 + cardWidth * 2 + cardGap * 2) - 40,
                    cardHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(associationsCard);
            Label associationsTitle = MakeLabel(T("Asocieri", "Associations"),
                13.2F, FontStyle.Bold, ModernPalette.Ink);
            associationsTitle.SetBounds(22, 18, 260, 28);
            associationsCard.Controls.Add(associationsTitle);
            DataGridView associationsGrid = CreateGrid();
            associationsGrid.SetBounds(18, 54, associationsCard.Width - 36, 120);
            associationsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                     AnchorStyles.Right;
            associationsCard.Controls.Add(associationsGrid);
            ComboBox associationStudent = CreateSampleCombo();
            associationStudent.SetBounds(18, 186, 160, 34);
            associationsCard.Controls.Add(associationStudent);
            ComboBox associationOption = CreateOptionCombo();
            associationOption.SetBounds(188, 186,
                Math.Max(150, associationsCard.Width - 372), 34);
            associationOption.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                                      AnchorStyles.Right;
            associationsCard.Controls.Add(associationOption);
            SmoothButton associate = new SmoothButton
            {
                Text = T("Leaga", "Link"),
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(associationsCard.Width - 174, 178, 150, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            associationsCard.Controls.Add(associate);

            GlassPanel catalogCard = new GlassPanel
            {
                Bounds = new Rectangle(40, 370, page.Width - 80,
                    page.Height - 395),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            page.Controls.Add(catalogCard);
            Label catalogTitle = MakeLabel(T("Facultati si specializari",
                    "Faculties and specializations"),
                14F, FontStyle.Bold, ModernPalette.Ink);
            catalogTitle.SetBounds(24, 18, 330, 30);
            catalogCard.Controls.Add(catalogTitle);
            ComboBox catalogOption = CreateOptionCombo();
            catalogOption.SetBounds(catalogCard.Width - 300, 18, 270, 34);
            catalogOption.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            catalogCard.Controls.Add(catalogOption);

            DataGridView catalogGrid = CreateGrid();
            catalogGrid.SetBounds(20, 64, catalogCard.Width - 40,
                Math.Max(130, catalogCard.Height - 184));
            catalogGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                 AnchorStyles.Left | AnchorStyles.Right;
            catalogCard.Controls.Add(catalogGrid);

            TextBox facultyName = StyledTextBox();
            facultyName.SetBounds(20, catalogCard.Height - 100, 210, 32);
            facultyName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            catalogCard.Controls.Add(facultyName);
            TextBox facultyCode = StyledTextBox();
            facultyCode.SetBounds(238, catalogCard.Height - 100, 70, 32);
            facultyCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            catalogCard.Controls.Add(facultyCode);
            SmoothButton saveFaculty = new SmoothButton
            {
                Text = T("Salveaza facultate", "Save faculty"),
                Style = ActionButtonStyle.Light,
                Bounds = new Rectangle(314, catalogCard.Height - 111, 168, 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            catalogCard.Controls.Add(saveFaculty);

            ComboBox facultyCombo = CreateFacultyCombo();
            facultyCombo.SetBounds(500, catalogCard.Height - 100, 220, 32);
            facultyCombo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            catalogCard.Controls.Add(facultyCombo);
            TextBox specializationName = StyledTextBox();
            specializationName.SetBounds(730, catalogCard.Height - 100, 220, 32);
            specializationName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            catalogCard.Controls.Add(specializationName);
            NumericUpDown seats = StyledNumber(1, 500, 30);
            seats.SetBounds(958, catalogCard.Height - 100, 78, 32);
            seats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            catalogCard.Controls.Add(seats);
            SmoothButton saveSpecialization = new SmoothButton
            {
                Text = T("Adauga", "Add"),
                Style = ActionButtonStyle.Secondary,
                Bounds = new Rectangle(1042, catalogCard.Height - 111, 120, 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            catalogCard.Controls.Add(saveSpecialization);
            SmoothButton updateSeats = new SmoothButton
            {
                Text = T("Schimba locuri", "Update seats"),
                Style = ActionButtonStyle.Primary,
                Bounds = new Rectangle(catalogCard.Width - 185,
                    catalogCard.Height - 111, 160, 50),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            catalogCard.Controls.Add(updateSeats);

            Action reloadAll = delegate
            {
                studentsGrid.DataSource = DatabaseManager.GetSamples();
                optionsGrid.DataSource = DatabaseManager.GetOptionSamples();
                associationsGrid.DataSource = DatabaseManager.GetSampleAssociations();
                ReloadSampleCombo(optionOwner);
                ReloadSampleCombo(associationStudent);
                ReloadOptionCombo(associationOption);
                ReloadOptionCombo(catalogOption);
                ReloadFacultyCombo(facultyCombo);
                LoadCatalogGrid(catalogGrid, SelectedOptionSampleId(catalogOption));
            };

            Action reloadCatalog = delegate
            {
                LoadCatalogGrid(catalogGrid, SelectedOptionSampleId(catalogOption));
            };

            catalogOption.SelectedIndexChanged += delegate { reloadCatalog(); };
            reloadAll();

            addStudentSample.Click += delegate
            {
                string name = PromptForText(T("Esantion nou", "New sample"),
                    T("Numele esantionului:", "Sample name:"),
                    T("Esantion ", "Sample ") + DateTime.Now.ToString("dd.MM HH:mm"));
                if (string.IsNullOrWhiteSpace(name))
                    return;
                try
                {
                    DatabaseManager.CreateSample(name);
                    reloadAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, T("Esantion nou", "New sample"));
                }
            };

            addOptionSample.Click += delegate
            {
                string name = PromptForText(T("Esantion optiuni nou", "New option sample"),
                    T("Numele esantionului de optiuni:", "Option sample name:"),
                    T("Optiuni ", "Options ") + DateTime.Now.ToString("dd.MM HH:mm"));
                if (string.IsNullOrWhiteSpace(name))
                    return;
                try
                {
                    int optionId = DatabaseManager.CreateOptionSample(
                        name, SelectedSampleId(optionOwner));
                    reloadAll();
                    SetComboSelectedValue(catalogOption, optionId);
                    reloadCatalog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Esantion optiuni", "Option sample"));
                }
            };

            associate.Click += delegate
            {
                try
                {
                    DatabaseManager.AssociateOptionSample(
                        SelectedSampleId(associationStudent),
                        SelectedOptionSampleId(associationOption));
                    reloadAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, T("Asociere", "Association"));
                }
            };

            saveFaculty.Click += delegate
            {
                try
                {
                    int facultyId = DatabaseManager.SaveFaculty(
                        facultyName.Text, facultyCode.Text);
                    facultyName.Clear();
                    facultyCode.Clear();
                    reloadAll();
                    SetComboSelectedValue(facultyCombo, facultyId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, T("Facultate", "Faculty"));
                }
            };

            saveSpecialization.Click += delegate
            {
                try
                {
                    DatabaseManager.SaveSpecializationInOptionSample(
                        SelectedOptionSampleId(catalogOption),
                        SelectedFacultyId(facultyCombo),
                        specializationName.Text,
                        Convert.ToInt32(seats.Value));
                    specializationName.Clear();
                    reloadCatalog();
                    ReloadOptionCombo(associationOption);
                    ReloadOptionCombo(catalogOption);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Specializare", "Specialization"));
                }
            };

            updateSeats.Click += delegate
            {
                int specializationId;
                if (!TryGetSelectedGridInt(catalogGrid,
                    "ID specializare", out specializationId))
                {
                    MessageBox.Show(T("Selecteaza o specializare din tabel.",
                        "Select a specialization from the table."));
                    return;
                }
                try
                {
                    DatabaseManager.UpdateOptionSeat(
                        SelectedOptionSampleId(catalogOption),
                        specializationId,
                        Convert.ToInt32(seats.Value));
                    reloadCatalog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,
                        T("Locuri", "Seats"));
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
            sampleCombo.SetBounds(30, 94, 180, 34);
            controlsCard.Controls.Add(sampleCombo);

            ComboBox algorithm = CreateAlgorithmCombo();
            algorithm.SetBounds(220, 94, 245, 34);
            controlsCard.Controls.Add(algorithm);

            Label associatedSuite = MakeLabel("", 9.2F,
                FontStyle.Bold, ModernPalette.Blue);
            associatedSuite.SetBounds(482, 94, 360, 34);
            controlsCard.Controls.Add(associatedSuite);

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
                    DatabaseManager.GetDefaultOptionSampleId(
                        SelectedSampleId(sampleCombo)),
                    SelectedAlgorithm(algorithm));
            };
            Action refreshAssociatedSuite = delegate
            {
                DataTable associated = DatabaseManager.GetAssociatedOptionSamples(
                    SelectedSampleId(sampleCombo));
                string suiteName = associated.Rows.Count == 0
                    ? "-"
                    : associated.Rows[0]["Nume"].ToString();
                associatedSuite.Text = T("Suita asociata: ", "Associated suite: ") +
                    suiteName;
            };
            refreshAssociatedSuite();
            loadResults();
            sampleCombo.SelectedIndexChanged += delegate
            {
                refreshAssociatedSuite();
                loadResults();
            };
            algorithm.SelectedIndexChanged += delegate { loadResults(); };
            run.Click += delegate
            {
                try
                {
                    int count = DatabaseManager.RunAdmission(
                        SelectedSampleId(sampleCombo),
                        DatabaseManager.GetDefaultOptionSampleId(
                            SelectedSampleId(sampleCombo)),
                        SelectedAlgorithm(algorithm));
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
                        SelectedSampleId(sampleCombo),
                        DatabaseManager.GetDefaultOptionSampleId(
                            SelectedSampleId(sampleCombo)),
                        SelectedAlgorithm(algorithm));
                    statusText.Text = "Rezultatele au fost resetate.";
                    statusText.ForeColor = ModernPalette.Muted;
                    loadResults();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Resetare nereușită");
                }
            };
            if (englishUi)
            {
                page.Controls[0].Text = "Candidate allocation";
                page.Controls[1].Text =
                    "Choose the student sample and algorithm; the associated option suite is used automatically.";
                controlsTitle.Text = "Allocation";
                statusText.Text = "Generate results for imported candidates.";
                run.Text = "Run allocation";
                reset.Text = "Reset results";
                resultsTitle.Text = "Admitted candidates";
            }
            return page;
        }

        private sealed class FacultyDraft
        {
            public string Name;
            public string Code;
            public int SpecializationCount;
            public List<SpecializationDraft> Specializations =
                new List<SpecializationDraft>();
        }

        private sealed class SpecializationDraft
        {
            public string Name;
            public int Seats;
        }

        private sealed class SpecializationInput
        {
            public FacultyDraft Faculty;
            public TextBox NameBox;
            public NumericUpDown SeatsBox;
        }

        private bool TryCollectZeroOptionSampleDetails(
            int facultyCount, out List<FacultyDraft> faculties)
        {
            faculties = new List<FacultyDraft>();
            List<TextBox> facultyNames = new List<TextBox>();
            List<TextBox> facultyCodes = new List<TextBox>();
            List<NumericUpDown> specializationCounts = new List<NumericUpDown>();

            using (Form form = new Form())
            {
                form.Text = T("Facultati in suita noua", "Faculties in new suite");
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.Font = new Font("Segoe UI", 10F);
                form.ClientSize = new Size(670, 560);

                Label title = MakeLabel(
                    T("Scrie facultatile si cate specializari are fiecare.",
                      "Enter the faculties and how many specializations each one has."),
                    11F, FontStyle.Bold, ModernPalette.Ink);
                title.SetBounds(18, 16, 630, 28);
                form.Controls.Add(title);

                Panel scroll = new Panel
                {
                    AutoScroll = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Left = 18,
                    Top = 56,
                    Width = 632,
                    Height = 420
                };
                form.Controls.Add(scroll);

                int y = 12;
                for (int i = 0; i < facultyCount; i++)
                {
                    Label index = new Label
                    {
                        Text = (i + 1).ToString() + ".",
                        Left = 10,
                        Top = y + 4,
                        Width = 28,
                        Height = 28
                    };
                    scroll.Controls.Add(index);

                    TextBox name = StyledTextBox();
                    name.SetBounds(44, y, 300, 32);
                    scroll.Controls.Add(name);
                    facultyNames.Add(name);

                    TextBox code = StyledTextBox();
                    code.SetBounds(354, y, 90, 32);
                    scroll.Controls.Add(code);
                    facultyCodes.Add(code);

                    NumericUpDown specs = StyledNumber(1, 50, 1);
                    specs.SetBounds(454, y, 70, 32);
                    scroll.Controls.Add(specs);
                    specializationCounts.Add(specs);

                    Label hint = new Label
                    {
                        Text = T("nume | cod | nr. specializari",
                            "name | code | specialization count"),
                        Left = 530,
                        Top = y + 6,
                        Width = 210,
                        Height = 24,
                        ForeColor = ModernPalette.Muted
                    };
                    scroll.Controls.Add(hint);
                    y += 42;
                }

                Button ok = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Left = 454,
                    Top = 500,
                    Width = 90,
                    Height = 34
                };
                Button cancel = new Button
                {
                    Text = T("Anuleaza", "Cancel"),
                    DialogResult = DialogResult.Cancel,
                    Left = 560,
                    Top = 500,
                    Width = 90,
                    Height = 34
                };
                form.Controls.Add(ok);
                form.Controls.Add(cancel);
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                if (form.ShowDialog(this) != DialogResult.OK)
                    return false;

                for (int i = 0; i < facultyCount; i++)
                {
                    string name = facultyNames[i].Text.Trim();
                    string code = facultyCodes[i].Text.Trim();
                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(code))
                    {
                        MessageBox.Show(T(
                            "Completeaza numele si codul pentru fiecare facultate.",
                            "Fill in the name and code for every faculty."));
                        return false;
                    }

                    faculties.Add(new FacultyDraft
                    {
                        Name = name,
                        Code = code,
                        SpecializationCount =
                            Convert.ToInt32(specializationCounts[i].Value)
                    });
                }
            }

            return TryCollectSpecializationsForFaculties(faculties);
        }

        private bool TryCollectSpecializationsForFaculties(
            List<FacultyDraft> faculties)
        {
            List<SpecializationInput> inputs = new List<SpecializationInput>();
            using (Form form = new Form())
            {
                form.Text = T("Specializari si locuri", "Specializations and seats");
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.Font = new Font("Segoe UI", 10F);
                form.ClientSize = new Size(760, 620);

                Label title = MakeLabel(T(
                        "Scrie specializarile pentru fiecare facultate si numarul de locuri.",
                        "Enter the specializations for each faculty and the number of seats."),
                    11F, FontStyle.Bold, ModernPalette.Ink);
                title.SetBounds(18, 16, 710, 28);
                form.Controls.Add(title);

                Panel scroll = new Panel
                {
                    AutoScroll = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Left = 18,
                    Top = 56,
                    Width = 724,
                    Height = 480
                };
                form.Controls.Add(scroll);

                int y = 12;
                foreach (FacultyDraft faculty in faculties)
                {
                    Label facultyLabel = new Label
                    {
                        Text = faculty.Name + " (" + faculty.Code + ")",
                        Left = 10,
                        Top = y,
                        Width = 660,
                        Height = 26,
                        Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                        ForeColor = ModernPalette.Blue
                    };
                    scroll.Controls.Add(facultyLabel);
                    y += 32;

                    for (int i = 0; i < faculty.SpecializationCount; i++)
                    {
                        Label index = new Label
                        {
                            Text = (i + 1).ToString() + ".",
                            Left = 26,
                            Top = y + 4,
                            Width = 28,
                            Height = 28
                        };
                        scroll.Controls.Add(index);

                        TextBox name = StyledTextBox();
                        name.SetBounds(60, y, 430, 32);
                        scroll.Controls.Add(name);

                        NumericUpDown seats = StyledNumber(1, 1000, 30);
                        seats.SetBounds(508, y, 80, 32);
                        scroll.Controls.Add(seats);

                        Label seatsLabel = new Label
                        {
                            Text = T("locuri", "seats"),
                            Left = 598,
                            Top = y + 6,
                            Width = 70,
                            Height = 24,
                            ForeColor = ModernPalette.Muted
                        };
                        scroll.Controls.Add(seatsLabel);

                        inputs.Add(new SpecializationInput
                        {
                            Faculty = faculty,
                            NameBox = name,
                            SeatsBox = seats
                        });
                        y += 40;
                    }
                    y += 8;
                }

                Button ok = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Left = 544,
                    Top = 562,
                    Width = 90,
                    Height = 34
                };
                Button cancel = new Button
                {
                    Text = T("Anuleaza", "Cancel"),
                    DialogResult = DialogResult.Cancel,
                    Left = 650,
                    Top = 562,
                    Width = 90,
                    Height = 34
                };
                form.Controls.Add(ok);
                form.Controls.Add(cancel);
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                if (form.ShowDialog(this) != DialogResult.OK)
                    return false;

                foreach (SpecializationInput input in inputs)
                {
                    string name = input.NameBox.Text.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        MessageBox.Show(T(
                            "Completeaza numele fiecarei specializari.",
                            "Fill in every specialization name."));
                        return false;
                    }

                    input.Faculty.Specializations.Add(new SpecializationDraft
                    {
                        Name = name,
                        Seats = Convert.ToInt32(input.SeatsBox.Value)
                    });
                }
            }

            return true;
        }

        private static string CurrentGridCellText(DataGridView grid, string columnName)
        {
            if (grid == null || grid.CurrentRow == null ||
                !grid.Columns.Contains(columnName) ||
                grid.CurrentRow.Cells[columnName].Value == null)
                return string.Empty;

            return grid.CurrentRow.Cells[columnName].Value.ToString();
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

        private static ComboBox CreateOptionCombo()
        {
            ComboBox combo = StyledCombo(new string[0]);
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantionOptiuni";
            ReloadOptionCombo(combo);
            return combo;
        }

        private static void ReloadOptionCombo(ComboBox combo)
        {
            int selectedId = SelectedOptionSampleId(combo);
            combo.DataSource = DatabaseManager.GetOptionSamples();
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantionOptiuni";
            if (selectedId > 0)
                SetComboSelectedValue(combo, selectedId);
        }

        private static ComboBox CreateAssociatedOptionCombo(int sampleId)
        {
            ComboBox combo = StyledCombo(new string[0]);
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantionOptiuni";
            ReloadAssociatedOptionCombo(combo, sampleId);
            return combo;
        }

        private static void ReloadAssociatedOptionCombo(ComboBox combo, int sampleId)
        {
            int selectedId = SelectedOptionSampleId(combo);
            combo.DataSource = DatabaseManager.GetAssociatedOptionSamples(sampleId);
            combo.DisplayMember = "Nume";
            combo.ValueMember = "IdEsantionOptiuni";
            if (selectedId > 0)
                SetComboSelectedValue(combo, selectedId);
        }

        private static int SelectedOptionSampleId(ComboBox combo)
        {
            int defaultId = DatabaseManager.GetDefaultOptionSampleId(
                DatabaseManager.GetDefaultSampleId());
            if (combo == null || combo.SelectedValue == null)
                return defaultId;

            DataRowView row = combo.SelectedItem as DataRowView;
            if (row != null)
                return Convert.ToInt32(row["IdEsantionOptiuni"]);

            int id;
            if (int.TryParse(combo.SelectedValue.ToString(), out id))
                return id;

            return defaultId;
        }

        private static ComboBox CreateFacultyCombo()
        {
            ComboBox combo = StyledCombo(new string[0]);
            combo.DisplayMember = "NumeFacultate";
            combo.ValueMember = "IdFacultate";
            ReloadFacultyCombo(combo);
            return combo;
        }

        private static void ReloadFacultyCombo(ComboBox combo)
        {
            int selectedId = SelectedFacultyId(combo);
            combo.DataSource = DatabaseManager.GetFaculties();
            combo.DisplayMember = "NumeFacultate";
            combo.ValueMember = "IdFacultate";
            if (selectedId > 0)
                SetComboSelectedValue(combo, selectedId);
        }

        private static int SelectedFacultyId(ComboBox combo)
        {
            if (combo == null || combo.SelectedValue == null)
                return 0;

            DataRowView row = combo.SelectedItem as DataRowView;
            if (row != null)
                return Convert.ToInt32(row["IdFacultate"]);

            int id;
            return int.TryParse(combo.SelectedValue.ToString(), out id) ? id : 0;
        }

        private static NumericUpDown StyledNumber(int minimum, int maximum, int value)
        {
            NumericUpDown number = new NumericUpDown
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = Math.Max(minimum, Math.Min(maximum, value)),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = ModernPalette.Ink,
                Font = new Font("Segoe UI", 10F)
            };
            return number;
        }

        private static void LoadCatalogGrid(DataGridView grid, int optionSampleId)
        {
            try
            {
                grid.DataSource = DatabaseManager.GetOptionCatalog(optionSampleId);
                HideColumn(grid, "ID facultate");
                HideColumn(grid, "ID specializare");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Catalog");
            }
        }

        private static void HideColumn(DataGridView grid, string name)
        {
            if (grid.Columns.Contains(name))
                grid.Columns[name].Visible = false;
        }

        private static bool TryGetSelectedGridInt(
            DataGridView grid, string columnName, out int value)
        {
            value = 0;
            if (grid == null || grid.CurrentRow == null ||
                !grid.Columns.Contains(columnName) ||
                grid.CurrentRow.Cells[columnName].Value == null)
                return false;

            return int.TryParse(
                grid.CurrentRow.Cells[columnName].Value.ToString(), out value);
        }

        private ComboBox CreateAlgorithmCombo()
        {
            ComboBox combo = StyledCombo(new string[0]);
            if (englishUi)
            {
                combo.Items.Add(new SelectItem(
                    "Standard: 70% baccalaureate + 30% high school", "weighted"));
                combo.Items.Add(new SelectItem("Baccalaureate priority", "bac"));
                combo.Items.Add(new SelectItem("High school priority", "liceu"));
                combo.Items.Add(new SelectItem("Equal BAC/high-school average", "balanced"));
                combo.SelectedIndex = 0;
                return combo;
            }
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
            DashboardOverviewPanel overview, int sampleId, bool english)
        {
            overview.English = english;
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
            if (criterion == "Nume" || criterion == "Name")
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
            DataGridView grid, int sampleId, int optionSampleId, string algorithm)
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
                  AND A.IdEsantionOptiuni = @IdEsantionOptiuni
                  AND A.Algoritm = @Algoritm
                ORDER BY [Medie finală] DESC";
            try
            {
                grid.DataSource = DatabaseManager.ExecuteQuery(query,
                    DatabaseManager.CreateParameter("@IdEsantion", sampleId),
                    DatabaseManager.CreateParameter("@IdEsantionOptiuni", optionSampleId),
                    DatabaseManager.CreateParameter("@Algoritm", algorithm));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Rezultate indisponibile");
            }
        }
    }
}
