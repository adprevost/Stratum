// StratumDemo/Apps/SettingsApp.cs
using Stratum.Controls;
using Stratum.Core;

namespace StratumDemo;

/// <summary>Settings panel with a sidebar nav and distinct per-category content.</summary>
public sealed class SettingsApp : Control
{
    private static readonly string[] _categories = ["System", "Personalization", "About"];

    // Row geometry must match OnPaint exactly so click hits align with drawn items.
    private const int SidebarWidth  = 160;
    private const int RowFirstY     = 56;   // centre-Y of first sidebar row (relative to content top)
    private const int RowStride     = 36;   // vertical pitch between rows

    private int    _activeCategory;
    private double _blurIntensity = 0.6;
    private bool   _draggingSlider;

    private readonly ToggleSwitch _darkMode;

    public SettingsApp()
    {
        _darkMode = new ToggleSwitch(0, 0, initialChecked: true);
        Add(_darkMode);
    }

    // ── Hit testing ──────────────────────────────────────────────────────────

    public override Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;
        int ax = AbsoluteX, ay = AbsoluteY;
        if (x < ax || x > ax + Width || y < ay || y > ay + Height) return null;

        if (x - ax < SidebarWidth) return this;   // sidebar

        // Toggle (only visible on System tab)
        if (_activeCategory == 0)
        {
            Control? hit = _darkMode.HitTest(x, y);
            if (hit != null) return hit;
        }

        // Slider (only visible on Personalization tab)
        if (_activeCategory == 1)
        {
            Rect sr = SliderRect();
            if (x >= sr.X && x <= sr.X + sr.Width && y >= sr.Y - 10 && y <= sr.Y + sr.Height + 10)
                return this;
        }

