# Contributing to Stratum

Stratum is a young project with a strong opinion: **the web stack is broken, and the
fix is to ignore most of it**. If that resonates with you, welcome.

This file describes how to get a working dev loop, where things live, and what kinds
of changes are likely to be merged.

---

## TL;DR

```bash
git clone https://github.com/<your-fork>/Stratum
cd Stratum
dotnet workload install wasm-tools wasm-experimental
dotnet tool install -g dotnet-serve
dotnet build Stratum.slnx
dotnet run --project tests/Stratum.Tests
./build/build.ps1 Counter        # or ./build/build.sh Counter on Linux/macOS
dotnet serve -d dist/Counter -p 8080
```

Open <http://localhost:8080>. You should see the Counter sample.

---

## Repo layout

| Path | What lives there |
|------|-----------------|
| `src/Stratum.Core/`      | Base types: `Application`, `Page`, `Control`, `Canvas`, `Theme`, `[JSImport]` surface |
| `src/Stratum.Runtime/`   | `[JSExport]` input bridge; loader JS shipped as NuGet content |
| `src/Stratum.Controls/`  | Built-in controls (Button, Label, TextBox, CheckBox, Panel, FlowPanel, DataGrid, ProgressBar, Tabs, Modal, SidebarNav, Toast) |
| `src/Stratum.DSL/`       | Runtime parser for the `.stratum` text DSL |
| `src/Stratum.Generator/` | Roslyn incremental source generator that turns `.stratum` files into partial classes |
| `samples/`               | Working sample apps (`Counter`, `TodoList`, `StratumDemo`) |
| `tests/Stratum.Tests/`   | Plain console test runner (no xUnit — see [`docs/DECISIONS.md`](docs/DECISIONS.md)) |
| `loader/`                | `index.html` + JS glue copied into every sample's `wwwroot` |
| `template/`              | `dotnet new stratum-app` template package |
| `build/`                 | `build.ps1` / `build.sh` — publish a sample and flatten the `wwwroot` |
| `nuget/`                 | `pack.ps1` — produce all five NuGet packages locally |
| `docs/`                  | All long-form docs (manifesto, tech spec, format reference, getting started) |

---

## Coding conventions

- **C# language version:** `latest` (set in `Directory.Build.props`).
- **Nullable reference types:** enabled.
- **Indentation:** spaces only, 2 per level (in both `.cs` and `.stratum`).
- **Naming:** PascalCase for types/methods, camelCase for fields and `.stratum` control names.
- **No new layers without a reason.** Stratum is allergic to abstractions that exist "just in case".
- **Keep `Stratum.Core` dependency-free** apart from the BCL. Other projects depend on Core, never the reverse.
- **No new top-level files** unless they are the README, LICENSE, or `CONTRIBUTING.md`. Everything else goes under `src/`, `samples/`, `tests/`, `docs/`, or `build/`.

---

## Adding a control

1. Add `MyControl.cs` to `src/Stratum.Controls/` deriving from `Control`.
2. Implement `Paint(Canvas g)` and any input overrides you need.
3. Register the control + its supported `.stratum` properties in
   `src/Stratum.Generator/StratumParser.cs` (`KnownControls`, `KnownProperties`).
4. Map any new property names in `src/Stratum.Generator/PropertyMapper.cs`.
5. Add a parser test in `tests/Stratum.Tests/Program.cs`.
6. Document the control in `docs/STRATUM_FORMAT.md`.

---

## Pull requests

- Keep PRs small and focused. One control, one bug fix, one doc rewrite.
- Run `dotnet build Stratum.slnx` and `dotnet run --project tests/Stratum.Tests`
  before pushing. Both must pass.
- If you change a public API, update `docs/STRATUM_FORMAT.md` or the README in the
  same PR.
- For non-trivial design choices, append a short entry to `docs/DECISIONS.md`.

---

## What is *not* in scope

- A virtual DOM, a diffing renderer, or anything resembling React.
- A CSS-like styling layer. Stratum has a `Theme` class and that is the entire
  styling story.
- A heavy layout engine. We have absolute positioning and `FlowPanel`. That is
  enough for v1.
- Server-side rendering, hydration, or SEO features. Stratum is for applications,
  not documents.

If you want to build any of those things on top of Stratum, please do — but in a
separate package.

---

## Filing issues

Useful issues include:
- A minimal reproducible sample (a `.stratum` file + the `.cs` code-behind, please).
- Browser + OS + .NET SDK version.
- The exact command you ran and what you expected vs what happened.

Thanks for helping push back on three decades of accumulated web cruft.
