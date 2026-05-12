// StratumDemo/GlassRenderer.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>
/// Approximates the Windows 11 / macOS acrylic-glass look on a flat 2D canvas.
/// True backdrop-filter blur is unavailable, so we layer a tinted translucent
/// fill on top of whatever was painted underneath (the wallpaper plus any
/// already-rendered windows) and decorate it with a thin highlight border.
/// </summary>
public static class GlassRenderer
{
    public static void DrawGlass(Canvas canvas, int x, int y, int w, int h, int radius, bool dark = true)
    {
        // Soft drop shadow (concentric translucent rectangles)
        DrawShadow(canvas, x, y, w, h, radius);

        // Translucent body
        string body = dark ? "rgba(30, 30, 46, 0.65)" : "rgba(255, 255, 255, 0.55)";
        canvas.SetFillStyle(body);
        canvas.BeginPath();
        canvas.RoundRect(x, y, w, h, radius);
        canvas.Fill();

        // Subtle inner highlight overlay (top half lighter)
        canvas.SetGlobalAlpha(0.07);
        canvas.SetFillStyle("#ffffff");
        canvas.BeginPath();
        canvas.RoundRect(x + 1, y + 1, w - 2, Math.Max(2, h / 2), radius);
        canvas.Fill();
        canvas.SetGlobalAlpha(1.0);

        // Glass edge
        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.35)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.RoundRect(x, y, w, h, radius);
        canvas.Stroke();
    }

    public static void DrawGlassPill(Canvas canvas, int x, int y, int w, int h)
        => DrawGlass(canvas, x, y, w, h, h / 2);

    /// <summary>Soft layered drop-shadow (multiple translucent rounded rects).</summary>
    public static void DrawShadow(Canvas canvas, int x, int y, int w, int h, int radius)
    {
        const int Layers = 6;
        for (int i = Layers; i >= 1; i--)
        {
            double alpha = 0.015 + (i / (double)Layers) * 0.03;
            canvas.SetGlobalAlpha(alpha);
            canvas.SetFillStyle("#000000");
            canvas.BeginPath();
            canvas.RoundRect(x - i, y + i + 2, w + i * 2, h + i, radius + i);
            canvas.Fill();
        }
        canvas.SetGlobalAlpha(1.0);
    }

    /// <summary>Draw a small filled circle.</summary>
    public static void DrawDot(Canvas canvas, int cx, int cy, int r, string color, double alpha = 1.0)
    {
        canvas.SetGlobalAlpha(alpha);
        canvas.SetFillStyle(color);
        canvas.BeginPath();
        canvas.Arc(cx, cy, r, 0, Math.PI * 2);
        canvas.Fill();
        canvas.SetGlobalAlpha(1.0);
    }
}
