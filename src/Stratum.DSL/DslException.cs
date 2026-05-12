// Stratum.DSL/DslException.cs
namespace Stratum.DSL;

public class DslException : Exception
{
    public int Line { get; }
    public int Col  { get; }

    public DslException(int line, int col, string message)
        : base($"DSL error at {line}:{col}: {message}")
    {
        Line = line;
        Col  = col;
    }
}
