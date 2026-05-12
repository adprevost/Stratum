// StratumDemo/DesktopApp.cs
using Stratum.Core;

namespace StratumDemo;

/// <summary>
/// The "WebOS" desktop. Three root controls are stacked:
///   1. <see cref="DesktopBackground"/>  — wallpaper, drives the frame loop and animation tick
///   2. <see cref="WindowLayer"/>        — z-ordered, draggable application windows
///   3. <see cref="Taskbar"/>            — floating glass dock + live clock (input-on-top)
/// </summary>
public sealed class DesktopApp : Application
{
    private readonly DesktopBackground _background = new();
    private readonly WindowLayer       _windows    = new();
    private readonly Taskbar           _taskbar    = new();

    private DesktopWindow? _focused;

    public int WorkspaceWidth  => ScreenWidth;
    public int WorkspaceHeight => Math.Max(0, ScreenHeight - Taskbar.DockHeight - Taskbar.DockBottomGap);

    protected override void OnStart()
    {
        Add(_background);
        Add(_windows);
        Add(_taskbar);

        _background.OnResize(ScreenWidth, ScreenHeight);
        _windows.OnResize(ScreenWidth, ScreenHeight);
        _taskbar.OnResize(ScreenWidth, ScreenHeight);

        // Register the dock apps (id, label, accent color, Material Symbols glyph).
        _taskbar.RegisterApp("files",    "Files",    "#1d4ed8", Glyphs.Folder);
        _taskbar.RegisterApp("settings", "Settings", "#1e40af", Glyphs.Settings);
        _taskbar.RegisterApp("browser",  "Browser",  "#2563eb", Glyphs.Browser);

        _taskbar.IconActivated += OnDockClicked;

        // Open one window at startup so the desktop isn't empty.
        OpenOrFocus("files");
    }

    private void OnDockClicked(string appId)
    {
        DesktopWindow? existing = _windows.Find(appId);
        if (existing == null)
        {
            OpenOrFocus(appId);
            return;
        }
        if (existing.IsMinimized)
        {
            existing.Restore();
            FocusWindow(existing);
            return;
        }
        if (_focused == existing)
        {
            existing.MinimizeAnimated();
            UpdateDockState();
            return;
        }
        FocusWindow(existing);
    }

    private void OpenOrFocus(string appId)
    {
        DesktopWindow? existing = _windows.Find(appId);
        if (existing != null) { FocusWindow(existing); return; }

        DesktopWindow window = appId switch
        {
            "files"    => new DesktopWindow("Files",    appId, new FilesApp(),    120, 80,  640, 420),
            "settings" => new DesktopWindow("Settings", appId, new SettingsApp(), 220, 140, 560, 380),
            "browser"  => new DesktopWindow("Browser",  appId, new BrowserApp(),  180, 120, 720, 460),
            _          => throw new InvalidOperationException($"Unknown app: {appId}")
        };

        window.Activated += FocusWindow;
        window.Closed    += w => { _windows.Remove(w); if (_focused == w) _focused = null; UpdateDockState(); };
        window.Minimized += _ => UpdateDockState();
        window.Restored  += FocusWindow;

        _windows.Add(window);
        // Animate in
        window.Restore();
        FocusWindow(window);
    }

    private void FocusWindow(DesktopWindow w)
    {
        _windows.BringToFront(w);
        _focused = w;
        UpdateDockState();
        RequestRedraw();
    }

    private void UpdateDockState()
    {
        // Snapshot the entry list first — SetState mutates the backing List<T>
        // via index assignment which increments the list version and would cause
        // InvalidOperation_EnumFailedVersion if we used foreach directly.
        IReadOnlyList<DockEntry> snapshot = _taskbar.Entries;
        for (int i = 0; i < snapshot.Count; i++)
        {
            string appId  = snapshot[i].AppId;
            DesktopWindow? w = _windows.Find(appId);
            bool running = w != null;
            bool focused = w != null && w == _focused && !w.IsMinimized;
            _taskbar.SetState(appId, running, focused);
        }
    }
}

/// <summary>
/// A root control that paints the wallpaper and pumps the animation engine + a
/// continuous redraw loop. This lets the aurora drift and the clock update
/// even when no input is happening.
/// </summary>
internal sealed class DesktopBackground : Control
{
    public DesktopBackground()
    {
        Width = 0; Height = 0;
    }

    public override void OnResize(int canvasWidth, int canvasHeight)
    {
        Width = canvasWidth; Height = canvasHeight;
    }

    public override Control? HitTest(int x, int y) => null;   // never blocks input

    public override void OnPaint(Canvas canvas)
    {
        Wallpaper.Paint(canvas, Width, Height);
        AnimationEngine.Tick();

        // Keep the canvas redrawing for live clock + aurora animation.
        Application.Current?.RequestRedraw();
    }
}

/// <summary>
/// Z-ordered container of desktop windows. Children are stored in
/// back-to-front render order; the last child is the top-most window.
/// </summary>
internal sealed class WindowLayer : Control
{
    public override void OnResize(int canvasWidth, int canvasHeight)
    {
        Width = canvasWidth; Height = canvasHeight;
    }

    public override Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            Control? hit = Children[i].HitTest(x, y);
            if (hit != null) return hit;
        }
        return null;
    }

    public override void OnPaint(Canvas canvas) { /* children paint themselves */ }

    public DesktopWindow? Find(string appId)
    {
        for (int i = 0; i < Children.Count; i++)
            if (Children[i] is DesktopWindow dw && dw.IconId == appId) return dw;
        return null;
    }

    public void BringToFront(DesktopWindow w)
    {
        if (Children.Remove(w)) Children.Add(w);
    }

    public void Remove(DesktopWindow w) => Children.Remove(w);
}
