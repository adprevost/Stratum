// Stratum.Core/Theme.cs
namespace Stratum.Core;

public class SoundProfile
{
    public bool   Enabled      { get; set; } = true;
    public double MasterVolume { get; set; } = 0.35;
    public string Click        { get; set; } = "click";
    public string Toggle       { get; set; } = "toggle";
    public string ToastInfo    { get; set; } = "chime";
    public string ToastSuccess { get; set; } = "success";
    public string ToastWarning { get; set; } = "warning";
    public string ToastError   { get; set; } = "error";

    public static SoundProfile Default => new();
    public static SoundProfile Silent  => new() { Enabled = false };
}

public static class Theme
{
    // Colors
    public static string PrimaryColor     = "#2563eb";
    public static string PrimaryHover     = "#1d4ed8";
    public static string SecondaryColor   = "#6b7280";
    public static string SecondaryHover   = "#4b5563";
    public static string BackgroundColor  = "#f9fafb";
    public static string SurfaceColor     = "#ffffff";
    public static string BorderColor      = "#d1d5db";
    public static string TextColor        = "#111827";
    public static string TextMuted        = "#6b7280";
    public static string TextOnPrimary    = "#ffffff";
    public static string FocusRing        = "#93c5fd";
    public static string ErrorColor       = "#dc2626";
    public static string SuccessColor     = "#16a34a";
    public static string SelectionColor   = "#bfdbfe";

    // Typography
    public static string FontFamily       = "system-ui, -apple-system, sans-serif";
    public static int    FontSizeBase     = 14;
    public static int    FontSizeSm       = 12;
    public static int    FontSizeLg       = 16;
    public static int    FontSizeXl       = 20;
    public static int    FontSizeXxl      = 28;

    // Spacing
    public static int    BorderRadius     = 6;
    public static int    BorderRadiusSm   = 4;
    public static int    Padding          = 8;
    public static int    PaddingLg        = 16;

    // Helpers
    public static string Font(int size = 0, bool bold = false, bool italic = false)
    {
        int sz = size > 0 ? size : FontSizeBase;
        string weight = bold ? "bold " : "";
        string style  = italic ? "italic " : "";
        return $"{style}{weight}{sz}px {FontFamily}";
    }

    // Sound
    public static SoundProfile Sounds { get; set; } = SoundProfile.Default;
}
