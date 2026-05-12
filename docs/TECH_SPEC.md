# Stratum — Technical Specification

Architecture reference for the v1 Stratum runtime: WASM bootstrap, JS interop
surface, control model, render loop, and reactivity system.

---

## 1. Project Goals

Stratum renders interactive web applications entirely to an HTML `<canvas>` element
using compiled C# targeting WebAssembly. There is no DOM manipulation beyond the
single canvas. No CSS. No JavaScript written by application developers.

**v1 scope:**
- Working runtime compiled to WASM.
- Core control set: Button, Label, TextBox, CheckBox, Panel, DataGrid, FlowPanel,
  ProgressBar, Tabs, Modal, SidebarNav, Toast, ToggleSwitch.
- Absolute positioning plus one auto-layout container (`FlowPanel`).
- Mouse event handling (click, hover, down, up, move).
- Keyboard handling for TextBox (characters, backspace, delete, arrows).
- A theme system with overridable defaults.
- A text-based `.stratum` DSL compiled by a Roslyn source generator.
- Build scripts that produce a deployable static artifact.
- Three working sample applications.
- A minimal HTML loader the developer never edits.

**Explicitly out of scope for v1:**
- Accessibility (ARIA) overlays.
- WebGPU backend.
- Touch / mobile gesture handling.
- Visual designer tooling.
- CSS-like stylesheets.
- SEO / crawlability.

---

## 2. Repository Structure

```
/Stratum
  /src
    /Stratum.Core          # Base: Control, Canvas, Application, Page, Theme, events
    /Stratum.Controls      # All controls
    /Stratum.DSL           # Runtime text DSL parser
    /Stratum.Generator     # Roslyn incremental source generator
    /Stratum.Runtime       # [JSExport] InputBridge; ships loader JS as NuGet content
  /samples
    /Counter               # Minimal: label + buttons, click increments counter
    /TodoList              # Realistic: text input, dynamic list, checkboxes
    /StratumDemo           # Full canvas-rendered demo application
  /loader
    Stratum.html           # HTML loader template (APP_NAME placeholder)
    Stratum.js             # Canvas drawing module ([JSImport] target)
    main.js                # dotnet bootstrap + input routing
  /build
    build.ps1              # Windows publish script
    build.sh               # Linux/macOS publish script
  /template
    /Stratum.Templates     # dotnet new stratum-app template
  /tests
    /Stratum.Tests         # Console-based DSL test runner
```

---

## 3. Build Target and Toolchain

Stratum uses the `Microsoft.NET.Sdk.WebAssembly` SDK (`wasmbrowser` project type).
This gives access to `[JSImport]`/`[JSExport]` with no Blazor surface area.

**Required toolchain:**
- .NET 10 SDK
- `wasm-tools` workload: `dotnet workload install wasm-tools`
- `wasm-experimental` workload: `dotnet workload install wasm-experimental`
- No Node.js, no npm, no webpack

**Build output (after `build.ps1` or `build.sh`):**
```
/dist/<SampleName>/
  app.wasm
  dotnet.js
  index.html
  Stratum.js
  main.js
```

**Common commands:**
```pwsh
dotnet build Stratum.slnx
dotnet run --project tests/Stratum.Tests
./build/build.ps1 Counter
dotnet serve -d dist/Counter -p 8080
```

---

## 4. JavaScript Layer

### 4.1 `main.js` — Bootstrap and input routing

Initialises the .NET WASM runtime and wires browser input events to `[JSExport]`
methods:
- Calls `dotnet.create()` and `dotnet.run()`.
- Wires `mousemove`, `mousedown`, `mouseup`, `wheel` on the canvas element.
- Wires `keydown`, `keypress` on `window`.
- Calls `InputBridge.OnResize` on load and on `window.resize`.

### 4.2 `Stratum.js` — Canvas drawing module

Canvas 2D API bridge registered as `"Stratum.js"` via `setModuleImports`.

Exposed functions: `clearRect`, `fillRect`, `strokeRect`, `fillText`, `measureText`,
`beginPath`, `closePath`, `moveTo`, `lineTo`, `arc`, `roundRect`, `fill`, `stroke`,
`save`, `restore`, `setClip`, `setFillStyle`, `setStrokeStyle`, `setLineWidth`,
`setFont`, `setTextBaseline`, `setGlobalAlpha`, `getCanvasWidth`, `getCanvasHeight`,
`requestFrame`.

---

