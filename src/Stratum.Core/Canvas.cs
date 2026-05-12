// Stratum.Core/Canvas.cs
namespace Stratum.Core;

public class Canvas
{
    public int Width  => JsCanvas.GetCanvasWidth();
    public int Height => JsCanvas.GetCanvasHeight();

    public void ClearRect(int x, int y, int w, int h) => JsCanvas.ClearRect(x, y, w, h);
    public void FillRect(int x, int y, int w, int h)  => JsCanvas.FillRect(x, y, w, h);
    public void StrokeRect(int x, int y, int w, int h)=> JsCanvas.StrokeRect(x, y, w, h);

    public void FillText(string text, int x, int y)   => JsCanvas.FillText(text, x, y);
    public double MeasureText(string text)             => JsCanvas.MeasureText(text);

    public void BeginPath()                            => JsCanvas.BeginPath();
    public void ClosePath()                            => JsCanvas.ClosePath();
    public void MoveTo(int x, int y)                   => JsCanvas.MoveTo(x, y);
    public void LineTo(int x, int y)                   => JsCanvas.LineTo(x, y);
    public void Arc(int x, int y, int r, double start, double end, bool ccw = false)
                                                       => JsCanvas.Arc(x, y, r, start, end, ccw);
    public void RoundRect(int x, int y, int w, int h, int radius)
                                                       => JsCanvas.RoundRect(x, y, w, h, radius);
    public void Fill()                                 => JsCanvas.Fill();
    public void Stroke()                               => JsCanvas.Stroke();

    public void Save()                                 => JsCanvas.Save();
    public void Restore()                              => JsCanvas.Restore();
    public void SetClip(int x, int y, int w, int h)   => JsCanvas.SetClip(x, y, w, h);

    public void SetFillStyle(string color)             => JsCanvas.SetFillStyle(color);
    public void SetStrokeStyle(string color)           => JsCanvas.SetStrokeStyle(color);
    public void SetLineWidth(double w)                 => JsCanvas.SetLineWidth(w);
    public void SetFont(string font)                   => JsCanvas.SetFont(font);
    public void SetTextBaseline(string b)              => JsCanvas.SetTextBaseline(b);
    public void SetTextAlign(string a)                 => JsCanvas.SetTextAlign(a);
    public void SetGlobalAlpha(double a)               => JsCanvas.SetGlobalAlpha(a);

    public void DrawRoundedRect(int x, int y, int w, int h, int r, string fill, string stroke, double lineWidth = 1)
    {
        Save();
        SetFillStyle(fill);
        SetStrokeStyle(stroke);
        SetLineWidth(lineWidth);
        BeginPath();
        RoundRect(x, y, w, h, r);
        Fill();
        Stroke();
        Restore();
    }

    /// <summary>
    /// Draws a Material Symbols glyph centred on (<paramref name="cx"/>, <paramref name="cy"/>).
    /// Requires the "Material Symbols Rounded" font to be loaded in the host page.
    /// Use <see cref="Glyphs"/> constants for the <paramref name="ligature"/> argument.
    /// </summary>
    /// <param name="ligature">Ligature name from <see cref="Glyphs"/>, e.g. <c>Glyphs.Close</c>.</param>
    /// <param name="cx">Horizontal centre of the glyph.</param>
    /// <param name="cy">Vertical centre of the glyph.</param>
    /// <param name="size">Font size in pixels.</param>
    /// <param name="color">CSS colour string.</param>
    /// <param name="fill">FILL axis (0 = outline, 1 = filled). Defaults to 1.</param>
    /// <param name="weight">wght axis (100–700). Defaults to 400.</param>
    public void DrawGlyph(string ligature, int cx, int cy, int size = 20,
                          string color = "#ffffff", int fill = 1, int weight = 400)
    {
        Save();
        SetFont($"{weight} {size}px 'Material Symbols Rounded'");
        SetFillStyle(color);
        SetTextBaseline("middle");
        SetTextAlign("center");
        FillText(ligature, cx, cy);
        Restore();
    }
}
