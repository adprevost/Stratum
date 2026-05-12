// Stratum.Controls/DataGrid.cs
using Stratum.Core;

namespace Stratum.Controls;

public class DataGridColumn
{
    public string Header { get; set; } = "";
    public int    Width  { get; set; } = 100;
    public Func<object, string>? ValueGetter { get; set; }
}

public class DataGrid : Control
{
    public List<DataGridColumn> Columns { get; } = new();
    public IList<object>        Items   { get; set; } = new List<object>();

    private int _scrollTop = 0;
    private int _selectedIndex = -1;
    private const int RowHeight    = 32;
    private const int HeaderHeight = 36;

    public event Action<int, object?>? SelectionChanged;

    public DataGrid(int x, int y, int width, int height)
    { X = x; Y = y; Width = width; Height = height; }

    public override void OnPaint(Canvas canvas)
    {
        canvas.SetFillStyle(Theme.SurfaceColor);
        canvas.FillRect(AbsoluteX, AbsoluteY, Width, Height);

        canvas.SetFillStyle(Theme.BackgroundColor);
        canvas.FillRect(AbsoluteX, AbsoluteY, Width, HeaderHeight);
        canvas.SetFont(Theme.Font(Theme.FontSizeBase, true));
        canvas.SetFillStyle(Theme.TextColor);
        canvas.SetTextBaseline("middle");

        int cx = AbsoluteX;
        foreach (var col in Columns)
        {
            canvas.FillText(col.Header, cx + 8, AbsoluteY + HeaderHeight / 2);
            cx += col.Width;
        }

        canvas.SetStrokeStyle(Theme.BorderColor);
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(AbsoluteX, AbsoluteY + HeaderHeight);
        canvas.LineTo(AbsoluteX + Width, AbsoluteY + HeaderHeight);
        canvas.Stroke();

        canvas.Save();
        canvas.SetClip(AbsoluteX, AbsoluteY + HeaderHeight, Width, Height - HeaderHeight);

        canvas.SetFont(Theme.Font());
        int visibleRows = (Height - HeaderHeight) / RowHeight + 1;
        int startRow    = _scrollTop / RowHeight;

        for (int i = startRow; i < Math.Min(startRow + visibleRows, Items.Count); i++)
        {
            int ry = AbsoluteY + HeaderHeight + i * RowHeight - _scrollTop;

            if (i == _selectedIndex)
                canvas.SetFillStyle(Theme.SelectionColor);
            else if (i % 2 == 1)
                canvas.SetFillStyle("#f3f4f6");
            else
                canvas.SetFillStyle(Theme.SurfaceColor);

            canvas.FillRect(AbsoluteX, ry, Width, RowHeight);

            canvas.SetFillStyle(Theme.TextColor);
            canvas.SetTextBaseline("middle");

            int rx = AbsoluteX;
            foreach (var col in Columns)
            {
                string val = col.ValueGetter?.Invoke(Items[i]) ?? Items[i]?.ToString() ?? "";
                canvas.FillText(val, rx + 8, ry + RowHeight / 2);
                rx += col.Width;
            }
        }

        canvas.Restore();

        canvas.SetStrokeStyle(Theme.BorderColor);
        canvas.SetLineWidth(1);
        canvas.StrokeRect(AbsoluteX, AbsoluteY, Width, Height);
    }

    public override void OnMouseDown(MouseEventArgs e)
    {
        int relY = e.Y - AbsoluteY - HeaderHeight + _scrollTop;
        if (relY < 0) return;
        int idx = relY / RowHeight;
        if (idx >= 0 && idx < Items.Count)
        {
            _selectedIndex = idx;
            SelectionChanged?.Invoke(idx, Items[idx]);
            Invalidate();
        }
    }

    public override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown": ScrollBy(RowHeight);  break;
            case "ArrowUp":   ScrollBy(-RowHeight); break;
            case "PageDown":  ScrollBy(Height - HeaderHeight); break;
            case "PageUp":    ScrollBy(-(Height - HeaderHeight)); break;
        }
    }

    private void ScrollBy(int delta)
    {
        int maxScroll = Math.Max(0, Items.Count * RowHeight - (Height - HeaderHeight));
        _scrollTop = Math.Clamp(_scrollTop + delta, 0, maxScroll);
        Invalidate();
    }
}
