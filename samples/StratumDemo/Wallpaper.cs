// StratumDemo/Wallpaper.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>
/// Deep royal-blue wallpaper: a midnight-navy base with layered royal-blue
/// gradient blobs, a faint geometric grid, and corner vignettes. Fully static —
/// no per-frame animation — so it is cheap to repaint.
/// </summary>
public static class Wallpaper
{
    // Unused by the static wallpaper but kept so DesktopBackground compiles.
    public static double Now => 0;

    public static void Paint(Canvas canvas, int width, int height)
    {
        DrawGradientMesh(canvas, width, height);
        DrawGrid(canvas, width, height);
        DrawVignette(canvas, width, height);
    }

    // ── Gradient mesh ────────────────────────────────────────────────────────
    // Approximated by splitting the canvas into four quadrant blobs whose
    // colours match the classic Ubuntu Yaru / GNOME default palette.

    private static readonly (double cx, double cy, string color, double radius)[] _mesh =
    [
        (0.0,  0.0,  "#020c1e", 1.20),   // top-left    — near-black navy
        (1.0,  0.0,  "#03122b", 1.10),   // top-right   — deep midnight
        (0.0,  1.0,  "#051a36", 0.95),   // bottom-left — dark navy
        (1.0,  1.0,  "#071e40", 1.00),   // bottom-right— navy blue
        (0.5,  0.45, "#0f3470", 0.70),   // centre      — royal blue core
        (0.75, 0.20, "#1a4fa0", 0.45),   // top-right   — bright royal accent
        (0.20, 0.75, "#0d2d5e", 0.40),   // bottom-left — deep royal
    ];

    private static void DrawGradientMesh(Canvas canvas, int w, int h)
    {
        // Base coat
        canvas.SetFillStyle("#020b1a");
        canvas.FillRect(0, 0, w, h);

        foreach (var (cx, cy, color, radiusFactor) in _mesh)
        {
            int x = (int)(cx * w);
            int y = (int)(cy * h);
            int r = (int)(Math.Max(w, h) * radiusFactor);
            DrawSoftBlob(canvas, x, y, r, color);
        }
    }

    private static void DrawSoftBlob(Canvas canvas, int x, int y, int r, string color)
    {
        const int    Layers = 40;
        const double Sigma2 = 4.0;
        const double PeakA  = 0.06;
        for (int i = Layers; i >= 1; i--)
        {
            double t  = i / (double)Layers;
            double a  = PeakA * Math.Exp(-Sigma2 * t * t);
            int    rr = (int)(r * t);
            canvas.SetGlobalAlpha(a);
            canvas.SetFillStyle(color);
            canvas.BeginPath();
            canvas.Arc(x, y, rr, 0, Math.PI * 2);
            canvas.Fill();
        }
        canvas.SetGlobalAlpha(1.0);
    }

    // ── Subtle geometric grid ────────────────────────────────────────────────

    private static void DrawGrid(Canvas canvas, int w, int h)
    {
        const int CellSize = 64;
        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.03)");
        canvas.SetLineWidth(1);

        for (int x = 0; x < w; x += CellSize)
        {
            canvas.BeginPath();
            canvas.MoveTo(x, 0);
            canvas.LineTo(x, h);
            canvas.Stroke();
        }
        for (int y = 0; y < h; y += CellSize)
        {
            canvas.BeginPath();
            canvas.MoveTo(0, y);
            canvas.LineTo(w, y);
            canvas.Stroke();
        }

        // Dot at each intersection
        canvas.SetGlobalAlpha(0.04);
        canvas.SetFillStyle("#4a90d9");
        for (int x = 0; x <= w; x += CellSize)
        {
            for (int y = 0; y <= h; y += CellSize)
            {
                canvas.BeginPath();
                canvas.Arc(x, y, 2, 0, Math.PI * 2);
                canvas.Fill();
            }
        }
        canvas.SetGlobalAlpha(1.0);
    }

    // ── Corner vignette ──────────────────────────────────────────────────────

    private static void DrawVignette(Canvas canvas, int w, int h)
    {
        // Four corner blobs of deep black to create a natural frame.
        DrawSoftBlob(canvas, 0,   0,   (int)(Math.Max(w, h) * 0.6), "#000000");
        DrawSoftBlob(canvas, w,   0,   (int)(Math.Max(w, h) * 0.5), "#000000");
        DrawSoftBlob(canvas, 0,   h,   (int)(Math.Max(w, h) * 0.5), "#000000");
        DrawSoftBlob(canvas, w,   h,   (int)(Math.Max(w, h) * 0.6), "#000000");
    }
}
