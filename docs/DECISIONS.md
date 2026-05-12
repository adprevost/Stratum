# Architecture Decisions

A running record of significant design choices made during the Stratum prototype.

---

## D-001 · Single `<canvas>` instead of DOM components

**Context:** Browsers provide both a retained-mode DOM widget system and an immediate-mode canvas API.

**Decision:** All UI is painted onto one `<canvas id="appCanvas">`. No DOM widgets are created at runtime.

**Rationale:** Eliminates layout/styling conflicts with the host page, removes the WebAssembly↔DOM GC boundary overhead for per-element updates, and makes the rendering model fully deterministic and cross-platform (same paint output on every browser and in future native back-ends).

**Trade-offs:** Screen-reader accessibility requires a custom ARIA live-region strategy (not yet implemented). Existing HTML/CSS idioms do not apply.

---

## D-002 · `[JSImport]` / `[JSExport]` over `IJSRuntime`

**Context:** Blazor exposes `IJSRuntime.InvokeAsync<T>` for JS interop. The wasmbrowser template provides the lower-level `[JSImport]`/`[JSExport]` attributes.

**Decision:** Stratum uses `[JSImport]` in `Stratum.Core.JsCanvas` and `[JSExport]` in `Stratum.Runtime.InputBridge`. No Blazor dependency.

**Rationale:** `[JSImport]`/`[JSExport]` is AOT-friendly, zero-allocation at call sites, and does not require the Blazor hosting model.  Removes ~3 MB of unused Blazor framework DLLs from published output.

---

## D-003 · `JsCanvas` lives in `Stratum.Core`, not `Stratum.Runtime`

**Context:** The initial design placed JS interop in `Stratum.Runtime`. `Stratum.Core` needed to call canvas APIs, so it would have had to reference Runtime — creating a circular dependency.

**Decision:** `JsCanvas` (the `[JSImport]` surface) was moved to `Stratum.Core`. `Stratum.Runtime` remains the thin `[JSExport]` bridge that routes browser events into the application.

**Rationale:** Breaks the circular reference. Core remains the single dependency that all other layers point toward.

---

## D-004 · Immediate-mode render loop via `requestAnimationFrame`

**Context:** Options considered: a fixed `Task.Delay` timer, a reactive/diffing virtual tree, or rAF-based dirty-flag redraw.

**Decision:** `Application.RequestRedraw()` sets a `_dirty` flag. `JsCanvas.RequestFrame` schedules a `requestAnimationFrame` callback; `Application.Frame()` paints the full control tree only when dirty.

**Rationale:** Native rAF scheduling is the lowest-latency, battery-friendly approach on all browsers. The dirty flag prevents redundant full repaints.

---

## D-005 · Console-based test runner instead of xUnit

**Context:** `[JSImport]`/`[JSExport]` attributes fail at runtime outside a WASM host. Using xUnit for the DSL layer would require a browser runner or mocking the JS interop.

**Decision:** `tests/Stratum.Tests` is a plain `net10.0` console app that exercises only the DSL parser and control construction (no JS calls). xUnit is not used.

**Rationale:** Simple, zero extra NuGet dependencies, fast CI execution (`dotnet run`). JS interop paths are integration-tested manually via the browser samples.

---

## D-006 · DSL identifiers are letter/underscore-only

**Context:** The DSL `Size` attribute uses `WxH` syntax (e.g., `200x80`). The tokenizer initially treated digits as valid identifier characters, which caused `200x80` to tokenize as a single identifier.

**Decision:** `ReadIdentifier()` in the tokenizer only consumes `[a-zA-Z_][a-zA-Z0-9_]*`. Dimension literals tokenize as `Number`, `Identifier("x")`, `Number`.

**Rationale:** Consistent with C-family identifier rules; makes dimension parsing unambiguous without a dedicated token kind.

---

## D-007 · Loader JS split into `main.js` + `Stratum.js`

**Context:** A single monolithic JS file would mix bootstrap concerns (dotnet runtime init, event wiring) with canvas drawing. The wasmbrowser `setModuleImports` API allows arbitrary named modules.

**Decision:** `loader/main.js` owns bootstrap and input routing. `loader/Stratum.js` is the canvas drawing module imported under the name `"Stratum.js"` via `setModuleImports`.

**Rationale:** Each file has a single responsibility. `Stratum.js` can be replaced with a native back-end adapter without touching the bootstrap.

---

## D-008 · `.slnx` solution format

**Context:** `dotnet new sln` in .NET 10 generates `Stratum.slnx` instead of `Stratum.sln`.

**Decision:** The repo uses `Stratum.slnx` and all build scripts target it.

**Rationale:** Adopting the SDK-default format avoids conversion friction and is compatible with `dotnet build`, `dotnet publish`, and the Visual Studio 2022 17.10+ solution explorer.

---

## D-009 · `InvariantGlobalization=true` for WASM samples

**Context:** Full ICU data adds ~1 MB to WASM publish output and is not needed for the demo samples.

**Decision:** Both sample `.csproj` files set `<InvariantGlobalization>true</InvariantGlobalization>`.

**Rationale:** Reduces published payload; can be disabled in production apps that need locale-aware string operations.

---

## D-010 · `RunAOTCompilation=false` for samples

**Context:** AOT compilation significantly increases publish time during development (~5–15 min).

