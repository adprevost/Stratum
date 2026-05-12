// Stratum.Generator/StratumParser.cs
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Stratum.Generator;

internal class StratumParser
{
    private static readonly HashSet<string> KnownControls = new HashSet<string>(StringComparer.Ordinal)
        { "Label", "Button", "TextBox", "CheckBox", "DataGrid", "Panel",
          "ProgressBar", "Tabs", "Modal", "SidebarNav" };

    private static readonly Dictionary<string, HashSet<string>> KnownProperties =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
    {
        ["Label"]      = new HashSet<string> { "at","size","visible","enabled","opacity","text","font","color","align" },
        ["Button"]     = new HashSet<string> { "at","size","visible","enabled","opacity","text","style" },
        ["TextBox"]    = new HashSet<string> { "at","size","visible","enabled","opacity","placeholder","masked" },
        ["CheckBox"]   = new HashSet<string> { "at","size","visible","enabled","opacity","text","checked" },
        ["DataGrid"]   = new HashSet<string> { "at","size","visible","enabled","opacity" },
        ["Panel"]      = new HashSet<string> { "at","size","visible","enabled","opacity","background","border","borderColor","radius" },
        ["ProgressBar"]= new HashSet<string> { "at","size","visible","enabled","opacity","value","showLabel","striped","barColor","trackColor" },
        ["Tabs"]       = new HashSet<string> { "at","size","visible","enabled","opacity" },
        ["Modal"]      = new HashSet<string> { "visible","title","message","width","overlayOpacity" },
        ["SidebarNav"] = new HashSet<string> { "at","size","visible","enabled","opacity","background","accentColor" },
    };

    private readonly string _filePath;
    private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public StratumParser(string filePath) { _filePath = filePath; }

    public StratumPage? Parse(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var page = new StratumPage();
        bool pageFound = false;
        bool pagePropertiesDone = false;
        StratumControl? current = null;
        var controlNames = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < lines.Length; i++)
        {
            string raw = lines[i];
            int lineNum = i + 1;

            // Tab check
            foreach (char ch in raw)
            {
                if (ch == '\t') { AddDiag(DiagnosticDescriptors.STRATUM004_TabCharacter, lineNum, 1); break; }
                if (ch != ' ') break;
            }

            string stripped = StripComment(raw);
            if (string.IsNullOrWhiteSpace(stripped)) continue;

            int indent = GetIndent(stripped, lineNum);
            if (indent < 0) continue;
            string trimmed = stripped.TrimStart();

            if (!pageFound)
            {
                if (!trimmed.StartsWith("page ", StringComparison.Ordinal))
                {
                    AddDiag(DiagnosticDescriptors.STRATUM005_NoPageDeclaration, lineNum, 1);
                    return null;
                }
                page.Name = trimmed.Substring(5).Trim();
                page.Title = page.Name;
                pageFound = true;
                continue;
            }

            // Level 0 line after page header
            if (indent == 0)
            {
                if (trimmed.StartsWith("page ", StringComparison.Ordinal))
                    AddDiag(DiagnosticDescriptors.STRATUM006_MultiplePageDeclarations, lineNum, 1);
                continue;
            }

            if (indent == 1)
            {
                // Could be page property or control declaration
                if (trimmed.Contains(":") && !pagePropertiesDone)
                {
                    // Try as page property
                    if (ParsePageProperty(page, trimmed, lineNum))
                        continue;
                }

                // Control declaration
                pagePropertiesDone = true;
                current = ParseControlDeclaration(trimmed, lineNum, controlNames);
                if (current != null)
                    page.Controls.Add(current);
                continue;
            }

            if (indent == 2 && current != null)
            {
                // Column declaration for DataGrid
                if (trimmed.StartsWith("Column ", StringComparison.Ordinal))
                {
                    if (current.Type == "DataGrid")
                    {
                        var col = ParseColumn(trimmed, lineNum);
                        if (col != null) current.Columns.Add(col);
                    }
                    continue;
                }

                // Tab declaration for Tabs control: Tab: "Label"
                if (current.Type == "Tabs" && trimmed.StartsWith("Tab:", StringComparison.Ordinal))
                {
                    string tv = trimmed.Substring(4).Trim().Trim('"');
                    current.Tabs.Add(tv);
                    continue;
                }

                // SidebarNav child entries
                if (current.Type == "SidebarNav" && trimmed.StartsWith("Group:", StringComparison.Ordinal))
                {
                    current.NavEntries.Add(new NavEntryDecl
                    {
                        IsGroup = true,
                        Label   = trimmed.Substring(6).Trim().Trim('"')
                    });
                    continue;
                }
                if (current.Type == "SidebarNav" && trimmed.StartsWith("NavItem:", StringComparison.Ordinal))
                {
                    current.NavEntries.Add(new NavEntryDecl
                    {
                        IsGroup = false,
                        Label   = trimmed.Substring(8).Trim().Trim('"')
                    });
                    continue;
                }

                // Property
                int colon = trimmed.IndexOf(':');
                if (colon < 0) continue;
                string key = trimmed.Substring(0, colon).Trim();
                string val = trimmed.Substring(colon + 1).Trim();

                if (KnownProperties.TryGetValue(current.Type, out var allowed) && !allowed.Contains(key))
                {
                    string suggestion = FindClose(key, allowed);
                    AddDiag(DiagnosticDescriptors.STRATUM003_UnknownProperty, lineNum, 1,
                        key, current.Type, suggestion.Length > 0 ? $". Did you mean '{suggestion}'?" : "");
                }
                else
                {
                    current.Properties[key] = val;
                }
                continue;
            }
        }

