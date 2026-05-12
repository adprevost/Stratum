// Stratum.Controls/Modal.cs
using Stratum.Core;

namespace Stratum.Controls;

public class Modal : ModalOverlay
{
    public string Title          { get; set; } = "";
    public string Message        { get; set; } = "";
    public double OverlayOpacity { get; set; } = 0.5;

    public override bool IsActive => Visible;

    public event Action? Confirmed;
    public event Action? Cancelled;

    private readonly List<(string label, Action handler, bool affirmative)> _buttons = new();
    private readonly List<(int x, int y, int w, int h, Action handler)>     _btnHits = new();

    public Modal()
    {
        Width = 480;
        Height = 0;       // calculated at paint time
        Visible = false;
    }

    public Modal OkOnly()
    {
        _buttons.Clear();
        _buttons.Add(("OK", () => { Visible = false; Confirmed?.Invoke(); }, true));
        return this;
    }

    public Modal OkCancel()
    {
        _buttons.Clear();
        _buttons.Add(("Cancel", () => { Visible = false; Cancelled?.Invoke(); }, false));
        _buttons.Add(("OK",     () => { Visible = false; Confirmed?.Invoke(); }, true));
        return this;
    }

    public Modal YesNo()
    {
        _buttons.Clear();
        _buttons.Add(("No",  () => { Visible = false; Cancelled?.Invoke(); }, false));
        _buttons.Add(("Yes", () => { Visible = false; Confirmed?.Invoke(); }, true));
        return this;
    }

    public Modal Custom(string label, Action handler)
    {
        _buttons.Add((label, () => { Visible = false; handler?.Invoke(); }, false));
        return this;
    }

    public Modal ClearButtons() { _buttons.Clear(); return this; }

    public void Dismiss()
    {
        Visible = false;
        Cancelled?.Invoke();
    }

    public override Control? HitTest(int x, int y)
    {
        if (!Visible) return null;
        return this; // consume entire canvas
    }

    public override void OnPaint(Canvas canvas)
    {
        if (!Visible) return;

        int cw = canvas.Width, ch = canvas.Height;

        canvas.Save();
        canvas.SetGlobalAlpha(OverlayOpacity);
        canvas.SetFillStyle("#000000");
        canvas.FillRect(0, 0, cw, ch);
        canvas.SetGlobalAlpha(1.0);

        int padding = 24;
        int titleH = 48;
        int btnAreaH = 64;
        int dlgW = Width;
        canvas.SetFont(Theme.Font());
        var lines = WrapLines(canvas, Message, dlgW - padding * 2);
        int bodyH = lines.Count * 22;
        int dlgH = titleH + padding + bodyH + padding + btnAreaH;
        int dlgX = (cw - dlgW) / 2;
        int dlgY = (ch - dlgH) / 2;

        // Update layout for hit testing
        X = dlgX; Y = dlgY; Height = dlgH;

        canvas.DrawRoundedRect(dlgX, dlgY, dlgW, dlgH,
            Theme.BorderRadius, Theme.SurfaceColor, Theme.BorderColor, 1);

        canvas.SetFont(Theme.Font(Theme.FontSizeLg, true));
        canvas.SetFillStyle(Theme.TextColor);
        canvas.SetTextBaseline("middle");
        canvas.FillText(Title, dlgX + padding, dlgY + titleH / 2);

        canvas.SetStrokeStyle(Theme.BorderColor);
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(dlgX, dlgY + titleH);
        canvas.LineTo(dlgX + dlgW, dlgY + titleH);
        canvas.Stroke();

        canvas.SetFont(Theme.Font());
        canvas.SetFillStyle(Theme.TextColor);
        int ty = dlgY + titleH + padding + 8;
        foreach (var line in lines)
        {
            canvas.FillText(line, dlgX + padding, ty);
            ty += 22;
        }

        // Buttons right-aligned
        int btnH = 36;
        int btnY = dlgY + dlgH - padding - btnH;
        int bx = dlgX + dlgW - padding;
        _btnHits.Clear();
        for (int i = _buttons.Count - 1; i >= 0; i--)
        {
            var b = _buttons[i];
            int bw = 96;
            int x0 = bx - bw;
            string bg = b.affirmative ? Theme.PrimaryColor : Theme.SurfaceColor;
            string fg = b.affirmative ? Theme.TextOnPrimary : Theme.TextColor;
            string br = b.affirmative ? Theme.PrimaryColor : Theme.BorderColor;
            canvas.DrawRoundedRect(x0, btnY, bw, btnH, Theme.BorderRadius, bg, br, 1);
            canvas.SetFont(Theme.Font(Theme.FontSizeBase, true));
            canvas.SetFillStyle(fg);
            canvas.SetTextBaseline("middle");
            double tw = canvas.MeasureText(b.label);
            canvas.FillText(b.label, x0 + (int)((bw - tw) / 2), btnY + btnH / 2);
            _btnHits.Add((x0, btnY, bw, btnH, b.handler));
            bx = x0 - 8;
        }

        canvas.Restore();
    }

    public override void OnClick(MouseEventArgs e)
    {
        foreach (var hit in _btnHits)
        {
            if (e.X >= hit.x && e.X <= hit.x + hit.w &&
                e.Y >= hit.y && e.Y <= hit.y + hit.h)
            {
                hit.handler?.Invoke();
                Invalidate();
                return;
            }
        }
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == "Escape") Dismiss();
    }

    private static List<string> WrapLines(Canvas canvas, string text, int maxWidth)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(text)) return result;
        var words = text.Split(' ');
        var current = "";
        foreach (var w in words)
        {
            var trial = current.Length == 0 ? w : current + " " + w;
            if (canvas.MeasureText(trial) > maxWidth && current.Length > 0)
            {
                result.Add(current);
                current = w;
            }
            else current = trial;
        }
        if (current.Length > 0) result.Add(current);
        return result;
    }
}
