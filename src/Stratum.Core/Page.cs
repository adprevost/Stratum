// Stratum.Core/Page.cs
namespace Stratum.Core;

/// <summary>
/// Base class for pages declared with the two-file .stratum model.
/// The source generator emits a partial class that overrides InitializeControls().
/// Page.Run calls InitializeControls() then OnStart() so all control fields
/// are non-null when the code-behind's OnStart runs.
/// </summary>
public abstract class Page : Application
{
    // Application.OnStart is virtual so we can seal it here and call
    // InitializeControls first, then delegate to the code-behind override.
    protected sealed override void OnStart()
    {
        InitializeControls();
        OnPageStart();
    }

    /// <summary>Called by the generated partial class to instantiate and register controls.</summary>
    protected virtual void InitializeControls() { }

    /// <summary>
    /// Override in the code-behind partial class to wire events and set initial state.
    /// Renamed from OnStart so the sealed override on Page can chain correctly.
    /// Spec note: the code-behind uses "protected override void OnStart()" per STRATUM_UI_FORMAT.md
    /// §9.2 — we expose it here as OnPageStart and route the call.
    /// Decision recorded in DECISIONS.md.
    /// </summary>
    protected virtual void OnPageStart() { }
}
