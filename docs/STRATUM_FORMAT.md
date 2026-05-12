# The `.stratum` File Format

Complete syntax reference for Stratum's text-based UI format: indentation rules,
supported controls and properties, color tokens, code generation, and the
code-behind contract.

---

## 1. Overview

Every Stratum page consists of exactly two files:

| File | Purpose | Who owns it |
|------|---------|-------------|
| `PageName.stratum` | Declares what controls exist, where they are, and how they look | Developer or designer |
| `PageName.cs` | Declares logic, state, and event wiring | Developer |
| `PageName.stratum.g.cs` | Auto-generated at build time — never edit | Build system |

The `.stratum` file answers one question: **what is on this page and where?**
The `.cs` file answers one question: **what does this page do?**

These concerns never mix. The `.stratum` file contains no event references, no
binding declarations, no method names, and no logic of any kind.

---

## 2. Design Principles

**Principle 1 — Pure layout.**
No `onClick`, no `bind`, no `State`, no method references. If it is a behavior or
logic concern it belongs in the code-behind.

**Principle 2 — Implicitly reactive.**
Every property on every control is reactive by default. Assigning a value in the
code-behind automatically triggers a redraw. Never call `Invalidate()`,
`RequestRedraw()`, or `StateHasChanged()`.

**Principle 3 — Flat structure.**
Controls are a flat list on the page. Every control has an explicit absolute position.

**Principle 4 — Minimal syntax.**
No braces. No commas. No JSON. Indentation carries structure. A colon separates
property keys from values.

**Principle 5 — No manual plumbing.**
The developer does not declare control fields, instantiate controls, or register
controls with the page. All of that is generated.

**Principle 6 — One place for wiring.**
All event handler assignments happen in `OnPageStart()` in the code-behind.

---

## 3. File Format Specification

### 3.1 File Extension

`.stratum` — all lowercase.

### 3.2 Encoding

UTF-8. No BOM.

### 3.3 Indentation

Spaces only. Two spaces per level. A tab character anywhere in the file is a
build error (`STRATUM004`).

### 3.4 Structure

```
page PageName          <- level 0: page declaration
  ControlType name     <- level 1: control declaration
    key: value         <- level 2: property
    key: value         <- level 2: property
  ControlType name     <- level 1: next control
    key: value         <- level 2: property
```

There is no level 3. Controls cannot contain controls in v1.

### 3.5 Comments

Double-slash comments are supported on any line. Everything after `//` is ignored.

```
page Login
  // Authentication form
  Button loginBtn
    text: "Sign in"    // primary action
    at: 40, 240
```

### 3.6 Blank Lines

Blank lines are ignored everywhere.

---

## 4. Grammar

### 4.1 Page Declaration

```
page Identifier
```

- `page` is a reserved keyword, lowercase.
- `Identifier` is PascalCase and must match the partial class name in the code-behind.
- Must be the first non-blank, non-comment line. Exactly one per file.

### 4.2 Page Properties

Appear at level 1, before any control declarations.

```
page Login
  size: 480 x 360
  background: surface
  title: "Sign in"
```

| Property     | Type         | Default          | Description |
|--------------|--------------|------------------|-------------|
| `size`       | `W x H`      | `800 x 600`      | Canvas dimensions in pixels |
| `background` | color token  | `background`     | Page background color |
| `title`      | string       | page name        | Browser tab title |

### 4.3 Control Declaration

```
ControlType controlName
```

- `ControlType` matches the control class name exactly (case-sensitive).
- `controlName` is camelCase and is the generated field name; must be unique.
- The name is optional for purely decorative controls — unnamed controls generate
  no field and cannot be referenced in the code-behind (warning `STRATUM102`).

### 4.4 Property Declaration

```
  key: value
```

- Indented two spaces under the control declaration.
- Key is lowercase, no spaces.
- Colon immediately follows the key; one space before the value.
- No trailing commas.
- Strings containing spaces require quotes; bare words do not.

### 4.5 Value Types

