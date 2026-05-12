// StratumDemo/Apps/FilesApp.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>"My Computer" — Quick Access sidebar plus a grid of folder tiles.</summary>
public sealed class FilesApp : Control
{
    private static readonly string[] _quickAccess =
        ["Home", "Recent", "Desktop", "Downloads", "Documents", "Pictures", "Music"];

    private static readonly (string Name, string Color)[] _folders =
    [
        ("Documents",   "#1d4ed8"),
        ("Pictures",    "#1e40af"),
        ("Music",       "#2563eb"),
        ("Videos",      "#1a56db"),
        ("Downloads",   "#1346b0"),
        ("Projects",    "#0e3a8c"),
    ];

    private int _activeQuickAccess;

    public override Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;
        int ax = AbsoluteX, ay = AbsoluteY;
        if (x < ax || x > ax + Width || y < ay || y > ay + Height) return null;
        return this;
    }

    public override void OnMouseDown(MouseEventArgs e)
    {
        const int SidebarWidth = 168;
        int ay = AbsoluteY;
        if (e.X - AbsoluteX < SidebarWidth)
        {
            int row = (e.Y - ay - 36) / 30;
            if (row >= 0 && row < _quickAccess.Length)
            {
                _activeQuickAccess = row;
                Invalidate();
            }
        }
    }

    public override void OnPaint(Canvas canvas)
    {
        int ax = AbsoluteX, ay = AbsoluteY;

        // Sidebar
        const int SidebarWidth = 168;

        canvas.SetFillStyle("rgba(4, 14, 38, 0.60)");
        canvas.FillRect(ax, ay, SidebarWidth, Height);

        canvas.SetStrokeStyle("rgba(37, 99, 200, 0.25)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(ax + SidebarWidth, ay);
        canvas.LineTo(ax + SidebarWidth, ay + Height);
        canvas.Stroke();

        canvas.SetFillStyle("rgba(147, 197, 253, 0.55)");
        canvas.SetFont("600 10px system-ui, -apple-system, sans-serif");
        canvas.SetTextBaseline("middle");
        canvas.FillText("QUICK ACCESS", ax + 16, ay + 18);

        canvas.SetFont("400 13px system-ui, -apple-system, sans-serif");
        for (int i = 0; i < _quickAccess.Length; i++)
        {
            int rowY = ay + 36 + i * 30;
            bool active = i == _activeQuickAccess;

            if (active)
            {
                canvas.SetFillStyle("rgba(29, 78, 216, 0.55)");
                canvas.BeginPath();
                canvas.RoundRect(ax + 8, rowY - 11, SidebarWidth - 16, 24, 6);
                canvas.Fill();
            }

            canvas.SetFillStyle(active ? "#ffffff" : "rgba(186, 219, 255, 0.75)");
            canvas.FillText(_quickAccess[i], ax + 18, rowY + 1);
        }

        // Content area
        int contentX = ax + SidebarWidth + 24;

        canvas.SetFillStyle("#ffffff");
        canvas.SetFont("300 22px system-ui, -apple-system, sans-serif");
        canvas.FillText(_quickAccess[_activeQuickAccess], contentX, ay + 28);

        canvas.SetFont("300 12px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(147, 197, 253, 0.60)");
        canvas.FillText("This PC > " + _quickAccess[_activeQuickAccess], contentX, ay + 50);

        // Folder tiles
        const int Tile = 100;
        const int Gap  = 16;
        int cols = Math.Max(1, (Width - SidebarWidth - 48) / (Tile + Gap));

        for (int i = 0; i < _folders.Length; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int tx = contentX + col * (Tile + Gap);
            int ty = ay + 72 + row * (Tile + Gap);

            canvas.SetFillStyle("rgba(8, 24, 60, 0.72)");
            canvas.BeginPath();
            canvas.RoundRect(tx, ty, Tile, Tile, 14);
            canvas.Fill();

            canvas.SetStrokeStyle("rgba(59, 130, 246, 0.22)");
            canvas.SetLineWidth(1);
            canvas.BeginPath();
            canvas.RoundRect(tx, ty, Tile, Tile, 14);
            canvas.Stroke();

            canvas.SetFillStyle(_folders[i].Color);
            canvas.BeginPath();
            canvas.RoundRect(tx + 20, ty + 26, Tile - 40, 34, 5);
            canvas.Fill();

            canvas.SetGlobalAlpha(0.75);
            canvas.BeginPath();
            canvas.RoundRect(tx + 20, ty + 20, 26, 10, 3);
            canvas.Fill();
            canvas.SetGlobalAlpha(1.0);

            canvas.SetFillStyle("rgba(219, 234, 254, 0.95)");
            canvas.SetFont("400 12px system-ui, -apple-system, sans-serif");
            canvas.SetTextBaseline("middle");
            double tw = canvas.MeasureText(_folders[i].Name);
            canvas.FillText(_folders[i].Name, tx + (int)((Tile - tw) / 2), ty + Tile - 13);
        }
    }
}
