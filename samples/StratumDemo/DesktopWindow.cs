// StratumDemo/DesktopWindow.cs
using Stratum.Core;

namespace StratumDemo;

internal enum HitZone { None, TitleBar, CloseButton, MinButton, MaxButton, Body }

/// <summary>
/// A single OS window: dark-purple glass title bar, Windows-style caption
/// buttons, drag-to-move, shade/roll-up minimize, and maximize animations.
/// Minimizing keeps the title bar visible (shade style) so the border and
/// chrome remain on screen.
/// </summary>
public sealed class DesktopWindow : Control
{
    public const int TitleBarHeight = 32;
    public const int CornerRadius   = 0;

    // Caption button geometry — three 40px-wide slots at the right edge.
    private const int BtnW = 40;

    public string Title    { get; }
    public string IconId   { get; }
    public Control Content { get; }

    public bool IsMinimized { get; private set; }
    public bool IsMaximized { get; private set; }

    public Action<DesktopWindow>? Closed;
    public Action<DesktopWindow>? Activated;
    public Action<DesktopWindow>? Minimized;
    public Action<DesktopWindow>? Restored;

    private HitZone _lastHitZone;
    private int _dragStartX, _dragStartY;
    private int _windowStartX, _windowStartY;
    private bool _dragging;

    private int _restoreX, _restoreY, _restoreW, _restoreH;

    // Current rendered height — animates to TitleBarHeight when shading.
    private int _renderedHeight;

    public DesktopWindow(string title, string iconId, Control content, int x, int y, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(content);
        Title   = title;
        IconId  = iconId;
        Content = content;
        X = x; Y = y; Width = width; Height = height;
        _restoreX = x; _restoreY = y; _restoreW = width; _restoreH = height;
        _renderedHeight = height;

        Content.X      = 0;
        Content.Y      = TitleBarHeight;
        Content.Width  = width;
        Content.Height = height - TitleBarHeight;
        Add(Content);
    }

    public void Activate() => Activated?.Invoke(this);
    public void Close()    => Closed?.Invoke(this);

    public void ToggleMinimize()
    {
        if (IsMinimized) Restore();
        else MinimizeAnimated();
    }

    public void ToggleMaximize(int workspaceWidth, int workspaceHeight)
    {
        if (IsMaximized)
        {
            IsMaximized = false;
            AnimateBounds(_restoreX, _restoreY, _restoreW, _restoreH);
        }
        else
        {
            _restoreX = X; _restoreY = Y; _restoreW = Width; _restoreH = Height;
            IsMaximized = true;
            AnimateBounds(0, 0, workspaceWidth, workspaceHeight);
        }
    }

    /// <summary>
    /// Shade/roll-up: animate the rendered height down to just the title bar.
    /// The title bar and border remain fully visible.
    /// </summary>
    public void MinimizeAnimated()
    {
        if (IsMinimized) return;
        IsMinimized = true;
        Content.Visible = false;

        int fromH = _renderedHeight;
        AnimationEngine.Tween(200, Ease.InOutQuad, t =>
        {
            _renderedHeight = (int)(fromH + (TitleBarHeight - fromH) * t);
        }, onComplete: () => Minimized?.Invoke(this));
    }

    /// <summary>Unshade: animate height back to the full window size.</summary>
    public void Restore()
    {
        if (!IsMinimized) return;
        IsMinimized = false;

        int fromH = _renderedHeight;
        int toH   = Height;
        AnimationEngine.Tween(220, Ease.OutCubic, t =>
        {
            _renderedHeight = (int)(fromH + (toH - fromH) * t);
        }, onComplete: () =>
        {
            Content.Visible = true;
            Restored?.Invoke(this);
        });
    }

    private void AnimateBounds(int tx, int ty, int tw, int th)
    {
        int sx = X, sy = Y, sw = Width, sh = Height;
        AnimationEngine.Tween(220, Ease.InOutQuad, t =>
        {
            X      = (int)(sx + (tx - sx) * t);
            Y      = (int)(sy + (ty - sy) * t);
            Width  = (int)(sw + (tw - sw) * t);
            Height = (int)(sh + (th - sh) * t);
            _renderedHeight = Height;
            Content.Width  = Width;
            Content.Height = Height - TitleBarHeight;
        });
    }