## 5. HTML Loader

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>{{APP_NAME}}</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    html, body { width: 100%; height: 100%; overflow: hidden; background: #f8f9fa; }
    #appCanvas { display: block; width: 100%; height: 100%; }
  </style>
</head>
<body>
  <canvas id="appCanvas" tabindex="0"></canvas>
  <script type="module" src="./main.js"></script>
</body>
</html>
```

The build script substitutes `{{APP_NAME}}` with the project name.

---

## 6. Core API

### 6.1 `JsCanvas`

`Stratum.Core.JsCanvas` is a static partial class of `[JSImport]` declarations,
one per canvas 2D function. Controls call `Canvas` methods, not `JsCanvas` directly.

### 6.2 `Canvas`

Thin C#-friendly facade over `JsCanvas`. Passed to every `OnPaint(Canvas canvas)`
call. Key methods: `FillRect`, `FillText`, `SetFillStyle`, `SetFont`,
`DrawRoundedRect`, `BeginPath`, `Arc`, `Save`, `Restore`, `SetClip`, etc.

### 6.3 `Theme`

Static class of overridable color, typography, and spacing constants read by all
controls.

```csharp
Theme.PrimaryColor    = "#0078d4";
Theme.BackgroundColor = "#1e1e1e";
Theme.TextColor       = "#ffffff";
```

Key members: `PrimaryColor`, `PrimaryHover`, `SecondaryColor`, `BackgroundColor`,
`SurfaceColor`, `BorderColor`, `TextColor`, `TextMuted`, `TextOnPrimary`,
`FocusRing`, `ErrorColor`, `SuccessColor`, `SelectionColor`, `FontFamily`,
`FontSizeBase/Sm/Lg/Xl`, `BorderRadius`, `BorderRadiusSm`, `Padding`.

### 6.4 Events

```csharp
public class MouseEventArgs { public int X, Y, Button; }
public class KeyEventArgs   { public string Key, Code; public bool Ctrl, Shift, Alt; }
public enum MouseButton { Left = 0, Middle = 1, Right = 2 }
```

### 6.5 `Control` base class

```
Properties:  X, Y, Width, Height, Visible, Enabled, Focused, Hovered, Name, Tag
Hierarchy:   Parent, Children, Add(child), Remove(child)
Position:    AbsoluteX, AbsoluteY (accumulated from parent chain)
Overrides:   OnPaint(Canvas), OnMouseDown/Up/Move/Click, OnKeyDown, OnKeyPress,
             OnFocus, OnBlur, OnResize
Hit-test:    HitTest(x, y) returns deepest visible+enabled child, or self, or null
Invalidate:  protected Invalidate() marks the application dirty for next frame
```

All interactive properties on concrete controls call `Invalidate()` automatically
when the value changes. The developer never calls it directly.

### 6.6 `Application`

Entry point base class for code-only apps (no `.stratum` file).

```csharp
public class MyApp : Application
{
    protected override void OnStart()
    {
        var label = new Label("Hello", 40, 40, 300, 28);
        var btn   = new Button("Click", 40, 80);
        btn.Click += () => label.Text = "Clicked!";
        Add(label);
        Add(btn);
    }
}
```

`Run()` sets `Application.Current`, calls `OnStart()`, and starts the rAF loop.

### 6.7 `Page`

Base class for apps using `.stratum` files. The source generator emits a partial
class that overrides `InitializeControls()`. Override `OnPageStart()` for logic.

```csharp
// generated (Counter.stratum.g.cs):
protected override void InitializeControls()
{
    countLabel = new Label("Count: 0", 40, 100, 200, 28);
    incBtn     = new Button("Increment", 40, 150, 120, 36);
    Add(countLabel); Add(incBtn);
}

// your code-behind (Counter.cs):
protected override void OnPageStart()
{
    incBtn.Click += () => { _count++; countLabel.Text = $"Count: {_count}"; };
}
```

### 6.8 `InputBridge`

`[JSExport]` methods called by `main.js` that forward events to
`Application.Current.Dispatch*`:

```csharp
[JSExport] public static void OnMouseMove(int x, int y)
    => Application.Current?.DispatchMouseMove(x, y);

[JSExport] public static void OnKeyDown(string key, string code, bool ctrl, bool shift, bool alt)
    => Application.Current?.DispatchKeyDown(key, code, ctrl, shift, alt);
// OnMouseDown, OnMouseUp, OnKeyPress, OnResize follow the same pattern
```

---

## 7. Controls

All controls extend `Control`. The required override is `OnPaint(Canvas canvas)`.

| Control        | Notable properties / events |
|----------------|-----------------------------|
| `Label`        | `Text`, `FontSize`, `Bold`, `Color`, `Align` |
| `Button`       | `Text`, `Style` (ButtonStyle enum), `Click` event |
| `TextBox`      | `Text`, `Placeholder`, `Masked`, `TextChanged` event |
| `CheckBox`     | `Text`, `Checked`, `Color`, `CheckedChanged` event |
| `Panel`        | `Background`, `DrawBorder`, `BorderColor`, `Add(child)` |
| `FlowPanel`    | Extends Panel; auto-arranges children left-to-right with `Gap` and wrapping |
| `DataGrid`     | `Columns` (DataGridColumn\<T\>), `Items`, `SelectionChanged` event |
| `ProgressBar`  | `Value` (0–100), `Striped`, `ShowLabel` |
| `Tabs`         | `TabList`, `ActiveTab`, `ActiveIndex`, `TabChanged` event |
| `Modal`        | `Title`, `Message`, `OkOnly()`, `OkCancel()`, `YesNo()`; `Confirmed`/`Cancelled` events |
| `SidebarNav`   | `NavEntries` (groups + items), `ActiveItem`, `NavigationChanged` event |
| `Toast`        | Static `Toast.Show(message)` — slide-in, auto-dismiss |
| `ToggleSwitch` | `IsOn`, `Label`, `Toggled` event |

---

## 8. Render Loop

```
app.Run()
  OnStart() / OnPageStart()
  ScheduleFrame()
    RequestFrame(Frame)
      Frame()
        if !dirty  --> ScheduleFrame() [skip paint]
        if dirty
          ClearCanvas
          foreach root: RenderTree(control, canvas)
            canvas.Save()
            control.OnPaint(canvas)
            foreach child: RenderTree(child, canvas)
            canvas.Restore()
          _dirty = false
          ScheduleFrame()
```

Every `Invalidate()` call sets `_dirty = true`. The loop repaints only when dirty,
keeping the rAF loop battery-friendly.

---

## 9. Input Dispatch

```
Browser event
  main.js listener
    [JSExport] InputBridge method
      Application.DispatchMouseXxx / DispatchKeyXxx
        HitTest(x, y) -> find target control
        Update hover / focus state
        control.OnMouseXxx / OnKeyXxx
        RequestRedraw()
```

Key events route to `_focused` only. `DispatchMouseDown` transfers focus, calling
`OnBlur` on the previous control and `OnFocus` on the new one.

---

## 10. Source Generator

`Stratum.Generator` is a Roslyn `IIncrementalGenerator` targeting `netstandard2.0`.

**Pipeline:**
1. `StratumParser` — parses `.stratum` text into a `StratumPage` model.
2. `PropertyMapper` — maps DSL property strings to C# initialiser expressions.
3. `StratumGenerator` — emits the partial class with `InitializeControls()`.
4. `DiagnosticDescriptors` — all `STRATUM0xx`/`STRATUM1xx` codes reported as
   Roslyn `Diagnostic` objects (appear in build output and IDE error lists).

**`.csproj` requirements:**
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

**Diagnostic codes:**

| Code       | Severity | Trigger |
|------------|----------|---------|
| STRATUM001 | Error    | Code-behind class name does not match page identifier |
| STRATUM002 | Error    | Unknown control type |
| STRATUM003 | Error    | Unknown property for control type |
| STRATUM004 | Error    | Tab character in indentation |
| STRATUM005 | Error    | No `page` declaration found |
| STRATUM006 | Error    | Multiple `page` declarations |
| STRATUM007 | Error    | Duplicate control name |
| STRATUM008 | Error    | Invalid value format for property |
| STRATUM009 | Error    | Wrong indentation level |
| STRATUM101 | Warning  | Control missing `at` — defaults to 0,0 |
| STRATUM102 | Warning  | Unnamed control — no field generated |

---

## 11. NuGet Packages

Built by `nuget/pack.ps1`:

| Package              | Contents |
|----------------------|----------|
| `Stratum.Core`       | Application, Page, Control, Canvas, Theme, JsCanvas |
| `Stratum.Runtime`    | InputBridge; copies loader JS to `wwwroot` as NuGet content |
| `Stratum.Controls`   | All 13 built-in controls |
| `Stratum.DSL`        | Runtime `.stratum` text parser |
| `Stratum.Templates`  | `dotnet new stratum-app` project template |

**Trimmer roots:** `[JSExport]` methods are called by the WASM runtime, not from
C#, so the IL trimmer would strip them by default. Sample projects and the template
include:

```xml
<TrimmerRootAssembly Include="Stratum.Runtime" />
<TrimmerRootAssembly Include="Stratum.Core" />
```

---

## 12. Known Limitations (v1)

- No ARIA / screen-reader support.
- `mouseleave` not synthesised; controls stay hovered when the cursor exits the canvas.
- No scrollable Panel — only DataGrid scrolls.
- No multi-line TextBox.
- TextBox scroll is approximate; very long strings may show minor cursor drift.
- No layout engine beyond FlowPanel; all other positioning is absolute.
- No touch / gesture input.

See [DECISIONS.md](DECISIONS.md) for the full architecture decision log.
