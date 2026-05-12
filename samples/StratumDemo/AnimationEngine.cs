// StratumDemo/AnimationEngine.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>
/// A minimal property tween engine. Animations are stored in a static list and
/// stepped each frame via <see cref="Tick"/>. Each tick requests a redraw while
/// animations remain active so the application keeps painting smoothly.
/// </summary>
public static class AnimationEngine
{
    private static readonly List<TweenAnimation> _active = new();

    public static bool HasActive => _active.Count > 0;

    public static TweenAnimation Tween(double durationMs, Func<double, double> easing, Action<double> onUpdate, Action? onComplete = null)
    {
        ArgumentNullException.ThrowIfNull(easing);
        ArgumentNullException.ThrowIfNull(onUpdate);

        TweenAnimation a = new()
        {
            DurationMs = durationMs,
            Easing     = easing,
            OnUpdate   = onUpdate,
            OnComplete = onComplete,
            StartTick  = Environment.TickCount64
        };
        _active.Add(a);
        Application.Current?.RequestRedraw();
        return a;
    }

    /// <summary>Convenience: tween a numeric property from start to end over duration.</summary>
    public static TweenAnimation To(Action<double> setter, double start, double end, double durationMs, Func<double, double>? easing = null, Action? onComplete = null)
    {
        easing ??= Ease.OutCubic;
        return Tween(durationMs, easing, t => setter(start + (end - start) * t), onComplete);
    }

    public static void Cancel(TweenAnimation? a)
    {
        if (a is null) return;
        _active.Remove(a);
    }

    public static void Tick()
    {
        if (_active.Count == 0) return;

        // Collect completed callbacks and fire them AFTER the update loop.
        // Firing inline can call BringToFront / Children.Remove+Add from inside
        // Application.RenderTree's foreach(c.Children) and trigger
        // InvalidOperation_EnumFailedVersion.
        List<Action>? completions = null;

        long now = Environment.TickCount64;
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            TweenAnimation a = _active[i];
            double progress = a.DurationMs <= 0 ? 1.0 : Math.Clamp((now - a.StartTick) / a.DurationMs, 0.0, 1.0);
            double eased    = a.Easing(progress);
            a.OnUpdate(eased);

            if (progress >= 1.0)
            {
                _active.RemoveAt(i);
                if (a.OnComplete != null)
                {
                    completions ??= new List<Action>();
                    completions.Add(a.OnComplete);
                }
            }
        }

        Application.Current?.RequestRedraw();

        // Fire completions now that we are outside the render-tree iteration.
        if (completions != null)
            foreach (Action cb in completions) cb();
    }
}

public sealed class TweenAnimation
{
    public required double DurationMs { get; init; }
    public required Func<double, double> Easing { get; init; }
    public required Action<double> OnUpdate { get; init; }
    public Action? OnComplete { get; init; }
    public long StartTick { get; init; }
}

/// <summary>Easing functions used to give the desktop a polished feel.</summary>
public static class Ease
{
    public static double Linear(double t) => t;

    public static double OutCubic(double t) => 1 - Math.Pow(1 - t, 3);

    public static double InOutQuad(double t)
        => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

    public static double OutBack(double t)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }
}