| Type        | Format                          | Examples |
|-------------|---------------------------------|---------|
| String      | `"quoted text"` or bare word    | `"Hello World"` / `enabled` |
| Position    | `X, Y`                          | `40, 100` |
| Dimension   | `W x H`                         | `400 x 36` |
| Boolean     | `true` / `false`                | `visible: false` |
| Number      | bare integer or decimal         | `opacity: 0.5` |
| Color token | bare token name                 | `color: error` / `background: surface` |
| Hex color   | `#RRGGBB`                       | `color: #ff0000` |
| Font token  | token(s) separated by space     | `font: xl bold` |

---

## 5. Supported Controls and Properties

### 5.1 Universal Properties

Every control supports these regardless of type:

| Property  | Type      | Default | Description |
|-----------|-----------|---------|-------------|
| `at`      | position  | 0, 0    | Top-left corner in pixels |
| `size`    | dimension | varies  | Width and height |
| `visible` | boolean   | `true`  | Whether the control renders |
| `enabled` | boolean   | `true`  | Whether the control accepts input |
| `opacity` | 0–1       | `1`     | Transparency level |

### 5.2 Label

```
Label controlName
  text: "Display text"
  at: X, Y
  size: W x H
  font: [size] [bold] [italic]
  color: [token or hex]
  align: [left|center|right]
```

**Font size tokens:** `sm` `base` `lg` `xl` `xxl`
**Font style tokens:** `bold` `italic` (combinable: `font: lg bold`)

### 5.3 Button

```
Button controlName
  text: "Button Label"
  at: X, Y
  size: W x H
  style: [primary|secondary|ghost|danger]
```

### 5.4 TextBox

```
TextBox controlName
  at: X, Y
  size: W x H
  placeholder: "Hint text"
  masked: [true|false]
```

### 5.5 CheckBox

```
CheckBox controlName
  text: "Checkbox label"
  at: X, Y
  size: W x H
  checked: [true|false]
```

### 5.6 DataGrid

```
DataGrid controlName
  at: X, Y
  size: W x H
  Column "Header" width: N
  Column "Header" width: N
```

`Column` declarations are configuration, not controls. They appear indented under
the DataGrid's property block, generate no fields, and may only appear inside a
DataGrid declaration.

### 5.7 Panel

```
Panel controlName
  at: X, Y
  size: W x H
  background: [color token or hex]
  border: [true|false]
  borderColor: [color token or hex]
  radius: N
```

---

## 6. Color Tokens

Color tokens map to `Theme` values. Using tokens means the entire app can be
re-themed by changing `Theme` — no `.stratum` files need to change.

| Token        | Maps to                  | Typical use |
|--------------|--------------------------|-------------|
| `background` | `Theme.BackgroundColor`  | Page backgrounds |
| `surface`    | `Theme.SurfaceColor`     | Cards, panels |
| `primary`    | `Theme.PrimaryColor`     | Primary actions |
| `secondary`  | `Theme.SecondaryColor`   | Secondary actions |
| `text`       | `Theme.TextColor`        | Body text |
| `muted`      | `Theme.TextMuted`        | Hints, subtitles |
| `error`      | `Theme.ErrorColor`       | Validation errors |
| `success`    | `Theme.SuccessColor`     | Confirmations |
| `border`     | `Theme.BorderColor`      | Dividers, outlines |
| `transparent`| (no fill)                | Panel background |

Raw hex values are accepted anywhere a color token is accepted: `color: #2563eb`

---

## 7. Code Generation

### 7.1 What Gets Generated

For every named control in the `.stratum` file:
- A `protected` field of the correct control type.
- Instantiation with all properties from the `.stratum` file applied.
- Registration with the page via `Add()`.

Everything is inside `InitializeControls()`, which the base `Page` class calls
before `OnPageStart()`. The developer never calls it.

### 7.2 Generated File Structure

