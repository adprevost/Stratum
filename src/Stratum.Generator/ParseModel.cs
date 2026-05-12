// Stratum.Generator/ParseModel.cs
using System.Collections.Generic;

namespace Stratum.Generator;

public class StratumPage
{
    public string Name       { get; set; } = "";
    public int Width         { get; set; } = 800;
    public int Height        { get; set; } = 600;
    public string Background { get; set; } = "background";
    public string Title      { get; set; } = "";
    public List<StratumControl> Controls { get; } = new List<StratumControl>();
}

public class StratumControl
{
    public string Type       { get; set; } = "";
    public string Name       { get; set; } = "";
    public int    Line       { get; set; }
    public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    public List<StratumColumn> Columns { get; } = new List<StratumColumn>();
    public List<string>        Tabs    { get; } = new List<string>();
    public List<NavEntryDecl>  NavEntries { get; } = new List<NavEntryDecl>();
}

public class StratumColumn
{
    public string Header { get; set; } = "";
    public int    Width  { get; set; } = 100;
}

public class NavEntryDecl
{
    public bool   IsGroup { get; set; }
    public string Label   { get; set; } = "";
}
