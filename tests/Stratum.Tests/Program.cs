using Stratum.Core;
using Stratum.Controls;
using Stratum.DSL;

int failed = 0;
void Test(string name, Action body)
{
    try { body(); Console.WriteLine($"  PASS  {name}"); }
    catch (Exception ex) { failed++; Console.WriteLine($"  FAIL  {name}: {ex.Message}"); }
}

Test("Parse empty panel", () => {
    var c = new DslParser().Parse("ui Panel \"root\" 0,0 100x100 { }");
    if (c is not Panel p) throw new Exception("not a Panel");
    if (p.Width != 100 || p.Height != 100) throw new Exception("size wrong");
});

Test("Parse label child", () => {
    var c = new DslParser().Parse(
      "ui Panel \"r\" 0,0 200x200 { Label \"Hi\" 10,20 100x24 }");
    if (c.Children.Count != 1) throw new Exception("child count");
    if (((Label)c.Children[0]).Text != "Hi") throw new Exception("text wrong");
});

Test("Parse button with handler", () => {
    var target = new TestTarget();
    var c = new DslParser().Parse(
      "ui Panel \"r\" 0,0 200x200 { Button \"Go\" 0,0 80x30 onClick:Hit }", target);
    var btn = (Button)c.Children[0];
    btn.OnClick(new MouseEventArgs());
    if (!target.WasHit) throw new Exception("handler not invoked");
});

Test("Parse checkbox with checked attr", () => {
    var c = new DslParser().Parse(
      "ui Panel \"r\" 0,0 200x200 { CheckBox \"X\" 0,0 100x24 checked }");
    if (!((CheckBox)c.Children[0]).Checked) throw new Exception("not checked");
});

Test("Parse error has line/col", () => {
    try {
        new DslParser().Parse("ui Panel 0,0 100x100 {");  // no closing brace
        throw new Exception("expected DslException");
    } catch (DslException) { /* ok */ }
});

Console.WriteLine(failed == 0 ? "ALL PASS" : $"{failed} FAILED");
return failed == 0 ? 0 : 1;

class TestTarget { public bool WasHit; public void Hit() { WasHit = true; } }