```csharp
// Login.stratum.g.cs
// AUTO-GENERATED — DO NOT EDIT
// Source: Login.stratum

using Stratum.Core;
using Stratum.Controls;

public partial class Login : Page
{
    protected Label   heading       = null!;
    protected TextBox emailInput    = null!;
    protected Button  loginBtn      = null!;

    protected override void InitializeControls()
    {
        heading = new Label("Welcome back", 40, 40, 400, 36)
        {
            FontSize = Theme.FontSizeXl,
            Bold     = true,
        };

        emailInput = new TextBox(40, 120, 400, 36)
        {
            Placeholder = "Email address"
        };

        loginBtn = new Button("Sign in", 40, 252, 400, 44)
        {
            Style = ButtonStyle.Primary
        };

        Add(heading);
        Add(emailInput);
        Add(loginBtn);
    }
}
```

### 7.3 Generation Rules

- The generated class is always `partial` and always inherits `Page`.
- Fields are `protected` — accessible from the code-behind partial class.
- Fields are initialised to `null!` at declaration and assigned in `InitializeControls()`.
- Controls are added in the order they appear in the `.stratum` file.
- Unnamed controls are instantiated and added but generate no field.
- If a `.stratum` file has not changed since the last build, regeneration is skipped.
- Generated files live in `obj/` and are never checked into source control.

### 7.4 Property Mapping

| `.stratum` property   | Generated C# |
|-----------------------|--------------|
| `at: X, Y`            | Constructor arguments X, Y |
| `size: W x H`         | Constructor arguments W, H |
| `text: "value"`       | `Text = "value"` |
| `visible: false`      | `Visible = false` |
| `enabled: false`      | `Enabled = false` |
| `font: xl bold`       | `FontSize = Theme.FontSizeXl, Bold = true` |
| `color: error`        | `Color = Theme.ErrorColor` |
| `color: #hex`         | `Color = "#hex"` |
| `style: primary`      | `Style = ButtonStyle.Primary` |
| `style: secondary`    | `Style = ButtonStyle.Secondary` |
| `style: ghost`        | `Style = ButtonStyle.Ghost` |
| `style: danger`       | `Style = ButtonStyle.Danger` |
| `placeholder: "text"` | `Placeholder = "text"` |
| `masked: true`        | `Masked = true` |
| `checked: true`       | `Checked = true` |
| `background: surface` | `Background = Theme.SurfaceColor` |
| `border: true`        | `DrawBorder = true` |
| `radius: N`           | `BorderRadius = N` |
| `align: center`       | `Align = TextAlign.Center` |
| `opacity: 0.5`        | `Opacity = 0.5f` |

---

## 8. Reactive Properties

Every settable property on a control uses a reactive backing. When the value
changes a redraw is automatically scheduled. When the value is set to the same
value it already holds, nothing happens.

```csharp
// The developer just assigns. The UI updates automatically.
errorMsg.Visible = true;
errorMsg.Text    = "Invalid email or password.";
loginBtn.Enabled = false;
```

No `Invalidate()`. No `RequestRedraw()`. No `StateHasChanged()`.

When multiple controls display the same data, use a C# property setter as the
single point of update:

```csharp
private List<Invoice> Invoices
{
    get => _invoices;
    set
    {
        _invoices = value;
        invoiceGrid.Items = value.Cast<object>().ToList();
        statusLabel.Text  = $"{value.Count} invoices";
        totalLabel.Text   = $"Total: {value.Sum(i => i.Amount):C}";
    }
}
```

---

## 9. The Code-Behind Contract

### 9.1 Class Declaration

The code-behind declares a `partial` class with the same name as the page
identifier in the `.stratum` file. It must **not** inherit `Page` — the generated
file handles inheritance.

```csharp
// Login.cs
public partial class Login
{
    protected override void OnPageStart()
    {
        loginBtn.Click += Login;
        emailInput.Enter += Login;
        errorMsg.Visible = false;
    }

    private void Login()
    {
        // authentication logic
    }
}
```

If the names do not match, the build fails:
```
Login.stratum(0,0): error STRATUM001: Code-behind class 'SignIn' does not match page identifier 'Login'.
```

### 9.2 `OnPageStart`

