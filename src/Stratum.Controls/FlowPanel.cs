// Stratum.Controls/FlowPanel.cs
using Stratum.Core;

namespace Stratum.Controls;

public class FlowPanel : Panel
{
    public int Gap { get; set; } = 8;

    public FlowPanel(int x, int y, int width, int height)
        : base(x, y, width, height) { }

    public override void OnPaint(Canvas canvas)
    {
        int cx = 0, cy = 0, rowH = 0;
        foreach (var child in Children)
        {
            if (cx + child.Width > Width && cx > 0) { cx = 0; cy += rowH + Gap; rowH = 0; }
            child.X = cx;
            child.Y = cy;
            cx += child.Width + Gap;
            if (child.Height > rowH) rowH = child.Height;
        }
        base.OnPaint(canvas);
    }
}
