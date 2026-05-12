// Stratum.Core/Scheduler.cs
using System.Threading.Tasks;

namespace Stratum.Core;

/// <summary>Time-based callback helpers. Each callback automatically requests a redraw.</summary>
public static class Scheduler
{
    public static async void After(int delayMs, Action action)
    {
        await Task.Delay(delayMs);
        action();
        Application.Current?.RequestRedraw();
    }

    public static async void Tween(int durationMs, Action<double> step, Action? complete = null)
    {
        const int frame = 16;
        var start = Environment.TickCount64;
        while (true)
        {
            var elapsed = Environment.TickCount64 - start;
            double t = Math.Clamp(elapsed / (double)durationMs, 0, 1);
            step(t);
            Application.Current?.RequestRedraw();
            if (t >= 1) break;
            await Task.Delay(frame);
        }
        complete?.Invoke();
        Application.Current?.RequestRedraw();
    }
}