Override `OnPageStart()` to wire events and set initial state. It is called after
`InitializeControls()` — all control fields are guaranteed to be non-null.

### 9.3 Rules

- All event handler assignments go in `OnPageStart()`.
- Never declare control fields manually — they are generated.
- Never call `InitializeControls()` — it is called by the framework.
- Never call `Add()` for controls declared in `.stratum` — the generated file handles it.
- Never call `Invalidate()` or `RequestRedraw()` — the reactive system handles it.

---

## 10. Build System Integration

### 10.1 Source Generator

The `.stratum` parser runs as a Roslyn `IIncrementalGenerator`, so generation
happens inside the normal `dotnet build` / `dotnet publish` pipeline. No separate
script is needed. Generated files live in `obj/` and are never committed.

### 10.2 `.csproj` Setup

```xml
<ItemGroup>
  <AdditionalFiles Include="**/*.stratum" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Stratum.Generator\Stratum.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

The `dotnet new stratum-app` template adds these automatically.

### 10.3 Error Reporting

All parse errors are reported as Roslyn diagnostics and appear in build output
and IDE error lists.

**Error format:**
```
Login.stratum(12,3): error STRATUM002: Unknown control type 'Buton'. Did you mean 'Button'?
Login.stratum(18,5): error STRATUM003: Unknown property 'positon' on TextBox. Did you mean 'at'?
Login.stratum(24,1): error STRATUM004: Tab character detected. Use spaces for indentation.
```

**Warning format:**
```
Login.stratum(30,3): warning STRATUM101: Control 'tempLabel' has no 'at' property. Position defaults to 0,0.
```

### 10.4 Diagnostic Code Reference

| Code       | Severity | Description |
|------------|----------|-------------|
| STRATUM001 | Error    | Code-behind class name does not match page identifier |
| STRATUM002 | Error    | Unknown control type (with typo suggestion) |
| STRATUM003 | Error    | Unknown property for control type (with typo suggestion) |
| STRATUM004 | Error    | Tab character in indentation |
| STRATUM005 | Error    | No `page` declaration found |
| STRATUM006 | Error    | Multiple `page` declarations in one file |
| STRATUM007 | Error    | Duplicate control name within page |
| STRATUM008 | Error    | Invalid value format for property type |
| STRATUM009 | Error    | Control declaration at wrong indentation level |
| STRATUM101 | Warning  | Control missing `at` — defaults to 0,0 |
| STRATUM102 | Warning  | Unnamed control generates no field |
| STRATUM103 | Warning  | Property set to its default value — has no effect |

---

## 11. Parser Implementation

### 11.1 Requirements

- Hand-written recursive descent parser. No parser generator or third-party library.
- Input: raw text content of a `.stratum` file.
- Output: a `StratumPage` object.
- All errors reported as diagnostics, never as thrown exceptions.
- The parser continues after errors to surface as many issues as possible per build.

### 11.2 Parse Model

```csharp
public class StratumPage
{
    public string Name       { get; set; } = "";
    public int Width         { get; set; } = 800;
    public int Height        { get; set; } = 600;
    public string Background { get; set; } = "background";
    public string Title      { get; set; } = "";
    public List<StratumControl> Controls { get; } = new();
}

public class StratumControl
{
    public string Type     { get; set; } = "";
    public string Name     { get; set; } = "";  // empty if unnamed
    public int    Line     { get; set; }
    public Dictionary<string, string>  Properties { get; } = new();
    public List<StratumColumn>         Columns    { get; } = new();  // DataGrid only
}

public class StratumColumn
{
    public string Header { get; set; } = "";
    public int    Width  { get; set; } = 100;
}
```

### 11.3 Indentation Detection

```csharp
int GetIndentLevel(string line)
{
    int spaces = 0;
    foreach (char c in line)
    {
        if (c == '\t') { ReportError(STRATUM004); return -1; }
        if (c == ' ')  spaces++;
        else           break;
    }
    return spaces / 2;  // 2 spaces per level
}
```

If the number of leading spaces is not a multiple of 2, `STRATUM009` is reported.