        if (!pageFound)
        {
            AddDiag(DiagnosticDescriptors.STRATUM005_NoPageDeclaration, 0, 0);
            return null;
        }

        // Warn missing 'at'
        foreach (var ctrl in page.Controls)
        {
            if (ctrl.Type == "Modal") continue;
            if (!ctrl.Properties.ContainsKey("at") && !string.IsNullOrEmpty(ctrl.Name))
                AddDiag(DiagnosticDescriptors.STRATUM101_MissingAt, ctrl.Line, 3, ctrl.Name);
        }

        return page;
    }

    private bool ParsePageProperty(StratumPage page, string trimmed, int lineNum)
    {
        int colon = trimmed.IndexOf(':');
        if (colon < 0) return false;
        string key = trimmed.Substring(0, colon).Trim();
        string val = trimmed.Substring(colon + 1).Trim();
        switch (key)
        {
            case "size":
                var dim = ParseDimension(val);
                if (dim.HasValue) { page.Width = dim.Value.Item1; page.Height = dim.Value.Item2; }
                return true;
            case "background":
                page.Background = val;
                return true;
            case "title":
                page.Title = val.Trim('"');
                return true;
        }
        return false;
    }

    private StratumControl? ParseControlDeclaration(string trimmed, int lineNum, HashSet<string> names)
    {
        string[] parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        string type = parts[0];
        string name = parts.Length > 1 ? parts[1].Trim() : "";

        if (!KnownControls.Contains(type))
        {
            string suggestion = FindClose(type, KnownControls);
            AddDiag(DiagnosticDescriptors.STRATUM002_UnknownControlType, lineNum, 1,
                type, suggestion.Length > 0 ? $". Did you mean '{suggestion}'?" : "");
            return null;
        }

        if (string.IsNullOrEmpty(name))
            AddDiag(DiagnosticDescriptors.STRATUM102_UnnamedControl, lineNum, 3, type);
        else if (!names.Add(name))
            AddDiag(DiagnosticDescriptors.STRATUM007_DuplicateControlName, lineNum, 3, name);

        return new StratumControl { Type = type, Name = name, Line = lineNum };
    }

    private StratumColumn? ParseColumn(string trimmed, int lineNum)
    {
        // Column "Header" width: N
        var col = new StratumColumn();
        int q1 = trimmed.IndexOf('"');
        int q2 = q1 >= 0 ? trimmed.IndexOf('"', q1 + 1) : -1;
        if (q1 >= 0 && q2 > q1)
            col.Header = trimmed.Substring(q1 + 1, q2 - q1 - 1);

        int w = trimmed.IndexOf("width:", StringComparison.Ordinal);
        if (w >= 0)
        {
            string wVal = trimmed.Substring(w + 6).Trim();
            if (int.TryParse(wVal, out int wInt)) col.Width = wInt;
        }
        return col;
    }

    private int GetIndent(string raw, int lineNum)
    {
        int spaces = 0;
        foreach (char c in raw)
        {
            if (c == ' ') spaces++;
            else break;
        }
        if (spaces % 2 != 0)
        {
            AddDiag(DiagnosticDescriptors.STRATUM009_BadIndent, lineNum, spaces + 1);
            return -1;
        }
        return spaces / 2;
    }

    private static string StripComment(string line)
    {
        int idx = line.IndexOf("//", StringComparison.Ordinal);
        return idx >= 0 ? line.Substring(0, idx) : line;
    }

    public static (int, int)? ParseDimension(string val)
    {
        int x = val.IndexOf('x');
        if (x < 0) return null;
        if (int.TryParse(val.Substring(0, x).Trim(), out int w) &&
            int.TryParse(val.Substring(x + 1).Trim(), out int h))
            return (w, h);
        return null;
    }

    public static (int, int)? ParsePosition(string val)
    {
        int comma = val.IndexOf(',');
        if (comma < 0) return null;
        if (int.TryParse(val.Substring(0, comma).Trim(), out int x) &&
            int.TryParse(val.Substring(comma + 1).Trim(), out int y))
            return (x, y);
        return null;
    }

    private static string FindClose(string input, IEnumerable<string> candidates)
    {
        string best = "";
        int bestDist = int.MaxValue;
        foreach (var c in candidates)
        {
            int d = EditDistance(input.ToLowerInvariant(), c.ToLowerInvariant());
            if (d < bestDist && d <= 3) { bestDist = d; best = c; }
        }
        return best;
    }

    private static int EditDistance(string a, string b)
    {
        int[,] dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                dp[i, j] = a[i - 1] == b[j - 1] ? dp[i - 1, j - 1]
                    : 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
        return dp[a.Length, b.Length];
    }

    private void AddDiag(DiagnosticDescriptor desc, int line, int col, params object[] args)
    {
        var loc = line > 0
            ? Location.Create(_filePath, TextSpan.FromBounds(0, 0),
                new LinePositionSpan(new LinePosition(line - 1, col - 1), new LinePosition(line - 1, col)))
            : Location.None;
        _diagnostics.Add(Diagnostic.Create(desc, loc, args));
    }
}