**Decision:** AOT is disabled for sample projects. Enabling it is documented as an optional release-build step.

**Rationale:** Fast iteration during development; AOT can be enabled by adding `<RunAOTCompilation>true</RunAOTCompilation>` in a `<PropertyGroup Condition="'$(Configuration)'=='Release'">` block.

---

## D-011 · `Page` base class and `OnPageStart()` instead of `OnStart()` in code-behind

**Context:** STRATUM_UI_FORMAT.md §9.2 says the code-behind overrides `OnStart()`. `Application.OnStart()` was `abstract`. `Page` needs to intercept `OnStart()` to call `InitializeControls()` first, then delegate to the developer's override. Two methods on the same class cannot both be named `OnStart()` in one inheritance chain.

**Decision:** `Application.OnStart()` is changed from `abstract` to `virtual`. `Page` seals it, calls `InitializeControls()`, then calls `virtual void OnPageStart()`. Code-behind partial classes override `OnPageStart()` instead of `OnStart()`. The samples and generator use `OnPageStart()`. This is the simplest working solution; renaming from `OnStart` to `OnPageStart` is a one-line change per page.

**Rationale:** C# single-inheritance rules make two virtual `OnStart()` methods impossible in the same chain. `OnPageStart()` is the minimal rename that satisfies the constraint.

---

## D-012 · Source generator targets `netstandard2.0`; Roslyn IIncrementalGenerator

**Context:** Roslyn source generators must target `netstandard2.0`. The spec says to use `ISourceGenerator` or `IIncrementalGenerator`.

**Decision:** `IIncrementalGenerator` is used; it is the current recommended API and provides better IDE performance via caching.

**Rationale:** `IIncrementalGenerator` is the future-proof choice per Microsoft guidance.

---

## D-013 · Old single-file sample classes (`CounterApp`, `TodoApp`) retained

**Context:** The spec says to rewrite samples. The old files (`CounterApp.cs`, `TodoApp.cs`) are left in the repo but are no longer referenced from `Program.cs`. They do not break the build.

**Decision:** Old files are left as-is (they still compile). New two-file model files (`Counter.stratum` + `Counter.cs`, `TodoList.stratum` + `TodoList.cs`) are the active entry points.

**Rationale:** Non-breaking; provides a before/after comparison. Can be deleted once the new model is verified in CI.


---

## StratumDemo new controls

**Decision:** Add four new controls — ProgressBar, Tabs, Modal, SidebarNav — plus a runtime-only Toast notifier, all painted on canvas with reactive properties.

**Rationale:** The StratumDemo sample needs feedback, navigation, and overlay primitives to showcase every interaction pattern without pulling in DOM widgets or extra dependencies.

### ProgressBar
- Single `Value` (0..100) drives a rounded fill over a rounded track.
- Striped variant overlays a 45-degree semi-transparent diagonal pattern clipped to the fill, preserving track edges.
- Optional centered `"{Value:0}%"` label uses contrasting color so it stays readable on both filled and unfilled regions.

### Tabs
- `Tab:` lines in the .stratum file are collected in order into `TabList` (a `List<string>`) — same ordered-child convention as DataGrid `Column:`.
- Active tab is tracked by both `ActiveTab` (string) and `ActiveIndex` (int) so code-behind can drive content visibility either way.
- Control paints headers and a divider only — content visibility is the developer's job, matching the spec.

### Modal
- New abstraction `ModalOverlay` in Stratum.Core lets `Application` render modals after all controls and block input behind them.
- Modal is *not* positioned via `at:` in the .stratum file; the generator emits the no-arg constructor and the runtime always centers the dialog on canvas.
- Action buttons are configured exclusively from code-behind via `OkOnly()`, `OkCancel()`, `YesNo()`, `Custom(...)` so the .stratum file stays free of event wiring.
- Escape key dismisses with `Cancelled` to match standard dialog UX.

### SidebarNav
- `Group:` and `NavItem:` lines are collected as ordered `NavEntries` (kind + label), preserving DSL order.
- Generator defaults `ActiveItem` to the first `NavItem` so the demo shows a highlighted entry on first paint without extra code.
- Active state uses a 4 px accent bar plus tinted background to read clearly even without icons.

### Toast (runtime-only)
- New abstraction `ToastHostBase` in Stratum.Core; `ToastManager` is registered with `Application` and paints after controls but before modals.
- `Toast.Show(...)` is a static facade that forwards to the active manager so any code-behind can fire toasts without a field.
- Slide-in (180 ms) and fade-out (180 ms) animations are implemented via a `Scheduler` time loop — the only place in v1 that uses time-based rendering, per spec.

### Generator / parser
- `StratumParser.KnownControls` and `KnownProperties` extended for the four new controls.
- `ParseModel` carries ordered `Tabs` and `NavEntries` collections in addition to `Columns`.
- `StratumGenerator` emits `RegisterModal(...)` for `Modal` and `Add(...)` for everything else, and unconditionally registers a default `ToastManager` if none is present.

## StratumDemo sample

**Decision:** A single persistent `Shell` page with seven section panels toggled by `Visible`, driven by `SidebarNav.NavigationChanged`.

**Rationale:** Avoids any page navigation cost and proves that the reactive property system can drive a real multi-section app with no manual `RequestRedraw()` calls in section code.
