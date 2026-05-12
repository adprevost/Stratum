// Stratum.Controls/Button.cs
using Stratum.Core;

namespace Stratum.Controls;

public enum ButtonStyle { Primary, Secondary, Ghost, Danger }

public class Button : Control
{
    public string      Text    { get; set; }
    public ButtonStyle Style   { get; set; } = ButtonStyle.Primary;
    // Legacy convenience
    public bool Primary
    {
        get => Style == ButtonStyle.Primary;
        set => Style = value ? ButtonStyle.Primary : ButtonStyle.Secondary;
    }

    public event Action? Click;

    private bool _pressed = false;

    public Button(string text, int x, int y, int width = 120, int height = 36)
    {
        Text = text; X = x; Y = y; Width = width; Height = height;
    }

    public override void OnPaint(Canvas canvas)
    {
        string bg = Style switch
        {
            ButtonStyle.Primary   => _pressed || Hovered ? Theme.PrimaryHover   : Theme.PrimaryColor,
            ButtonStyle.Secondary => _pressed || Hovered ? Theme.SecondaryHover : Theme.SecondaryColor,
            ButtonStyle.Ghost     => _pressed || Hovered ? Theme.BorderColor    : "transparent",
            ButtonStyle.Danger    => _pressed || Hovered ? "#b91c1c"            : Theme.ErrorColor,
            _                     => Theme.PrimaryColor
        };

        string border = Focused ? Theme.FocusRing : bg;

        canvas.DrawRoundedRect(AbsoluteX, AbsoluteY, Width, Height,
            Theme.BorderRadius, bg, border, Focused ? 2 : 1);

        canvas.SetFont(Theme.Font(Theme.FontSizeBase, true));
        canvas.SetFillStyle(Theme.TextOnPrimary);
        canvas.SetTextBaseline("middle");
        double tw = canvas.MeasureText(Text);
        canvas.FillText(Text, AbsoluteX + (int)((Width - tw) / 2), AbsoluteY + Height / 2);
    }

    public override void OnMouseDown(MouseEventArgs e) { _pressed = true;  Invalidate(); }
    public override void OnMouseUp(MouseEventArgs e)   { _pressed = false; Invalidate(); }
    public override void OnClick(MouseEventArgs e)     { SoundService.Play(Theme.Sounds.Click); Click?.Invoke(); }
}