        return this;
    }

    // ── Input handling ────────────────────────────────────────────────────────

    public override void OnMouseDown(MouseEventArgs e)
    {
        int ax = AbsoluteX, ay = AbsoluteY;

        if (e.X - ax < SidebarWidth)
        {
            // Map click Y → row index using the exact same geometry as OnPaint.
            int relY = e.Y - ay - RowFirstY;
            int row  = (int)Math.Round(relY / (double)RowStride);
            if (row >= 0 && row < _categories.Length)
            {
                _activeCategory = row;
                Invalidate();
            }
            return;
        }

        if (_activeCategory == 1)
        {
            Rect sr = SliderRect();
            if (e.Y >= sr.Y - 10 && e.Y <= sr.Y + sr.Height + 10)
            {
                _draggingSlider = true;
                UpdateSlider(e.X);
            }
        }
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        if (_draggingSlider) UpdateSlider(e.X);
    }

    public override void OnMouseUp(MouseEventArgs e) => _draggingSlider = false;

    private Rect SliderRect()
    {
        int ax = AbsoluteX, ay = AbsoluteY;
        int sx = ax + SidebarWidth + 28;
        int sy = ay + 160;
        int sw = Math.Max(120, Width - SidebarWidth - 64);
        return new Rect(sx, sy, sw, 4);
    }

    private void UpdateSlider(int mouseX)
    {
        Rect sr = SliderRect();
        _blurIntensity = Math.Clamp((mouseX - sr.X) / (double)sr.Width, 0.0, 1.0);
        Invalidate();
    }

    // ── Paint ─────────────────────────────────────────────────────────────────

    public override void OnPaint(Canvas canvas)
    {
        int ax = AbsoluteX, ay = AbsoluteY;

        DrawSidebar(canvas, ax, ay);
        DrawContent(canvas, ax, ay);
    }

    private void DrawSidebar(Canvas canvas, int ax, int ay)
    {
        canvas.SetFillStyle("rgba(4, 14, 38, 0.45)");
        canvas.FillRect(ax, ay, SidebarWidth, Height);

        canvas.SetFont("600 10px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(255, 255, 255, 0.35)");
        canvas.SetTextBaseline("middle");
        canvas.FillText("SETTINGS", ax + 16, ay + 22);

        for (int i = 0; i < _categories.Length; i++)
        {
            int rowY  = ay + RowFirstY + i * RowStride;
            bool active = i == _activeCategory;

            if (active)
            {
                canvas.SetFillStyle("rgba(29, 78, 216, 0.35)");
                canvas.BeginPath();
                canvas.RoundRect(ax + 8, rowY - 14, SidebarWidth - 16, 28, 6);
                canvas.Fill();
            }

            canvas.SetFillStyle(active ? "#ffffff" : "rgba(255, 255, 255, 0.60)");
            canvas.SetFont("400 13px system-ui, -apple-system, sans-serif");
            canvas.FillText(_categories[i], ax + 18, rowY);
        }
    }

    private void DrawContent(Canvas canvas, int ax, int ay)
    {
        int cx = ax + SidebarWidth + 28;

        // Title + subtitle
        canvas.SetFillStyle("#ffffff");
        canvas.SetFont("300 22px system-ui, -apple-system, sans-serif");
        canvas.SetTextBaseline("middle");
        canvas.FillText(_categories[_activeCategory], cx, ay + 30);

        string subtitle = _activeCategory switch
        {
            0 => "Display, notifications and power.",
            1 => "Background, colors and visual style.",
            _ => "Device information and legal notices.",
        };
        canvas.SetFont("300 12px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(255, 255, 255, 0.45)");
        canvas.FillText(subtitle, cx, ay + 56);

        switch (_activeCategory)
        {
            case 0: DrawSystemTab(canvas, cx, ay); break;
            case 1: DrawPersonalizationTab(canvas, cx, ay); break;
            case 2: DrawAboutTab(canvas, cx, ay); break;
        }
    }

    private void DrawSystemTab(Canvas canvas, int cx, int ay)
    {
        // Row: Dark mode toggle
        DrawSettingRow(canvas, cx, ay + 100, "Dark mode", "Use a dark colour scheme for all surfaces.");

        // Position toggle aligned to the right of the content area
        _darkMode.X = Width - SidebarWidth - 60;
        _darkMode.Y = 90;

        // Row: Display brightness (static preview)
        DrawSettingRow(canvas, cx, ay + 156, "Brightness", "Adjust the display brightness level.");
        DrawStaticSlider(canvas, cx, ay + 175, 0.72, "#2563eb");

        // Row: Night light
        DrawSettingRow(canvas, cx, ay + 220, "Night light", "Reduces blue light to help you sleep.");
        DrawOffBadge(canvas, cx, ay + 218);
    }

    private void DrawPersonalizationTab(Canvas canvas, int cx, int ay)
    {
        DrawSettingRow(canvas, cx, ay + 100, "Blur intensity", "Controls the acrylic blur strength.");

        Rect sr = SliderRect();
        // Track
        canvas.SetFillStyle("rgba(255, 255, 255, 0.12)");
        canvas.BeginPath();
        canvas.RoundRect(sr.X, sr.Y, sr.Width, sr.Height, 2);
        canvas.Fill();
        // Fill
        int fillW = Math.Max(0, (int)(sr.Width * _blurIntensity));
        canvas.SetFillStyle("#2563eb");
        canvas.BeginPath();
        canvas.RoundRect(sr.X, sr.Y, fillW, sr.Height, 2);
        canvas.Fill();
        // Knob
        int knobX = sr.X + fillW;
        canvas.SetFillStyle("#ffffff");
        canvas.BeginPath();
        canvas.Arc(knobX, sr.Y + sr.Height / 2, 8, 0, Math.PI * 2);
        canvas.Fill();

        canvas.SetFont("300 11px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(255, 255, 255, 0.55)");
        canvas.SetTextBaseline("middle");
        canvas.FillText($"{(int)(_blurIntensity * 100)}%", sr.X + sr.Width + 14, sr.Y + 2);

        // Accent colour swatches
        DrawSettingRow(canvas, cx, ay + 200, "Accent colour", "Choose a highlight colour for the UI.");
        string[] accents = ["#1d4ed8", "#2563eb", "#1e40af", "#1a56db", "#1346b0", "#0e3a8c"];
        for (int i = 0; i < accents.Length; i++)
        {
            int sx = cx + i * 30, sy = ay + 216;
            canvas.SetFillStyle(accents[i]);
            canvas.BeginPath();
            canvas.Arc(sx + 10, sy + 10, 10, 0, Math.PI * 2);
            canvas.Fill();
        }
    }

    private void DrawAboutTab(Canvas canvas, int cx, int ay)
    {
        (string Label, string Value)[] info =
        [
            ("Framework",   "Stratum 1.0"),
            ("Runtime",     ".NET 10 WebAssembly"),
            ("Renderer",    "HTML5 Canvas 2D"),
            ("Resolution",  $"{Application.Current?.ScreenWidth ?? 0} × {Application.Current?.ScreenHeight ?? 0}"),
            ("Build",       "Debug"),
        ];

        for (int i = 0; i < info.Length; i++)
        {
            int ry = ay + 96 + i * 36;
            canvas.SetFillStyle("rgba(255, 255, 255, 0.05)");
            canvas.BeginPath();
            canvas.RoundRect(cx, ry, Math.Max(10, Width - SidebarWidth - 56), 28, 5);
            canvas.Fill();

            canvas.SetFont("400 12px system-ui, -apple-system, sans-serif");
            canvas.SetFillStyle("rgba(255, 255, 255, 0.50)");
            canvas.SetTextBaseline("middle");
            canvas.FillText(info[i].Label, cx + 14, ry + 14);

            canvas.SetFillStyle("#ffffff");
            canvas.FillText(info[i].Value, cx + 160, ry + 14);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void DrawSettingRow(Canvas canvas, int cx, int cy, string label, string desc)
    {
        canvas.SetFont("400 14px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("#ffffff");
        canvas.SetTextBaseline("middle");
        canvas.FillText(label, cx, cy);

        canvas.SetFont("300 11px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(255, 255, 255, 0.45)");
        canvas.FillText(desc, cx, cy + 17);
    }

    private static void DrawStaticSlider(Canvas canvas, int cx, int ay, double value, string color)
    {
        const int SliderW = 160, SliderH = 4;
        int sx = cx, sy = ay;
        canvas.SetFillStyle("rgba(255, 255, 255, 0.12)");
        canvas.BeginPath();
        canvas.RoundRect(sx, sy, SliderW, SliderH, 2);
        canvas.Fill();
        int fillW = (int)(SliderW * value);
        canvas.SetFillStyle(color);
        canvas.BeginPath();
        canvas.RoundRect(sx, sy, fillW, SliderH, 2);
        canvas.Fill();
        canvas.SetFillStyle("#ffffff");
        canvas.BeginPath();
        canvas.Arc(sx + fillW, sy + SliderH / 2, 7, 0, Math.PI * 2);
        canvas.Fill();
    }

    private static void DrawOffBadge(Canvas canvas, int cx, int cy)
    {
        canvas.SetFillStyle("rgba(255, 255, 255, 0.10)");
        canvas.BeginPath();
        canvas.RoundRect(cx + 240, cy, 36, 18, 9);
        canvas.Fill();
        canvas.SetFont("400 10px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(255, 255, 255, 0.50)");
        canvas.SetTextBaseline("middle");
        canvas.FillText("OFF", cx + 251, cy + 9);
    }
}

