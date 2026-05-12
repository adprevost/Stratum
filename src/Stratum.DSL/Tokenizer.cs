// Stratum.DSL/Tokenizer.cs
namespace Stratum.DSL;

internal enum TokenKind
{
    LBrace, RBrace, LParen, RParen,
    Colon, Comma,
    Identifier, String, Number,
    EOF
}

internal sealed class Token
{
    public TokenKind Kind  { get; }
    public string    Value { get; }
    public int       Line  { get; }
    public int       Col   { get; }

    public Token(TokenKind kind, string value, int line, int col)
    {
        Kind = kind; Value = value; Line = line; Col = col;
    }

    public override string ToString() => $"{Kind}({Value}) @{Line}:{Col}";
}

internal sealed class Tokenizer
{
    private readonly string _src;
    private int _pos;
    private int _line = 1;
    private int _col  = 1;

    public Tokenizer(string src) { _src = src; }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (true)
        {
            SkipWhitespaceAndComments();
            if (_pos >= _src.Length) { tokens.Add(Tok(TokenKind.EOF, "")); break; }

            char c = _src[_pos];
            int l = _line, col = _col;

            if (c == '{')      { tokens.Add(Tok(TokenKind.LBrace, "{")); Advance(); }
            else if (c == '}') { tokens.Add(Tok(TokenKind.RBrace, "}")); Advance(); }
            else if (c == '(') { tokens.Add(Tok(TokenKind.LParen, "(")); Advance(); }
            else if (c == ')') { tokens.Add(Tok(TokenKind.RParen, ")")); Advance(); }
            else if (c == ':') { tokens.Add(Tok(TokenKind.Colon,  ":")); Advance(); }
            else if (c == ',') { tokens.Add(Tok(TokenKind.Comma,  ",")); Advance(); }
            else if (c == '"') { tokens.Add(ReadString()); }
            else if (char.IsDigit(c) || (c == '-' && _pos + 1 < _src.Length && char.IsDigit(_src[_pos + 1])))
                                { tokens.Add(ReadNumber()); }
            else if (char.IsLetter(c) || c == '_')
                                { tokens.Add(ReadIdentifier()); }
            else
                throw new DslException(_line, _col, $"Unexpected character '{c}'");
        }
        return tokens;
    }

    private void SkipWhitespaceAndComments()
    {
        while (_pos < _src.Length)
        {
            char c = _src[_pos];
            if (c == '\n') { _line++; _col = 1; _pos++; }
            else if (char.IsWhiteSpace(c)) { Advance(); }
            else if (_pos + 1 < _src.Length && c == '/' && _src[_pos + 1] == '/')
            {
                while (_pos < _src.Length && _src[_pos] != '\n') _pos++;
            }
            else break;
        }
    }

    private Token ReadString()
    {
        int l = _line, col = _col;
        Advance(); // skip opening "
        var sb = new System.Text.StringBuilder();
        while (_pos < _src.Length && _src[_pos] != '"')
        {
            if (_src[_pos] == '\\' && _pos + 1 < _src.Length)
            {
                _pos++;
                sb.Append(_src[_pos] == 'n' ? '\n' : _src[_pos]);
                Advance();
            }
            else { sb.Append(_src[_pos]); Advance(); }
        }
        if (_pos >= _src.Length) throw new DslException(l, col, "Unterminated string");
        Advance(); // skip closing "
        return new Token(TokenKind.String, sb.ToString(), l, col);
    }

    private Token ReadNumber()
    {
        int l = _line, col = _col;
        var sb = new System.Text.StringBuilder();
        if (_src[_pos] == '-') { sb.Append('-'); Advance(); }
        while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.'))
        {
            sb.Append(_src[_pos]); Advance();
        }
        return new Token(TokenKind.Number, sb.ToString(), l, col);
    }

    private Token ReadIdentifier()
    {
        int l = _line, col = _col;
        var sb = new System.Text.StringBuilder();
        // DSL identifiers are letters and underscores only.
        // Digits following immediately after a letter are emitted as a separate Number token,
        // which keeps "NxN" size syntax unambiguous (x is Identifier, N is Number).
        while (_pos < _src.Length && (char.IsLetter(_src[_pos]) || _src[_pos] == '_'))
        {
            sb.Append(_src[_pos]); Advance();
        }
        return new Token(TokenKind.Identifier, sb.ToString(), l, col);
    }

    private Token Tok(TokenKind kind, string value) => new(kind, value, _line, _col);

    private void Advance()
    {
        if (_pos < _src.Length && _src[_pos] != '\n') _col++;
        _pos++;
    }
}
