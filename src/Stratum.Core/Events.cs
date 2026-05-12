// Stratum.Core/Events.cs
namespace Stratum.Core;

public class MouseEventArgs
{
    public int X      { get; init; }
    public int Y      { get; init; }
    public int Button { get; init; }  // 0=left, 1=middle, 2=right
}

public class KeyEventArgs
{
    public string Key  { get; init; } = "";
    public string Code { get; init; } = "";
    public bool Ctrl   { get; init; }
    public bool Shift  { get; init; }
    public bool Alt    { get; init; }
}

public enum MouseButton { Left = 0, Middle = 1, Right = 2 }