    public override Control? HitTest(int x, int y)
    {
        if (!Visible || !Enabled) return null;
        int ax = AbsoluteX, ay = AbsoluteY;

        // When minimized the clickable area is just the title-bar strip.
        int hitHeight = IsMinimized ? TitleBarHeight : Height;
        if (x < ax || x > ax + Width || y < ay || y > ay + hitHeight)
        {
            _lastHitZone = HitZone.None;
            return null;
        }

        int local = y - ay;
        if (local < TitleBarHeight)
        {
            int closeX = ax + Width - BtnW;
            int maxX   = closeX - BtnW;
            int minX   = maxX   - BtnW;

            if (x >= closeX) { _lastHitZone = HitZone.CloseButton; return this; }
            if (x >= maxX)   { _lastHitZone = HitZone.MaxButton;   return this; }
            if (x >= minX)   { _lastHitZone = HitZone.MinButton;   return this; }

            _lastHitZone = HitZone.TitleBar;
            return this;
        }

        if (!IsMinimized)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Control? hit = Children[i].HitTest(x, y);
                if (hit != null) { _lastHitZone = HitZone.Body; return hit; }
            }
        }

        _lastHitZone = HitZone.Body;
        return this;
    }

    public override void OnMouseDown(MouseEventArgs e)
    {
        Activate();
        switch (_lastHitZone)
        {
            case HitZone.CloseButton: Close(); break;
            case HitZone.MinButton:   ToggleMinimize(); break;
            case HitZone.MaxButton:
                if (Application.Current is DesktopApp da) ToggleMaximize(da.WorkspaceWidth, da.WorkspaceHeight);
                break;
            case HitZone.TitleBar when !IsMaximized:
                _dragging     = true;
                _dragStartX   = e.X; _dragStartY   = e.Y;
                _windowStartX = X;   _windowStartY = Y;
                break;
        }
    }

    public override void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging) return;
        X = _windowStartX + (e.X - _dragStartX);
        Y = _windowStartY + (e.Y - _dragStartY);

        if (Application.Current is DesktopApp da)
        {
            X = Math.Clamp(X, -Width + 80, da.WorkspaceWidth  - 40);
            Y = Math.Clamp(Y, 0,            da.WorkspaceHeight - TitleBarHeight);
        }
        Invalidate();
    }

    public override void OnMouseUp(MouseEventArgs e) => _dragging = false;

    public override void OnPaint(Canvas canvas)
    {
        int drawH = IsMinimized ? _renderedHeight : Height;

        // Drop shadow
        if (!IsMaximized)
            GlassRenderer.DrawShadow(canvas, X, Y, Width, drawH, 0);

        // ── Body (hidden when fully shaded) ─────────────────────────────────
        if (!IsMinimized || _renderedHeight > TitleBarHeight)
        {
            // Deep navy-blue glass surface
            canvas.SetFillStyle("rgba(12, 12, 14, 0.26)");
            canvas.FillRect(X, Y, Width, drawH);
        }

        // ── Title bar ────────────────────────────────────────────────────────
        canvas.SetFillStyle("rgba(22, 22, 26, 0.29)");
        canvas.FillRect(X, Y, Width, TitleBarHeight);

        // Title text — left-aligned
        canvas.SetFillStyle("rgba(255, 255, 255, 0.82)");
        canvas.SetFont("400 13px system-ui, -apple-system, sans-serif");
        canvas.SetTextBaseline("middle");
        canvas.FillText(Title, X + 14, Y + TitleBarHeight / 2);

        // ── Caption buttons ──────────────────────────────────────────────────
        int closeX = X + Width - BtnW;
        int maxX   = closeX - BtnW;
        int minX   = maxX   - BtnW;
        int btnMidY = Y + TitleBarHeight / 2;

        // Close — red
        canvas.SetFillStyle("rgba(192, 40, 28, 0.25)");
        canvas.FillRect(closeX, Y, BtnW, TitleBarHeight);

        // Maximise / Minimise — subtle tint
        canvas.SetFillStyle("rgba(255, 255, 255, 0.05)");
        canvas.FillRect(maxX, Y, BtnW, TitleBarHeight);
        canvas.FillRect(minX, Y, BtnW, TitleBarHeight);

        // Caption button glyphs via Material Symbols
        string maxGlyph = IsMaximized ? Glyphs.Restore : Glyphs.Maximize;
        canvas.DrawGlyph(Glyphs.Close,   closeX + BtnW / 2, btnMidY, 16, "rgba(255, 255, 255, 0.88)");
        canvas.DrawGlyph(maxGlyph,       maxX   + BtnW / 2, btnMidY, 16, "rgba(255, 255, 255, 0.88)");
        canvas.DrawGlyph(Glyphs.Minimize, minX  + BtnW / 2, btnMidY, 16, "rgba(255, 255, 255, 0.88)");

        // ── Borders ──────────────────────────────────────────────────────────
        // Title bar separator
        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.07)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.MoveTo(X, Y + TitleBarHeight);
        canvas.LineTo(X + Width, Y + TitleBarHeight);
        canvas.Stroke();

        // Outer window border — royal blue
        canvas.SetStrokeStyle("rgba(255, 255, 255, 0.04)");
        canvas.SetLineWidth(1);
        canvas.BeginPath();
        canvas.RoundRect(X, Y, Width, drawH, 0);
        canvas.Stroke();
    }

    public Rect TitleBarRect => new(X, Y, Width, TitleBarHeight);
}

public readonly record struct Rect(int X, int Y, int Width, int Height);
