// Stratum.Controls/SidebarNav.cs
using Stratum.Core;

namespace Stratum.Controls;

public enum NavEntryKind { Group, Item }

public class SidebarNavEntry
{
    public NavEntryKind Kind  { get; set; }
    public string       Label { get; set; } = "";
}

public class SidebarNav : Control
{
    public List<SidebarNavEntry> Items       { get; set; } = new();
    public string                ActiveItem  { get; set; } = "";
    public string                Background  { get; set; } = Theme.SurfaceColor;
    public string                AccentColor { get; set; } = Theme.PrimaryColor;

    public event Action<string>? NavigationChanged;

    private string? _hoveredItem;

    public SidebarNav(int x, int y, int width, int height)
    { X = x; Y = y; Width = width; Height = height; }

    public override void OnPaint(Canvas canvas)
    {
        canvas.SetFillStyle(Background);
        canvas.FillRect(AbsoluteX, AbsoluteY, Width, Height);

        canvas.SetStrokeStyle(Theme.BorderColor);
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(AbsoluteX + Width - 1, AbsoluteY);
        canvas.LineTo(AbsoluteX + Width - 1, AbsoluteY + Height);
        canvas.Stroke();

        // App title area
        canvas.SetFont(Theme.Font(Theme.FontSizeXl, true));
        canvas.SetFillStyle(Theme.TextColor);
        canvas.SetTextBaseline("middle");
        canvas.FillText("Stratum", AbsoluteX + 20, AbsoluteY + 36);

        int y = AbsoluteY + 80;
        int idx = 0;
        foreach (var e in Items)
        {
            if (e.Kind == NavEntryKind.Group)
            {
                y += 8;
                canvas.SetFont(Theme.Font(Theme.FontSizeSm, true));
                canvas.SetFillStyle(Theme.TextMuted);
                canvas.SetTextBaseline("middle");
                canvas.FillText(e.Label.ToUpperInvariant(), AbsoluteX + 20, y + 8);
                y += 26;
            }
            else
            {
                int rowH = 38;
                bool active = e.Label == ActiveItem;
                bool hover = e.Label == _hoveredItem;

                if (active)
                {
                    canvas.SetFillStyle("#dbeafe");
                    canvas.FillRect(AbsoluteX, y, Width, rowH);
                    canvas.SetFillStyle(AccentColor);
                    canvas.FillRect(AbsoluteX, y, 4, rowH);
                }
                else if (hover)
                {
                    canvas.SetFillStyle(Theme.BackgroundColor);
                    canvas.FillRect(AbsoluteX, y, Width, rowH);
                }

                DrawIcon(canvas, AbsoluteX + 24, y + rowH / 2, idx, active);

                canvas.SetFont(Theme.Font(Theme.FontSizeBase, active));
                canvas.SetFillStyle(active ? AccentColor : Theme.TextColor);
                canvas.SetTextBaseline("middle");
                canvas.FillText(e.Label, AbsoluteX + 50, y + rowH / 2);

                y += rowH;
                idx++;
            }
        }
    }

    private void DrawIcon(Canvas c, int cx, int cy, int idx, bool active)
    {
        string color = active ? AccentColor : Theme.TextMuted;
        c.SetStrokeStyle(color);
        c.SetFillStyle(color);
        c.SetLineWidth(2);
        int r = 7;
        switch (idx % 3)
        {
            case 0:
                c.BeginPath(); c.Arc(cx, cy, r, 0, Math.PI * 2); c.Stroke();
                break;
            case 1:
                c.SetLineWidth(2);
                c.BeginPath();
                c.MoveTo(cx - r, cy - r);
                c.LineTo(cx + r, cy - r);
                c.LineTo(cx + r, cy + r);
                c.LineTo(cx - r, cy + r);
                c.ClosePath();
                c.Stroke();
                break;
            case 2:
                c.BeginPath();
                c.MoveTo(cx, cy - r);
                c.LineTo(cx + r, cy + r);
                c.LineTo(cx - r, cy + r);
                c.ClosePath();
                c.Stroke();
                break;
        }
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        var item = ItemAt(e.X, e.Y);
        if (item != _hoveredItem) { _hoveredItem = item; Invalidate(); }
    }

    public override void OnClick(MouseEventArgs e)
    {
        var item = ItemAt(e.X, e.Y);
        if (item != null && item != ActiveItem)
        {
            ActiveItem = item;
            NavigationChanged?.Invoke(item);
            Invalidate();
        }
    }

    private string? ItemAt(int mx, int my)
    {
        if (mx < AbsoluteX || mx > AbsoluteX + Width) return null;
        int y = AbsoluteY + 80;
        foreach (var e in Items)
        {
            if (e.Kind == NavEntryKind.Group)
            {
                y += 8 + 26;
            }
            else
            {
                int rowH = 38;
                if (my >= y && my < y + rowH) return e.Label;
                y += rowH;
            }
        }
        return null;
    }
}
