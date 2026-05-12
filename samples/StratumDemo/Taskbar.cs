// StratumDemo/Taskbar.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>
/// Windows 11-style bottom taskbar with centered app icons.
/// Clicking an icon opens or focuses the matching window via callbacks.
/// </summary>
public sealed class Taskbar : Control
{
    public const int DockHeight     = 52;
    public const int DockBottomGap  = 0;
    public const int IconSize       = 32;
    public const int IconGap        = 6;
    public const int DockPaddingX   = 12;

    private const int IconCellSize  = 42;

    public IReadOnlyList<DockEntry> Entries => _entries;
    private readonly List<DockEntry> _entries = new();

    public Action<string>? IconActivated;

    public void RegisterApp(string appId, string label, string color, string glyph = "")
        => _entries.Add(new DockEntry(appId, label, color, string.IsNullOrEmpty(glyph) ? label[..1].ToUpperInvariant() : glyph));

    public void SetState(string appId, bool running, bool focused)
    {
        for (int i = 0; i < _entries.Count; i++)
            if (_entries[i].AppId == appId)
                _entries[i] = _entries[i] with { Running = running, Focused = focused };
        Invalidate();
    }

    public override void OnResize(int canvasWidth, int canvasHeight)
    {
        Width  = canvasWidth;
        Height = canvasHeight;
        Invalidate();
    }

    private (int x, int y, int w, int h) DockBounds()
    {
        int dockW = DockPaddingX * 2 + _entries.Count * IconCellSize + Math.Max(0, _entries.Count - 1) * IconGap;
        int dockX = (Width - dockW) / 2;
        int dockY = Height - DockHeight - DockBottomGap;
        return (dockX, dockY + (DockHeight - IconCellSize) / 2, dockW, IconCellSize);
    }

    public override Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;

        // The visible taskbar spans the bottom edge, but only app buttons activate.
        if (y >= Height - DockHeight - DockBottomGap && y <= Height - DockBottomGap) return this;
        return null;
    }

    public override void OnMouseDown(MouseEventArgs e)
    {
        (int dx, int dy, int _, int _) = DockBounds();
        int relX = e.X - dx - DockPaddingX;
        int idx  = relX / (IconCellSize + IconGap);
        if (idx < 0 || idx >= _entries.Count) return;
        int iconStart = idx * (IconCellSize + IconGap);
        if (relX < iconStart || relX > iconStart + IconCellSize) return;

        IconActivated?.Invoke(_entries[idx].AppId);
    }

    public override void OnPaint(Canvas canvas) => DrawDock(canvas);

    private void DrawDock(Canvas canvas)
    {
        int barY = Height - DockHeight - DockBottomGap;

        canvas.SetFillStyle("rgba(10, 12, 18, 0.72)");
        canvas.FillRect(0, barY, Width, DockHeight);

        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.08)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(0, barY);
        canvas.LineTo(Width, barY);
        canvas.Stroke();

        if (_entries.Count == 0) return;

        (int dx, int dy, _, _) = DockBounds();
        int cellY = dy;
        for (int i = 0; i < _entries.Count; i++)
        {
            int cellX = dx + DockPaddingX + i * (IconCellSize + IconGap);
            DrawIcon(canvas, _entries[i], cellX, cellY);
        }
    }

    private static void DrawIcon(Canvas canvas, DockEntry entry, int x, int y)
    {
        int iconX = x + (IconCellSize - IconSize) / 2;
        int iconY = y + 4;

        if (entry.Focused)
        {
            canvas.SetFillStyle("rgba(255, 255, 255, 0.16)");
            canvas.BeginPath();
            canvas.RoundRect(x, y, IconCellSize, IconCellSize, 8);
            canvas.Fill();

            canvas.SetStrokeStyle("rgba(255, 255, 255, 0.20)");
            canvas.SetLineWidth(1);
            canvas.BeginPath();
            canvas.RoundRect(x, y, IconCellSize, IconCellSize, 8);
            canvas.Stroke();
        }
        else
        {
            canvas.SetFillStyle(entry.Running ? "rgba(255, 255, 255, 0.08)" : "rgba(255, 255, 255, 0.035)");
            canvas.BeginPath();
            canvas.RoundRect(x, y, IconCellSize, IconCellSize, 8);
            canvas.Fill();
        }

        canvas.SetFillStyle(entry.Color);
        canvas.BeginPath();
        canvas.RoundRect(iconX, iconY, IconSize, IconSize, 8);
        canvas.Fill();

        canvas.DrawGlyph(entry.Glyph, iconX + IconSize / 2, iconY + IconSize / 2, 19, "rgba(255, 255, 255, 0.94)");

        if (entry.Running)
        {
            int indicatorW = entry.Focused ? 16 : 6;
            string color = entry.Focused ? "rgba(96, 165, 250, 0.95)" : "rgba(255, 255, 255, 0.62)";
            canvas.SetFillStyle(color);
            canvas.BeginPath();
            canvas.RoundRect(x + (IconCellSize - indicatorW) / 2, y + IconCellSize - 3, indicatorW, 3, 2);
            canvas.Fill();
        }
    }

}

public sealed record DockEntry(string AppId, string Label, string Color, string Glyph = "")
{
    public bool Running { get; init; }
    public bool Focused { get; init; }
}
