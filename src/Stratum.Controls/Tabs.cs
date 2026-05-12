// Stratum.Controls/Tabs.cs
using Stratum.Core;

namespace Stratum.Controls;

public class Tabs : Control
{
    public List<string> TabList   { get; set; } = new();
    public string       ActiveTab { get; set; } = "";
    public int          ActiveIndex { get; set; } = 0;

    public event Action<string, int>? TabChanged;

    public Tabs(int x, int y, int width, int height)
    { X = x; Y = y; Width = width; Height = height; }

    public void EnsureActive()
    {
        if (TabList.Count == 0) return;
        if (ActiveIndex < 0 || ActiveIndex >= TabList.Count) ActiveIndex = 0;
        ActiveTab = TabList[ActiveIndex];
    }

    public override void OnPaint(Canvas canvas)
    {
        if (TabList.Count == 0) return;
        EnsureActive();
        int tabW = Width / TabList.Count;
        int rowH = Height - 4;

        for (int i = 0; i < TabList.Count; i++)
        {
            int x = AbsoluteX + i * tabW;
            bool active = i == ActiveIndex;
            string bg     = active ? Theme.PrimaryColor : Theme.SurfaceColor;
            string fg     = active ? Theme.TextOnPrimary : Theme.TextColor;
            string border = active ? Theme.PrimaryColor : Theme.BorderColor;

            canvas.DrawRoundedRect(x + 2, AbsoluteY + 2, tabW - 4, rowH - 4,
                Theme.BorderRadius, bg, border, 1);

            canvas.SetFont(Theme.Font(Theme.FontSizeBase, active));
            canvas.SetFillStyle(fg);
            canvas.SetTextBaseline("middle");
            double tw = canvas.MeasureText(TabList[i]);
            canvas.FillText(TabList[i], x + (int)((tabW - tw) / 2), AbsoluteY + rowH / 2);
        }

        canvas.SetStrokeStyle(Theme.BorderColor);
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(AbsoluteX, AbsoluteY + Height - 1);
        canvas.LineTo(AbsoluteX + Width, AbsoluteY + Height - 1);
        canvas.Stroke();
    }

    public override void OnClick(MouseEventArgs e)
    {
        if (TabList.Count == 0) return;
        int tabW = Width / TabList.Count;
        int idx = (e.X - AbsoluteX) / tabW;
        if (idx >= 0 && idx < TabList.Count && idx != ActiveIndex)
        {
            ActiveIndex = idx;
            ActiveTab = TabList[idx];
            TabChanged?.Invoke(ActiveTab, idx);
            Invalidate();
        }
    }
}
