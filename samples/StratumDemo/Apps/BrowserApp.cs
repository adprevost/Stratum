// StratumDemo/Apps/BrowserApp.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>A mock browser with URL bar and a grid of "content cards".</summary>
public sealed class BrowserApp : Control
{
    private const string Url = "stratum://welcome";
    private static readonly string[] _palette =
        ["#1d4ed8", "#1e40af", "#2563eb", "#1a56db", "#1346b0", "#0e3a8c", "#1659c7", "#0c2e73"];

    public override void OnPaint(Canvas canvas)
    {
        int ax = AbsoluteX, ay = AbsoluteY;

        // URL bar background
        canvas.SetFillStyle("rgba(255, 255, 255, 0.04)");
        canvas.FillRect(ax, ay, Width, 44);

        // URL pill
        int pillX = ax + 16, pillY = ay + 10, pillW = Width - 32, pillH = 24;
        canvas.SetFillStyle("rgba(0, 0, 0, 0.35)");
        canvas.BeginPath();
        canvas.RoundRect(pillX, pillY, pillW, pillH, pillH / 2);
        canvas.Fill();
        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.08)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.RoundRect(pillX, pillY, pillW, pillH, pillH / 2);
        canvas.Stroke();

        // Lock glyph
        canvas.SetFillStyle("#3b82f6");
        canvas.BeginPath();
        canvas.RoundRect(pillX + 10, pillY + 8, 8, 9, 2);
        canvas.Fill();

        // URL text
        canvas.SetFont("400 12px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("rgba(225, 230, 245, 0.9)");
        canvas.SetTextBaseline("middle");
        canvas.FillText(Url, pillX + 28, pillY + pillH / 2);

        // Page surface
        int pageY = ay + 56;
        canvas.SetFillStyle("rgba(255, 255, 255, 0.96)");
        canvas.FillRect(ax + 12, pageY, Width - 24, Height - pageY + ay - 12);

        // Hero
        canvas.SetFillStyle("#111827");
        canvas.SetFont("600 20px system-ui, -apple-system, sans-serif");
        canvas.SetTextBaseline("middle");
        canvas.FillText("Welcome to the Stratum Web", ax + 32, pageY + 32);
        canvas.SetFont("400 12px system-ui, -apple-system, sans-serif");
        canvas.SetFillStyle("#6b7280");
        canvas.FillText("Cards rendered entirely on a single <canvas> element.", ax + 32, pageY + 54);

        // Cards grid
        const int CardW = 130, CardH = 90, Gap = 14;
        int gridX = ax + 32, gridY = pageY + 86;
        int cols = Math.Max(1, (Width - 64) / (CardW + Gap));

        for (int i = 0; i < 8; i++)
        {
            int col = i % cols, row = i / cols;
            int cx = gridX + col * (CardW + Gap);
            int cy = gridY + row * (CardH + Gap);

            canvas.SetFillStyle(_palette[i % _palette.Length]);
            canvas.BeginPath();
            canvas.RoundRect(cx, cy, CardW, CardH - 28, 8);
            canvas.Fill();

            canvas.SetFillStyle("#0a1a3a");
            canvas.SetFont("400 11px system-ui, -apple-system, sans-serif");
            canvas.SetTextBaseline("alphabetic");
            canvas.FillText($"Card {i + 1}", cx + 6, cy + CardH - 12);
        }
    }
}
