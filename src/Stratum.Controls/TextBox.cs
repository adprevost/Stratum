// Stratum.Controls/TextBox.cs
using Stratum.Core;

namespace Stratum.Controls;

public class TextBox : Control
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? "";
            // Keep cursor in bounds when text is replaced externally.
            if (_cursor > _text.Length) _cursor = _text.Length;
        }
    }
    public string Placeholder { get; set; } = "";
    public bool   Password    { get; set; } = false;
    // Alias used by .stratum files
    public bool   Masked      { get => Password; set => Password = value; }

    public event Action<string>? TextChanged;

    private int _cursor = 0;
    private int _scrollOffset = 0;

    public TextBox(int x, int y, int width = 200, int height = 36)
    { X = x; Y = y; Width = width; Height = height; }

    private string DisplayText => Password ? new string('•', Text.Length) : Text;

    public override void OnPaint(Canvas canvas)
    {
        string border = Focused ? Theme.PrimaryColor : Theme.BorderColor;
        canvas.DrawRoundedRect(AbsoluteX, AbsoluteY, Width, Height,
            Theme.BorderRadiusSm, Theme.SurfaceColor, border, Focused ? 2 : 1);

        int pad = Theme.Padding;
        int innerX = AbsoluteX + pad;
        int innerW = Width - pad * 2;

        canvas.Save();
        canvas.SetClip(AbsoluteX + 1, AbsoluteY + 1, Width - 2, Height - 2);

        canvas.SetFont(Theme.Font());
        canvas.SetTextBaseline("middle");
        int midY = AbsoluteY + Height / 2;

        string display = DisplayText;

        if (string.IsNullOrEmpty(display) && !Focused)
        {
            canvas.SetFillStyle(Theme.TextMuted);
            canvas.FillText(Placeholder, innerX, midY);
        }
        else
        {
            canvas.SetFillStyle(Theme.TextColor);
            canvas.FillText(display, innerX - _scrollOffset, midY);
        }

        // Refine scroll offset using real MeasureText
        if (Focused)
        {
            double cursorPxReal = canvas.MeasureText(display.Substring(0, Math.Clamp(_cursor, 0, display.Length)));
            if (cursorPxReal - _scrollOffset > innerW) _scrollOffset = (int)(cursorPxReal - innerW);
            else if (cursorPxReal < _scrollOffset)     _scrollOffset = (int)cursorPxReal;
            if (_scrollOffset < 0) _scrollOffset = 0;

            bool blink = (Environment.TickCount64 / 500) % 2 == 0;
            if (blink)
            {
                int cx = innerX + (int)cursorPxReal - _scrollOffset;
                canvas.SetStrokeStyle(Theme.TextColor);
                canvas.SetLineWidth(1.5);
                canvas.BeginPath();
                canvas.MoveTo(cx, AbsoluteY + 6);
                canvas.LineTo(cx, AbsoluteY + Height - 6);
                canvas.Stroke();
            }
            Invalidate(); // keep redrawing for blink
        }

        canvas.Restore();
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case "Backspace":
                if (_cursor > 0) { Text = Text.Remove(_cursor - 1, 1); _cursor--; TextChanged?.Invoke(Text); }
                break;
            case "Delete":
                if (_cursor < Text.Length) { Text = Text.Remove(_cursor, 1); TextChanged?.Invoke(Text); }
                break;
            case "ArrowLeft":
                if (_cursor > 0) _cursor--;
                break;
            case "ArrowRight":
                if (_cursor < Text.Length) _cursor++;
                break;
            case "Home":
                _cursor = 0; _scrollOffset = 0;
                break;
            case "End":
                _cursor = Text.Length;
                break;
        }
        UpdateScroll();
        Invalidate();
    }

    public override void OnKeyPress(string key)
    {
        if (key.Length == 1 && !char.IsControl(key[0]))
        {
            Text = Text.Insert(_cursor, key);
            _cursor++;
            TextChanged?.Invoke(Text);
            UpdateScroll();
        }
    }

    private void UpdateScroll()
    {
        int pad = Theme.Padding;
        int innerW = Width - pad * 2;
        int approxCharW = Math.Max(6, (int)(Theme.FontSizeBase * 0.55));
        int cursorPx = _cursor * approxCharW;
        if (cursorPx - _scrollOffset > innerW) _scrollOffset = cursorPx - innerW;
        else if (cursorPx < _scrollOffset)     _scrollOffset = cursorPx;
        if (_scrollOffset < 0) _scrollOffset = 0;
    }
}
