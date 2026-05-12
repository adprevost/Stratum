// Stratum.Controls/Panel.cs
using Stratum.Core;

namespace Stratum.Controls;

public class Panel : Control
{
    public bool   DrawBorder  { get; set; } = false;
    public string Background  { get; set; } = "transparent";
    public string BorderColor { get; set; } = Theme.BorderColor;
    public int    BorderRadius { get; set; } = 0;

    public Panel(int x, int y, int width, int height)
    { X = x; Y = y; Width = width; Height = height; }

    public override void OnPaint(Canvas canvas)
    {
        if (Background != "transparent")
        {
            if (BorderRadius > 0)
                canvas.DrawRoundedRect(AbsoluteX, AbsoluteY, Width, Height, BorderRadius, Background, Background, 0);
            else
            {
                canvas.SetFillStyle(Background);
                canvas.FillRect(AbsoluteX, AbsoluteY, Width, Height);
            }
        }
        if (DrawBorder)
        {
            canvas.SetStrokeStyle(BorderColor);
            canvas.SetLineWidth(1);
            if (BorderRadius > 0)
            {
                canvas.BeginPath();
                canvas.RoundRect(AbsoluteX, AbsoluteY, Width, Height, BorderRadius);
                canvas.Stroke();
            }
            else
                canvas.StrokeRect(AbsoluteX, AbsoluteY, Width, Height);
        }
    }
}
