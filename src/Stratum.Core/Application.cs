// Stratum.Core/Application.cs
namespace Stratum.Core;

public abstract class Application
{
    public static Application? Current { get; private set; }

    private readonly List<Control>       _roots  = new();
    private readonly List<ModalOverlay>  _modals = new();
    private ToastHostBase?               _toastHost;
    private readonly Canvas              _canvas = new();
    private readonly Action              _frameDelegate;
    private Control?                     _focused;
    private Control?                     _captured;
    private bool                         _dirty = true;

    protected int CanvasWidth  => _canvas.Width;
    protected int CanvasHeight => _canvas.Height;
    public    int ScreenWidth  => _canvas.Width;
    public    int ScreenHeight => _canvas.Height;

    public ToastHostBase? ToastHost => _toastHost;
    public void RegisterToastHost(ToastHostBase host) => _toastHost = host;

    public void RegisterModal(ModalOverlay modal)
    {
        if (!_modals.Contains(modal)) _modals.Add(modal);
    }

    protected Application()
    {
        _frameDelegate = Frame;
    }

    public void Run()
    {
        Current = this;
        OnStart();
        ScheduleFrame();
    }

    protected virtual void OnStart() { }

    public    void Add(Control c)    => _roots.Add(c);
    public    void Remove(Control c) => _roots.Remove(c);

    public void RequestRedraw() => _dirty = true;

    private void ScheduleFrame() => JsCanvas.RequestFrame(_frameDelegate);

    private void Frame()
    {
        if (_toastHost != null && _toastHost.HasActive) _dirty = true;
        if (_dirty)
        {
            Render();
            _dirty = false;
        }
        ScheduleFrame();
    }

    private void Render()
    {
        _canvas.SetFillStyle(Theme.BackgroundColor);
        _canvas.FillRect(0, 0, _canvas.Width, _canvas.Height);
        foreach (var c in _roots)
            RenderTree(c, _canvas);
        foreach (var m in _modals)
            if (m.Visible) RenderTree(m, _canvas);
        if (_toastHost != null && _toastHost.HasActive)
            RenderTree(_toastHost, _canvas);
    }

    private static void RenderTree(Control c, Canvas canvas)
    {
        if (!c.Visible) return;
        canvas.Save();
        c.OnPaint(canvas);
        // Snapshot children before recursing: animation callbacks or event
        // handlers invoked during OnPaint can mutate the list (e.g. BringToFront,
        // window close) which would throw InvalidOperation_EnumFailedVersion.
        var children = c.Children.Count == 0
            ? (IReadOnlyList<Control>)Array.Empty<Control>()
            : c.Children.ToArray();
        foreach (var child in children)
            RenderTree(child, canvas);
        canvas.Restore();
    }

    private ModalOverlay? ActiveModal()
    {
        for (int i = _modals.Count - 1; i >= 0; i--)
            if (_modals[i].Visible) return _modals[i];
        return null;
    }

    internal void DispatchMouseMove(int x, int y)
    {
        var e = new MouseEventArgs { X = x, Y = y };
        var hit = HitTest(x, y);

        foreach (var root in _roots) ClearHover(root);
        if (hit != null) { hit.Hovered = true; RequestRedraw(); }

        (_captured ?? hit)?.OnMouseMove(e);
    }

    internal void DispatchMouseDown(int x, int y, int btn)
    {
        var e = new MouseEventArgs { X = x, Y = y, Button = btn };
        var hit = HitTest(x, y);
        _captured = hit;

        if (hit != _focused)
        {
            _focused?.OnBlur();
            _focused = hit;
            _focused?.OnFocus();
        }

        hit?.OnMouseDown(e);
        RequestRedraw();
    }

    internal void DispatchMouseUp(int x, int y, int btn)
    {
        var e = new MouseEventArgs { X = x, Y = y, Button = btn };
        var hit = HitTest(x, y);
        _captured?.OnMouseUp(e);
        if (_captured == hit) hit?.OnClick(e);
        _captured = null;
        RequestRedraw();
    }

    internal void DispatchKeyDown(string key, string code, bool ctrl, bool shift, bool alt)
    {
        var e = new KeyEventArgs { Key = key, Code = code, Ctrl = ctrl, Shift = shift, Alt = alt };
        var modal = ActiveModal();
        if (modal != null) modal.OnKeyDown(e);
        else _focused?.OnKeyDown(e);
        RequestRedraw();
    }

    internal void DispatchKeyPress(string key)
    {
        if (ActiveModal() != null) return;
        _focused?.OnKeyPress(key);
        RequestRedraw();
    }

    internal void DispatchResize(int w, int h)
    {
        foreach (var root in _roots) root.OnResize(w, h);
        RequestRedraw();
    }

    private Control? HitTest(int x, int y)
    {
        if (_toastHost != null && _toastHost.HasActive)
        {
            var th = _toastHost.HitTest(x, y);
            if (th != null) return th;
        }
        var modal = ActiveModal();
        if (modal != null) return modal;
        for (int i = _roots.Count - 1; i >= 0; i--)
        {
            var hit = _roots[i].HitTest(x, y);
            if (hit != null) return hit;
        }
        return null;
    }

    private static void ClearHover(Control c)
    {
        c.Hovered = false;
        foreach (var child in c.Children) ClearHover(child);
    }
}
