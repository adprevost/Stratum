// Stratum.Core/Control.cs
namespace Stratum.Core;

public abstract class Control
{
    // Layout
    public int X      { get; set; }
    public int Y      { get; set; }
    public int Width  { get; set; }
    public int Height { get; set; }

    // State
    public bool   Visible { get; set; } = true;
    public bool   Enabled { get; set; } = true;
    public bool   Focused { get; private set; }
    public bool   Hovered { get; internal set; }
    public double Opacity { get; set; } = 1.0;

    // Metadata
    public string? Name { get; set; }
    public object? Tag  { get; set; }

    // Hierarchy
    public Control?      Parent   { get; internal set; }
    public List<Control> Children { get; } = new();

    // Absolute position relative to root canvas
    public int AbsoluteX => (Parent?.AbsoluteX ?? 0) + X;
    public int AbsoluteY => (Parent?.AbsoluteY ?? 0) + Y;

    // Child management
    public void Add(Control child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void Remove(Control child)
    {
        Children.Remove(child);
        child.Parent = null;
    }

    internal void SetParent(Control? parent) => Parent = parent;

    // Override in subclasses
    public virtual void OnPaint(Canvas canvas) { }
    public virtual void OnMouseDown(MouseEventArgs e) { }
    public virtual void OnMouseUp(MouseEventArgs e) { }
    public virtual void OnMouseMove(MouseEventArgs e) { }
    public virtual void OnClick(MouseEventArgs e) { }
    public virtual void OnKeyDown(KeyEventArgs e) { }
    public virtual void OnKeyPress(string key) { }
    public virtual void OnFocus() { Focused = true; }
    public virtual void OnBlur()  { Focused = false; }
    public virtual void OnResize(int canvasWidth, int canvasHeight) { }

    // Hit testing
    public virtual Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;
        int ax = AbsoluteX, ay = AbsoluteY;
        if (x < ax || x > ax + Width || y < ay || y > ay + Height) return null;
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var hit = Children[i].HitTest(x, y);
            if (hit != null) return hit;
        }
        return this;
    }

    protected internal void Invalidate() => Application.Current?.RequestRedraw();
}
