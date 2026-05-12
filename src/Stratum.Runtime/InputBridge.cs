// Stratum.Runtime/InputBridge.cs
using System.Runtime.InteropServices.JavaScript;
using Stratum.Core;

namespace Stratum.Runtime;

public static partial class InputBridge
{
    [JSExport]
    public static void OnMouseMove(int x, int y)
        => Application.Current?.DispatchMouseMove(x, y);

    [JSExport]
    public static void OnMouseDown(int x, int y, int btn)
        => Application.Current?.DispatchMouseDown(x, y, btn);

    [JSExport]
    public static void OnMouseUp(int x, int y, int btn)
        => Application.Current?.DispatchMouseUp(x, y, btn);

    [JSExport]
    public static void OnKeyDown(string key, string code, bool ctrl, bool shift, bool alt)
        => Application.Current?.DispatchKeyDown(key, code, ctrl, shift, alt);

    [JSExport]
    public static void OnKeyPress(string key)
        => Application.Current?.DispatchKeyPress(key);

    [JSExport]
    public static void OnResize(int w, int h)
        => Application.Current?.DispatchResize(w, h);
}
