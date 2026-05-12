# Getting Started with Stratum

This walkthrough takes you from an empty folder to a running Stratum app, and then
to building a real two-file page from scratch. If you only want to run the bundled
samples, see the **Quick start** section of the [README](../README.md).

---

## 1. Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK              | **10.0** or later | <https://dotnet.microsoft.com/download/dotnet/10.0> |
| `wasm-tools` workload | latest            | `dotnet workload install wasm-tools` |
| `wasm-experimental`   | latest            | `dotnet workload install wasm-experimental` |
| `dotnet-serve`        | latest            | `dotnet tool install -g dotnet-serve` |

Verify:

```pwsh
dotnet --version          # must print 10.0.x
dotnet workload list      # must include wasm-tools and wasm-experimental
```

---

## 2. Install the project template

Stratum ships a `dotnet new` template called `stratum-app`. You can install it from
the local NuGet packages produced by `nuget/pack.ps1`, or (once published) from
nuget.org directly.

### Option A — from the cloned repo

```pwsh
git clone https://github.com/<your-org>/Stratum
cd Stratum
./nuget/pack.ps1
dotnet nuget add source "$PWD/nuget/artifacts" --name StratumLocal
dotnet new install Stratum.Templates
```

### Option B — from nuget.org (when published)

```pwsh
dotnet new install Stratum.Templates
```

Verify:

```pwsh
dotnet new list stratum
# Stratum App   stratum-app   [C#]   Web/WebAssembly/Canvas/Stratum
```

---

## 3. Create your first app

```pwsh
dotnet new stratum-app -n MyApp
cd MyApp
dotnet publish -o dist
dotnet serve -d dist -p 8080
```

Open <http://localhost:8080>. You will see a "Hello from Stratum!" label and a
button. Click the button — the label updates with a click count.

The template ships a single-file `Application` so you can see the bare minimum
without yet introducing the `.stratum` DSL. The next section converts it to the
two-file model.

---

## 4. Convert to the two-file (`.stratum` + `.cs`) model

This is the recommended way to author every page.

### 4.1 Add the generator + controls package

Edit `MyApp.csproj` and add:

```xml
<ItemGroup>
  <PackageReference Include="Stratum.Controls" Version="1.0.0" />
  <PackageReference Include="Stratum.Generator" Version="1.0.0"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>

<ItemGroup>
  <AdditionalFiles Include="**/*.stratum" />
</ItemGroup>
```

If you cloned the repo and are working inside it, replace the `PackageReference`
lines with `ProjectReference` lines pointing at `src/Stratum.Controls` and
`src/Stratum.Generator`.

### 4.2 Create `Hello.stratum`

```text
page Hello
  size: 480 x 240
  title: "Hello"

  Label greeting
    text: "Hello, Stratum!"
    at: 40, 40
    size: 300 x 32
    font: xl bold

  Button sayHi
    text: "Say hi"
    at: 40, 100
    size: 120 x 36
    style: primary
```

### 4.3 Create `Hello.cs`

```csharp
public partial class Hello
{
    private int _clicks;

    protected override void OnPageStart()
    {
        sayHi.Click += () =>
        {
            _clicks++;
            greeting.Text = $"Hi #{_clicks}";
        };
    }
}
```

### 4.4 Update `Program.cs`

```csharp
using Stratum.Core;

Hello page = new();
page.Run();

while (true) await Task.Delay(1000);
```

Delete the old `MyApp.cs` (or keep it as a reference). Then:

```pwsh
dotnet publish -o dist
dotnet serve -d dist -p 8080
```

That's the full author loop. The control fields (`greeting`, `sayHi`) come from
the source generator — you never declare them by hand. Assigning to `greeting.Text`
automatically marks the page dirty, and the next animation frame repaints it.

---

## 5. Where to go next

- **[`STRATUM_FORMAT.md`](STRATUM_FORMAT.md)** — every supported control, every
  property, every value type the parser accepts.
- **[`TECH_SPEC.md`](TECH_SPEC.md)** — the full architecture: WASM bootstrap, JS
  interop, render loop, reactivity.
- **[`WHY_THIS_MATTERS.md`](WHY_THIS_MATTERS.md)** — the longer argument for why
  Stratum exists.
- **[`DECISIONS.md`](DECISIONS.md)** — design decisions and trade-offs, kept as a
  running log.
- **The `samples/` folder** — `Counter` (minimal), `TodoList` (dynamic list),
  `StratumDemo` (full canvas-rendered desktop).

---

## 6. Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| `error : Workload ID 'wasm-tools' is not recognized.` | .NET SDK older than 10. | Install .NET 10. |
| Browser shows a blank canvas. | The trimmer stripped `[JSExport]` methods. | Make sure `Stratum.Runtime` and `Stratum.Core` are listed in `<TrimmerRootAssembly>`. The template csproj already does this. |
| `dotnet new stratum-app` not found. | Template package not installed. | Run `dotnet new install Stratum.Templates`. |
| `dotnet serve` not found. | Tool not installed globally. | `dotnet tool install -g dotnet-serve`, then ensure `~/.dotnet/tools` is on `PATH`. |
| `dotnet publish` succeeds but `dist/` looks wrong. | The WASM SDK puts everything under `dist/wwwroot/`. | Use `build/build.ps1` (it flattens), or serve `dist/wwwroot` directly. |
| The page renders but clicks do nothing. | You wired events outside `OnPageStart()`. | Move all `Click +=` lines into `OnPageStart()`. |

If you hit something not on this list, please file an issue with the exact command,
the error output, and your `dotnet --info`.
