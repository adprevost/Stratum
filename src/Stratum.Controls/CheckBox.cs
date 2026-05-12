// Stratum.Controls/CheckBox.cs
using Stratum.Core;

namespace Stratum.Controls;

public class CheckBox : Control
{
    public string Text    { get; set; }
    public bool   Checked { get; set; }
    public string Color   { get; set; } = Theme.TextColor;

    public event Action<bool>? CheckedChanged;

    private const int BoxSize = 18;

    public CheckBox(string text, int x, int y, int width = 200, int height = 28)
    { Text = text; X = x; Y = y; Width = width; Height = height; }

    public override void OnPaint(Canvas canvas)
    {
        int by = AbsoluteY + (Height - BoxSize) / 2;

        string fill = Checked ? Theme.PrimaryColor : Theme.SurfaceColor;
        string border = Focused ? Theme.FocusRing : (Checked ? Theme.PrimaryColor : Theme.BorderColor);
        canvas.DrawRoundedRect(AbsoluteX, by, BoxSize, BoxSize, Theme.BorderRadiusSm, fill, border);

        if (Checked)
        {
            canvas.SetStrokeStyle(Theme.TextOnPrimary);
            canvas.SetLineWidth(2);
            canvas.BeginPath();
            canvas.MoveTo(AbsoluteX + 4, by + 9);
            canvas.LineTo(AbsoluteX + 8, by + 13);
            canvas.LineTo(AbsoluteX + 14, by + 5);
            canvas.Stroke();
        }

        canvas.SetFont(Theme.Font());
        canvas.SetFillStyle(Color);
        canvas.SetTextBaseline("middle");
        canvas.FillText(Text, AbsoluteX + BoxSize + 8, AbsoluteY + Height / 2);
    }

    public override void OnClick(MouseEventArgs e)
    {
        Checked = !Checked;
        SoundService.Play(Theme.Sounds.Toggle);
        CheckedChanged?.Invoke(Checked);
        Invalidate();
    }
}
