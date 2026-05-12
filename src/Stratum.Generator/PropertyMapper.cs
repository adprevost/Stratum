// Stratum.Generator/PropertyMapper.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Stratum.Generator;

/// <summary>
/// Maps .stratum property key/value pairs to C# object-initializer assignments.
/// </summary>
internal static class PropertyMapper
{
    // Color token → Theme field
    private static readonly Dictionary<string, string> ColorTokens =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["background"] = "Theme.BackgroundColor",
        ["surface"]    = "Theme.SurfaceColor",
        ["primary"]    = "Theme.PrimaryColor",
        ["secondary"]  = "Theme.SecondaryColor",
        ["text"]       = "Theme.TextColor",
        ["muted"]      = "Theme.TextMuted",
        ["error"]      = "Theme.ErrorColor",
        ["success"]    = "Theme.SuccessColor",
        ["border"]     = "Theme.BorderColor",
    };

    // Font size token → Theme field
    private static readonly Dictionary<string, string> FontSizeTokens =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["sm"]  = "Theme.FontSizeSm",
        ["base"]= "Theme.FontSizeBase",
        ["lg"]  = "Theme.FontSizeLg",
        ["xl"]  = "Theme.FontSizeXl",
        ["xxl"] = "Theme.FontSizeXxl",
    };

    /// <summary>
    /// Returns a list of "Property = Value" strings for the object initializer.
    /// 'at' and 'size' are handled as constructor arguments and excluded here.
    /// </summary>
    public static List<string> MapProperties(StratumControl ctrl)
    {
        var result = new List<string>();
        foreach (var kv in ctrl.Properties)
        {
            var expr = MapOne(ctrl.Type, kv.Key, kv.Value);
            if (expr == null) continue;
            // Font may produce multiple props joined by '\u0001'
            if (expr.IndexOf('\u0001') >= 0)
            {
                foreach (var sub in expr.Split('\u0001'))
                    if (sub.Length > 0) result.Add(sub);
            }
            else result.Add(expr);
        }
        return result;
    }

    private static string? MapOne(string controlType, string key, string value)
    {
        switch (key)
        {
            case "at":
            case "size":
                return null; // constructor args

            case "text":
                return $"Text = {QuoteString(value)}";

            case "visible":
                return $"Visible = {value.ToLowerInvariant()}";

            case "enabled":
                return $"Enabled = {value.ToLowerInvariant()}";

            case "opacity":
                return $"Opacity = {value}";

            case "color":
                return $"Color = {MapColor(value)}";

            case "background":
                return $"Background = {MapColor(value)}";

            case "border":
                return $"DrawBorder = {value.ToLowerInvariant()}";

            case "borderColor":
                return $"BorderColor = {MapColor(value)}";

            case "radius":
                return $"BorderRadius = {value}";

            case "placeholder":
                return $"Placeholder = {QuoteString(value)}";

            case "masked":
                return $"Masked = {value.ToLowerInvariant()}";

            case "checked":
                return $"Checked = {value.ToLowerInvariant()}";

            case "style":
                return MapButtonStyle(value);

            case "align":
                return MapAlign(value);

            case "font":
                return MapFont(value);

            case "value":
                return $"Value = {value}";

            case "showLabel":
                return $"ShowLabel = {value.ToLowerInvariant()}";

            case "striped":
                return $"Striped = {value.ToLowerInvariant()}";

            case "barColor":
                return $"BarColor = {MapColor(value)}";

            case "trackColor":
                return $"TrackColor = {MapColor(value)}";

            case "title":
                return $"Title = {QuoteString(value)}";

            case "message":
                return $"Message = {QuoteString(value)}";

            case "width":
                return $"Width = {value}";

            case "overlayOpacity":
                return $"OverlayOpacity = {value}";

            case "accentColor":
                return $"AccentColor = {MapColor(value)}";

            default:
                return null;
        }
    }

    private static string MapColor(string value)
    {
        if (ColorTokens.TryGetValue(value, out var theme)) return theme;
        // hex or bare word
        return value.StartsWith("#") ? $"\"{value}\"" : $"\"{value}\"";
    }

    private static string MapButtonStyle(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "primary"   => "Style = ButtonStyle.Primary",
            "secondary" => "Style = ButtonStyle.Secondary",
            "ghost"     => "Style = ButtonStyle.Ghost",
            "danger"    => "Style = ButtonStyle.Danger",
            _           => "Style = ButtonStyle.Primary"
        };
    }

    private static string MapAlign(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "center" => "Align = TextAlign.Center",
            "right"  => "Align = TextAlign.Right",
            _        => "Align = TextAlign.Left"
        };
    }

    private static string MapFont(string value)
    {
        var sb = new StringBuilder();
        var tokens = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string? sizeExpr = null;
        bool bold = false;
        bool italic = false;

        foreach (var t in tokens)
        {
            if (FontSizeTokens.TryGetValue(t, out var sz))
                sizeExpr = $"FontSize = {sz}";
            else if (string.Equals(t, "bold", StringComparison.OrdinalIgnoreCase))
                bold = true;
            else if (string.Equals(t, "italic", StringComparison.OrdinalIgnoreCase))
                italic = true;
        }

        var parts = new List<string>();
        if (sizeExpr != null) parts.Add(sizeExpr);
        if (bold)   parts.Add("Bold = true");
        if (italic) parts.Add("Italic = true");

        return string.Join(", ", parts);
    }

    /// <summary>Returns constructor arguments (x, y, w, h) extracted from 'at' and 'size' properties.</summary>
    public static (int x, int y, int w, int h) GetPositionAndSize(StratumControl ctrl)
    {
        int x = 0, y = 0, w = 0, h = 0;
        if (ctrl.Properties.TryGetValue("at", out var atVal))
        {
            var pos = StratumParser.ParsePosition(atVal);
            if (pos.HasValue) { x = pos.Value.Item1; y = pos.Value.Item2; }
        }
        if (ctrl.Properties.TryGetValue("size", out var sizeVal))
        {
            var dim = StratumParser.ParseDimension(sizeVal);
            if (dim.HasValue) { w = dim.Value.Item1; h = dim.Value.Item2; }
        }
        return (x, y, w, h);
    }

    /// <summary>Returns the first text argument string (for Label/Button constructors) or null.</summary>
    public static string? GetTextArg(StratumControl ctrl)
    {
        if (ctrl.Properties.TryGetValue("text", out var v)) return v;
        return null;
    }

    private static string QuoteString(string value)
    {
        if (value.StartsWith("\"") && value.EndsWith("\"")) return value;
        return $"\"{value}\"";
    }
}
