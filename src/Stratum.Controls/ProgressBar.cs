// Stratum.Controls/ProgressBar.cs
using Stratum.Core;

namespace Stratum.Controls;

public class ProgressBar : Control
{
    public double Value      { get; set; } = 0;
    public bool   ShowLabel  { get; set; } = true;
    public string BarColor   { get; set; } = Theme.PrimaryColor;
    public string TrackColor { get; set; } = Theme.BorderColor;
    public bool   Striped    { get; set; } = false;

    public ProgressBar(int x, int y, int width, int height)
    { X = x; Y = y; Width = width; Height = height; }

    public override void OnPaint(Canvas canvas)
    {
        int r = Height / 2;
        canvas.DrawRoundedRect(AbsoluteX, AbsoluteY, Width, Height, r, TrackColor, TrackColor, 0);

        double pct = Math.Clamp(Value, 0, 100) / 100.0;
        int fillW = (int)(Width * pct);

        if (fillW > 0)
        {
            canvas.Save();
            canvas.SetClip(AbsoluteX, AbsoluteY, fillW, Height);
            canvas.DrawRoundedRect(AbsoluteX, AbsoluteY, Width, Height, r, BarColor, BarColor, 0);

            if (Striped)
            {
                canvas.SetGlobalAlpha(0.25);
                canvas.SetStrokeStyle("#ffffff");
                canvas.SetLineWidth(8);
                for (int i = -Height; i < Width + Height; i += 16)
                {
                    canvas.BeginPath();
                    canvas.MoveTo(AbsoluteX + i, AbsoluteY + Height);
                    canvas.LineTo(AbsoluteX + i + Height, AbsoluteY);
                    canvas.Stroke();
                }
                canvas.SetGlobalAlpha(1.0);
            }
            canvas.Restore();
        }

        if (ShowLabel)
        {
            canvas.SetFont(Theme.Font(Theme.FontSizeSm, true));
            canvas.SetTextBaseline("middle");
            string text = $"{Math.Round(Value)}%";
            double tw = canvas.MeasureText(text);
            canvas.SetFillStyle(pct >= 0.5 ? Theme.TextOnPrimary : Theme.TextColor);
            canvas.FillText(text, AbsoluteX + (int)((Width - tw) / 2), AbsoluteY + Height / 2);
        }
    }
}
