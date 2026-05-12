// Stratum.Controls/Label.cs
using Stratum.Core;

namespace Stratum.Controls;

public enum TextAlign { Left, Center, Right }

public class Label : Control
{
    public string    Text     { get; set; }
    public string    Color    { get; set; } = Theme.TextColor;
    public int       FontSize { get; set; } = Theme.FontSizeBase;
    public bool      Bold     { get; set; } = false;
    public bool      Italic   { get; set; } = false;
    public TextAlign Align    { get; set; } = TextAlign.Left;

    public Label(string text, int x, int y, int width = 200, int height = 24)
    {
        Text = text; X = x; Y = y; Width = width; Height = height;
    }

    public override void OnPaint(Canvas canvas)
    {
        canvas.SetFont(Theme.Font(FontSize, Bold, Italic));
        canvas.SetFillStyle(Color);
        canvas.SetTextBaseline("middle");
        int tx = Align switch
        {
            TextAlign.Center => AbsoluteX + Width / 2 - (int)(canvas.MeasureText(Text) / 2),
            TextAlign.Right  => AbsoluteX + Width     - (int)canvas.MeasureText(Text),
            _                => AbsoluteX
        };
        canvas.FillText(Text, tx, AbsoluteY + Height / 2);
    }
}
