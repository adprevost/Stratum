// Stratum.DSL/DslParser.cs
using System.Reflection;
using Stratum.Core;
using Stratum.Controls;

namespace Stratum.DSL;

public class DslParser
{
    public Control Parse(string dsl, object? handlerTarget = null)
    {
        var tokens = new Tokenizer(dsl).Tokenize();
        var p = new ParserState(tokens, handlerTarget);
        p.ExpectIdent("ui");
        return p.ParseControl();
    }
}

internal sealed class ParserState
{
    private readonly List<Token> _tokens;
    private readonly object?     _handlerTarget;
    private int _pos;

    public ParserState(List<Token> tokens, object? handlerTarget)
    {
        _tokens = tokens;
        _handlerTarget = handlerTarget;
    }

    private Token Peek() => _tokens[Math.Min(_pos, _tokens.Count - 1)];
    private Token Next() { var t = _tokens[_pos]; _pos++; return t; }

    public void ExpectIdent(string value)
    {
        var t = Next();
        if (t.Kind != TokenKind.Identifier || !string.Equals(t.Value, value, StringComparison.OrdinalIgnoreCase))
            throw new DslException(t.Line, t.Col, $"Expected '{value}', got '{t.Value}'");
    }

    private Token ExpectKind(TokenKind kind)
    {
        var t = Next();
        if (t.Kind != kind)
            throw new DslException(t.Line, t.Col, $"Expected {kind}, got {t.Kind}('{t.Value}')");
        return t;
    }

    private int ParseInt()
    {
        var t = ExpectKind(TokenKind.Number);
        if (!int.TryParse(t.Value, out int v))
            throw new DslException(t.Line, t.Col, $"Expected integer, got '{t.Value}'");
        return v;
    }

    // Parse "N,N" coords
    private (int x, int y) ParseCoords()
    {
        int x = ParseInt();
        ExpectKind(TokenKind.Comma);
        int y = ParseInt();
        return (x, y);
    }

    // Parse "NxN" size: Number 'x' Number
    private (int w, int h)? TryParseSize()
    {
        if (Peek().Kind != TokenKind.Number) return null;
        // Save pos in case this isn't a size
        int saved = _pos;
        int w = ParseInt();
        var xt = Peek();
        if (xt.Kind == TokenKind.Identifier && xt.Value.Equals("x", StringComparison.OrdinalIgnoreCase))
        {
            Next(); // consume 'x'
            int h = ParseInt();
            return (w, h);
        }
        _pos = saved;
        return null;
    }

    public Control ParseControl()
    {
        var typeTok = ExpectKind(TokenKind.Identifier);
        string typeName = typeTok.Value;

        // Optional name string
        string? name = null;
        if (Peek().Kind == TokenKind.String)
            name = Next().Value;

        // Coords: x,y
        var (cx, cy) = ParseCoords();

        // Optional size: WxH
        int cw = 0, ch = 0;
        var sz = TryParseSize();
        if (sz.HasValue) { cw = sz.Value.w; ch = sz.Value.h; }

        // Build the control
        Control ctrl = typeName.ToLowerInvariant() switch
        {
            "panel"     => new Panel(cx, cy, cw, ch),
            "flowpanel" => new FlowPanel(cx, cy, cw, ch),
            "label"     => new Label(name ?? "", cx, cy, cw > 0 ? cw : 200, ch > 0 ? ch : 24),
            "button"    => new Button(name ?? "", cx, cy, cw > 0 ? cw : 120, ch > 0 ? ch : 36),
            "textbox"   => new TextBox(cx, cy, cw > 0 ? cw : 200, ch > 0 ? ch : 36),
            "checkbox"  => new CheckBox(name ?? "", cx, cy, cw > 0 ? cw : 200, ch > 0 ? ch : 28),
            "datagrid"  => new DataGrid(cx, cy, cw, ch),
            _ => throw new DslException(typeTok.Line, typeTok.Col, $"Unknown control type '{typeName}'")
        };
        if (name != null) ctrl.Name = name;

        // Parse attributes until '{' or next control keyword / EOF
        ParseAttributes(ctrl, typeTok);

        // Optional child block
        if (Peek().Kind == TokenKind.LBrace)
        {
            Next(); // consume '{'
            while (Peek().Kind != TokenKind.RBrace)
            {
                if (Peek().Kind == TokenKind.EOF)
                    throw new DslException(Peek().Line, Peek().Col, "Unexpected EOF, expected '}'");

                if (ctrl is DataGrid dg && Peek().Kind == TokenKind.Identifier &&
                    Peek().Value.Equals("Column", StringComparison.OrdinalIgnoreCase))
                {
                    dg.Columns.Add(ParseColumn());
                }
                else
                {
                    ctrl.Add(ParseControl());
                }
            }
            Next(); // consume '}'
        }

        return ctrl;
    }

