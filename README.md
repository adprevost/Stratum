# Stratum

> **Build web applications in pure C#. No HTML. No CSS. No JavaScript framework.
> No bundler. No transpiler. No `node_modules`.**
>
> Just a canvas, your code, and the browser.

Stratum compiles C# to WebAssembly and renders every pixel of your app onto a single
HTML `<canvas>` — bypassing the DOM entirely. It is shamelessly modeled after the
RAD tools that made desktop development *fun* in the 90s: Visual Basic, Delphi,
early WinForms.

If you have ever looked at a "modern" web project — `package.json`, `webpack.config.js`,
`tailwind.config.js`, `tsconfig.json`, `vite.config.ts`, `eslintrc`, `prettierrc`,
a `src/` folder full of `.tsx`, a `pages/` folder full of `.tsx`, a `components/`
folder full of `.tsx`, and *one* `index.html` that's mostly empty — and quietly
wondered *"how did we end up here?"* — Stratum is for you.

**Stratum does for modern web UI what WinForms did for desktop in the late 90s.**
It throws out three decades of accumulated patchwork (jQuery → Backbone → Angular →
React → Next → Remix → ...) and asks a simpler question: *what if your UI was just
code?*

---

<video src="https://github.com/user-attachments/assets/0eb2ec40-4c81-4efc-9da7-b9a7d47053af" autoplay loop muted playsinline width="100%"></video>

---

## Why Stratum?

| The web circa 2025                                | Stratum                                  |
|---------------------------------------------------|------------------------------------------|
| HTML for structure                                | One `<canvas>` element                   |
| CSS for styling                                   | A C# `Theme` class                       |
| JavaScript for behavior                           | C# methods                               |
| A framework on top of JS (React, Vue, ...)        | None                                     |
| A bundler on top of the framework (Vite, Webpack) | None                                     |
| A package manager on top of the bundler (npm)     | NuGet, like every other .NET project     |
| Five languages, four config files, one app        | One language, one project, one app       |

Stratum is built for **applications**, not documents. Internal tools, dashboards,
data-entry forms, line-of-business apps, developer tooling — anywhere the people
using the app are known and logic matters more than SEO.

For the longer argument, see [`docs/WHY_THIS_MATTERS.md`](docs/WHY_THIS_MATTERS.md).

---

## Hello, Stratum

A counter app. Two files. That's the whole program.

**`Counter.stratum`** — what's on the page

```
page Counter
  size: 480 x 300
  title: "Counter"

  Label heading
    text: "Stratum Counter"
    at: 40, 40
    size: 300 x 32
    font: xl bold

  Label countLabel
    text: "Count: 0"
    at: 40, 100
    size: 200 x 28

  Button incBtn
    text: "Increment"
    at: 40, 150
    size: 120 x 36
    style: primary
```

**`Counter.cs`** — what it does

```csharp
public partial class Counter
{
    private int _count;

    protected override void OnPageStart()
    {
        incBtn.Click += () =>
        {
            _count++;
            countLabel.Text = $"Count: {_count}";
        };
    }
}
```

That's it. No `Program.cs` plumbing for the controls. No `useState`. No `Invalidate()`.
The control fields (`incBtn`, `countLabel`, ...) are generated from the `.stratum`
file at build time. Assigning to a property automatically schedules a redraw.

It's the WinForms designer surface, written as text, that an AI can read and write
as easily as a human.

---

## Quick start

### Prerequisites

```pwsh
# .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0
dotnet workload install wasm-tools wasm-experimental
dotnet tool install -g dotnet-serve
```

### Run a sample (3 commands)

```pwsh
git clone https://github.com/<your-org>/Stratum
cd Stratum
./build/build.ps1 Counter        # or ./build/build.sh Counter on Linux/macOS
dotnet serve -d dist/Counter -p 8080
```

Open <http://localhost:8080>. Click the button. The counter goes up.

That is a `.wasm` file rendering a UI to a canvas. No DOM widgets, no CSS, no
hand-written JS. The browser is now a runtime, not a document viewer.

### One-shot bootstrap

Prefer to skip the manual setup? Run the included init script:

```pwsh
./init.ps1 -AppName MyApp     # Windows / pwsh
./init.sh   MyApp             # Linux / macOS
```

It verifies your SDK, installs workloads, packs the local NuGets, registers the
`StratumLocal` feed, installs the `dotnet new stratum-app` template, and (if you
pass an app name) scaffolds a fresh project ready to run.

### Or scaffold a brand-new app by hand

```pwsh
./nuget/pack.ps1
dotnet nuget add source ./nuget/artifacts --name StratumLocal
dotnet new install Stratum.Templates
dotnet new stratum-app -n MyApp
cd MyApp
dotnet publish -o dist
dotnet serve -d dist -p 8080
```

See [`docs/GETTING_STARTED.md`](docs/GETTING_STARTED.md) for the full walkthrough,
including building your first two-file page.

---

## How it works

