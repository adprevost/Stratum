// Stratum.Controls/ToggleSwitch.cs
using Stratum.Core;

namespace Stratum.Controls;

/// <summary>
/// A modern pill-shaped on/off switch. Animates the knob using a simple tween
/// driven by the next paint frames; raises <see cref="CheckedChanged"/> on click.
/// </summary>
public class ToggleSwitch : Control
{
    public const int TrackWidth  = 34;
    public const int TrackHeight = 20;
    public const int KnobSize    = 16;

    public string OnColor    { get; set; } = "#7c5cff";
    public string OffColor   { get; set; } = "#3f3f46";
    public string KnobColor  { get; set; } = "#ffffff";
    public string BorderColor { get; set; } = "rgba(255,255,255,0.25)";

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            StartAnimation();
            CheckedChanged?.Invoke(_checked);
        }
    }
    public event Action<bool>? CheckedChanged;
    public event Action? Click;

    private bool   _checked;
    private double _knobT;          // 0 = off, 1 = on
    private long   _animStart;
    private bool   _animating;
    private const int AnimDurationMs = 180;

    public ToggleSwitch(int x, int y)
    {
        X = x; Y = y;
        Width  = TrackWidth;
        Height = TrackHeight;
    }

    public ToggleSwitch(int x, int y, bool initialChecked) : this(x, y)
    {
        _checked = initialChecked;
        _knobT   = initialChecked ? 1 : 0;
    }

    private void StartAnimation()
    {
        _animating = true;
        _animStart = Environment.TickCount64;
        Invalidate();
    }

    public override void OnClick(MouseEventArgs e)
    {
        Checked = !_checked;
        SoundService.Play(Theme.Sounds.Toggle);
        Click?.Invoke();
    }

    public override void OnPaint(Canvas canvas)
    {
        if (_animating)
        {
            double progress = Math.Clamp((Environment.TickCount64 - _animStart) / (double)AnimDurationMs, 0, 1);
            // Ease-out cubic
            double eased = 1 - Math.Pow(1 - progress, 3);
            double target = _checked ? 1.0 : 0.0;
            double from   = _checked ? 0.0 : 1.0;
            _knobT = from + (target - from) * eased;
            if (progress >= 1) { _animating = false; _knobT = target; }
            else Invalidate();
        }

        int ax = AbsoluteX, ay = AbsoluteY;

        // Track
        string fill = _checked ? OnColor : OffColor;
        canvas.SetGlobalAlpha(_checked ? 0.95 : 0.6);
        canvas.DrawRoundedRect(ax, ay, TrackWidth, TrackHeight, TrackHeight / 2, fill, BorderColor, 1);
        canvas.SetGlobalAlpha(1.0);

        // Knob
        int knobX = ax + 2 + (int)Math.Round(_knobT * (TrackWidth - KnobSize - 4));
        int knobY = ay + (TrackHeight - KnobSize) / 2;

        // Knob shadow
        canvas.SetGlobalAlpha(0.25);
        canvas.SetFillStyle("#000000");
        canvas.BeginPath();
        canvas.Arc(knobX + KnobSize / 2, knobY + KnobSize / 2 + 1, KnobSize / 2, 0, Math.PI * 2);
        canvas.Fill();
        canvas.SetGlobalAlpha(1.0);

        // Knob fill
        canvas.SetFillStyle(KnobColor);
        canvas.BeginPath();
        canvas.Arc(knobX + KnobSize / 2, knobY + KnobSize / 2, KnobSize / 2, 0, Math.PI * 2);
        canvas.Fill();
    }
}