    private void ParseAttributes(Control ctrl, Token typeTok)
    {
        while (true)
        {
            var t = Peek();
            // Stop on '{', '}', EOF, or next control keyword (Identifier that is a control type)
            if (t.Kind == TokenKind.LBrace || t.Kind == TokenKind.RBrace || t.Kind == TokenKind.EOF)
                break;
            if (t.Kind == TokenKind.Identifier && IsControlType(t.Value))
                break;
            // Stop if next token is a Number (start of coords for a sibling) — but this shouldn't happen
            // in attributes position. Attributes start with Identifier.
            if (t.Kind != TokenKind.Identifier) break;

            var attrTok = Next();
            string attr = attrTok.Value;

            if (Peek().Kind == TokenKind.Colon)
            {
                Next(); // consume ':'
                var valTok = Next();
                string val = valTok.Value;
                ApplyAttribute(ctrl, attr, val, attrTok);
            }
            else
            {
                // Flag attribute (no value)
                ApplyFlagAttribute(ctrl, attr, attrTok);
            }
        }
    }

    private void ApplyAttribute(Control ctrl, string attr, string value, Token attrTok)
    {
        switch (attr.ToLowerInvariant())
        {
            case "placeholder" when ctrl is TextBox tb:
                tb.Placeholder = value; break;
            case "fontsize" when ctrl is Label lbl:
                if (int.TryParse(value, out int fs)) lbl.FontSize = fs; break;
            case "color" when ctrl is Label lbl2:
                lbl2.Color = value; break;
            case "onclick" when ctrl is Button btn:
                var mi = _handlerTarget?.GetType().GetMethod(value,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (mi == null)
                    throw new DslException(attrTok.Line, attrTok.Col, $"Handler '{value}' not found.");
                btn.Click += () => mi.Invoke(_handlerTarget, null);
                break;
            default:
                // Unknown attribute — silently ignore for extensibility
                break;
        }
    }

    private void ApplyFlagAttribute(Control ctrl, string attr, Token attrTok)
    {
        switch (attr.ToLowerInvariant())
        {
            case "password" when ctrl is TextBox tb:
                tb.Password = true; break;
            case "secondary" when ctrl is Button btn:
                btn.Primary = false; break;
            case "checked" when ctrl is CheckBox cb:
                cb.Checked = true; break;
            case "drawborder" when ctrl is Panel p:
                p.DrawBorder = true; break;
            case "bold" when ctrl is Label lbl:
                lbl.Bold = true; break;
            default:
                break;
        }
    }

    private DataGridColumn ParseColumn()
    {
        Next(); // consume 'Column'
        var col = new DataGridColumn();

        if (Peek().Kind == TokenKind.String)
            col.Header = Next().Value;

        // Parse column attributes
        while (Peek().Kind == TokenKind.Identifier)
        {
            var attrTok = Next();
            if (Peek().Kind == TokenKind.Colon)
            {
                Next();
                var valTok = Next();
                switch (attrTok.Value.ToLowerInvariant())
                {
                    case "width":
                        if (int.TryParse(valTok.Value, out int w)) col.Width = w;
                        break;
                    case "getter":
                        string propName = valTok.Value;
                        col.ValueGetter = item => item?.GetType().GetProperty(propName)?.GetValue(item)?.ToString() ?? "";
                        break;
                }
            }
        }
        return col;
    }

    private static bool IsControlType(string name) => name.ToLowerInvariant() is
        "panel" or "flowpanel" or "label" or "button" or "textbox" or "checkbox" or "datagrid";
}
