// Stratum.Controls/Toast.cs
using Stratum.Core;

namespace Stratum.Controls;

public enum ToastKind { Info, Success, Warning, Error }

public static class Toast
{
    public static void Show(string message, ToastKind kind = ToastKind.Info, int durationMs = 4000)
    {
        var app = Application.Current;
        if (app == null) return;
        if (app.ToastHost is ToastManager tm) tm.Show(message, kind, durationMs);
    }
}

internal class ToastInstance
{
    public string Message = "";
    public ToastKind Kind;
    public int DurationMs;
    public long StartTick;
    public bool Dismissing;
    public long DismissStartTick;
    public int Y;        // current rendered Y (animated)
    public int TargetY;  // target Y in stack
    public int Height;
}

public class ToastManager : ToastHostBase
{
    private readonly List<ToastInstance> _toasts = new();
    private const int Width0    = 320;
    private const int MarginEdge = 16;
    private const int Gap        = 8;

    public ToastManager()
    {
        // Spans canvas; not interactive other than close button
        X = 0; Y = 0; Width = 0; Height = 0;
        Visible = true;
    }

    public override bool HasActive => _toasts.Count > 0;

    public void Show(string message, ToastKind kind, int durationMs)
    {
        var t = new ToastInstance
        {
            Message = message,
            Kind = kind,
            DurationMs = durationMs,
            StartTick = Environment.TickCount64
        };
        _toasts.Add(t);
        string soundId = kind switch
        {
            ToastKind.Success => Theme.Sounds.ToastSuccess,
            ToastKind.Warning => Theme.Sounds.ToastWarning,
            ToastKind.Error   => Theme.Sounds.ToastError,
            _                 => Theme.Sounds.ToastInfo,
        };
        SoundService.Play(soundId);
        Invalidate();
    }

    public override Control? HitTest(int x, int y)
    {
        // Only hit the close-button regions of visible toasts
        foreach (var t in _toasts)
        {
            int tx = (Application.Current?.ScreenWidth ?? 1280) - Width0 - MarginEdge;
            int ty = t.Y;
            int closeX = tx + Width0 - 24;
            int closeY = ty + 8;
            if (x >= closeX && x <= closeX + 16 && y >= closeY && y <= closeY + 16)
                return this;
        }
        return null;
    }

    public override void OnClick(MouseEventArgs e)
    {
        for (int i = 0; i < _toasts.Count; i++)
        {
            var t = _toasts[i];
            int tx = (Application.Current?.ScreenWidth ?? 1280) - Width0 - MarginEdge;
            int ty = t.Y;
            int closeX = tx + Width0 - 24;
            int closeY = ty + 8;
            if (e.X >= closeX && e.X <= closeX + 16 && e.Y >= closeY && e.Y <= closeY + 16)
            {
                if (!t.Dismissing) { t.Dismissing = true; t.DismissStartTick = Environment.TickCount64; }
                Invalidate();
                return;
            }
        }
    }

    public override void OnPaint(Canvas canvas)
    {
        if (_toasts.Count == 0) return;
        long now = Environment.TickCount64;

        // Tick: trigger dismissal for expired toasts and remove fully-faded ones
        for (int i = _toasts.Count - 1; i >= 0; i--)
        {
            var t = _toasts[i];
            if (!t.Dismissing && now - t.StartTick >= t.DurationMs)
            { t.Dismissing = true; t.DismissStartTick = now; }
            if (t.Dismissing && now - t.DismissStartTick >= 180)
                _toasts.RemoveAt(i);
        }
        if (_toasts.Count == 0) return;

        int cw = canvas.Width;
        int x = cw - Width0 - MarginEdge;
        int curY = MarginEdge;

        foreach (var t in _toasts)
        {
            // Compute height from message wrap
            canvas.SetFont(Theme.Font());
            var lines = WrapLines(canvas, t.Message, Width0 - 60);
            t.Height = Math.Max(56, 16 + lines.Count * 20);

            // Animate slide in
            long age = now - t.StartTick;
            double slideT = Math.Clamp(age / 180.0, 0, 1);
            double slideEase = 1 - Math.Pow(1 - slideT, 3);
            int xOff = (int)((1 - slideEase) * (Width0 + MarginEdge));

            // Animate fade out
            double alpha = 1.0;
            if (t.Dismissing)
            {
                long da = now - t.DismissStartTick;
                alpha = 1.0 - Math.Clamp(da / 180.0, 0, 1);
            }

            t.TargetY = curY;
            t.Y = curY;

            int drawX = x + xOff;
            int drawY = curY;

            canvas.Save();
            canvas.SetGlobalAlpha(alpha);

            // Card
            canvas.DrawRoundedRect(drawX, drawY, Width0, t.Height,
                Theme.BorderRadius, Theme.SurfaceColor, Theme.BorderColor, 1);

            // Accent bar
            string accent = t.Kind switch
            {
                ToastKind.Success => Theme.SuccessColor,
                ToastKind.Warning => "#f59e0b",
                ToastKind.Error   => Theme.ErrorColor,
                _                 => Theme.PrimaryColor
            };
            canvas.SetFillStyle(accent);
            canvas.FillRect(drawX, drawY, 4, t.Height);

            // Text
            canvas.SetFont(Theme.Font());
            canvas.SetFillStyle(Theme.TextColor);
            canvas.SetTextBaseline("middle");
            int ty = drawY + 18;
            foreach (var line in lines)
            {
                canvas.FillText(line, drawX + 16, ty);
                ty += 20;
            }

            // Close button
            canvas.SetFont(Theme.Font(Theme.FontSizeBase, true));
            canvas.SetFillStyle(Theme.TextMuted);
            canvas.FillText("×", drawX + Width0 - 20, drawY + 14);

            canvas.SetGlobalAlpha(1.0);
            canvas.Restore();

            curY += t.Height + Gap;
        }

        // Schedule another redraw to keep animations smooth
        Invalidate();
    }

    private static List<string> WrapLines(Canvas canvas, string text, int maxWidth)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(text)) return result;
        var words = text.Split(' ');
        string current = "";
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