```
┌─────────────────┐  source generator   ┌─────────────────┐
│  Page.stratum   │ ──────────────────▶ │ Page.stratum.g  │
│  (text DSL)     │                     │ (partial class) │
└─────────────────┘                     └────────┬────────┘
                                                 │ merges with
                                                 ▼
┌─────────────────┐    Roslyn + WASM    ┌─────────────────┐
│  Page.cs        │ ──────────────────▶ │   app.wasm      │
│  (your logic)   │                     │ + tiny JS glue  │
└─────────────────┘                     └────────┬────────┘
                                                 │ runs in
                                                 ▼
                                        ┌─────────────────┐
                                        │ <canvas>        │
                                        │ (the entire UI) │
                                        └─────────────────┘
```

1. **`.stratum` files** are parsed at build time by a Roslyn source generator
   (`Stratum.Generator`) into partial C# classes. You never see the generated code,
   but the control fields it produces show up in IntelliSense the moment you save.
2. **The runtime** (`Stratum.Core` + `Stratum.Runtime`) is compiled to WebAssembly
   via the standard `Microsoft.NET.Sdk.WebAssembly` SDK — no Blazor, no ASP.NET host.
3. **A ~60-line JS shim** wires browser input events into `[JSExport]` C# methods
   and exposes the canvas 2D API to C# via `[JSImport]`. That is the entire
   JavaScript footprint of any Stratum app, and it ships inside the framework.
4. **The render loop** is `requestAnimationFrame`-driven and dirty-flagged. Setting
   a control property marks the page dirty; the next frame paints it.

Full architecture details: [`docs/TECH_SPEC.md`](docs/TECH_SPEC.md).
Full `.stratum` syntax reference: [`docs/STRATUM_FORMAT.md`](docs/STRATUM_FORMAT.md).

---

## What's in the box

**Built-in controls:** `Label`, `Button`, `TextBox`, `CheckBox`, `Panel`,
`FlowPanel`, `DataGrid`, `ProgressBar`, `Tabs`, `Modal`, `SidebarNav`, `Toast`,
`ToggleSwitch`.

**Samples:**
- **`samples/Counter`** — the canonical first app.
- **`samples/TodoList`** — text input, dynamic list, completion state.
- **`samples/StratumDemo`** — a full canvas-rendered "WebOS" desktop with draggable
  glass windows, an animated dock, and three demo apps (Files, Settings, Browser).
  Possibly the only WinForms-style desktop environment ever shipped as a single
  `.wasm`.

---

## Why now: the WebAssembly moment

WebAssembly has been quietly shipping in every browser for years. The web
community has mostly used it as an optimization inside the existing paradigm —
"make React faster", "speed up Figma's renderer". Stratum bets that the more
interesting use of WASM is the obvious one: **stop pretending the browser is a
document viewer.** It's a runtime. Treat it like one.

This is also a moment where AI agents are writing more and more code. The web's
multi-language stack is hostile to LLMs — a button that looks right requires
correct HTML, correct CSS, *and* correct JS, in three different files, all agreeing
with each other. Stratum collapses that into one file in one language, with a text
DSL that reads like Mermaid: dense, unambiguous, and trivial for an AI to generate
correctly the first time.

---

## Roadmap

**v1 (here, now)**
- ✅ WASM runtime, single-canvas render
- ✅ 13 controls, theme, reactive properties
- ✅ `.stratum` DSL + source generator
- ✅ `dotnet new stratum-app` template
- ✅ Three working samples

**v1.x — quality of life**
- Hot reload of `.stratum` files
- A scrollable `Panel`
- Multi-line `TextBox`
- ARIA bridge for screen-reader accessibility
- More samples (charts, forms, settings UI)

**v2 — ambition**
- Visual designer (drag-drop produces `.stratum` text)
- WebGPU back-end (canvas 2D becomes one of several renderers)
- Native back-end (the same C# code, no browser)
- Animation primitives
- Component composition (`.stratum` files that include other `.stratum` files)

**Non-goals, forever:** virtual DOM, CSS-in-C#, server-side rendering, SEO.
Those are different products for different problems.

---

## Limitations (be honest)

- No screen-reader / ARIA support yet. Canvas content is invisible to assistive tech.
- No SEO. Don't build your marketing site in Stratum.
- No browser text selection or find-in-page inside the canvas.
- No layout engine beyond `FlowPanel`. Everything else is absolute positioning.
- v1 has no scrollable container besides `DataGrid`.

If those are dealbreakers for your project, use Blazor or React. If they aren't,
keep reading.

---

## Repository layout

```
Stratum/
├── src/                    # Framework source (Core, Runtime, Controls, DSL, Generator)
├── samples/                # Counter, TodoList, StratumDemo
├── tests/Stratum.Tests/    # Console-based test runner
├── template/               # `dotnet new stratum-app` template
├── loader/                 # Stratum.html + JS glue (copied into every published app)
├── build/                  # build.ps1 / build.sh
├── nuget/                  # pack.ps1 (produces all 5 NuGets locally)
└── docs/                   # WHY, TECH_SPEC, STRATUM_FORMAT, GETTING_STARTED, DECISIONS
```

---

## Contributing

PRs welcome. Read [`CONTRIBUTING.md`](CONTRIBUTING.md) first — it covers the dev
loop, where things live, and what kinds of changes are likely to land.

---

## License

[MIT](LICENSE) — Copyright © 2026 Andrew D. Prevost and contributors.

---

*"You don't have to use the entire stack. You never did."*
