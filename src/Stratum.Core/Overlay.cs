// Stratum.Core/Overlay.cs
namespace Stratum.Core;

/// <summary>Marker base for controls that act as modal overlays. Painted above all roots; consumes input.</summary>
public abstract class ModalOverlay : Control
{
    public abstract bool IsActive { get; }
}

/// <summary>Marker base for the toast manager. Painted above modals.</summary>
public abstract class ToastHostBase : Control
{
    public abstract bool HasActive { get; }
}
