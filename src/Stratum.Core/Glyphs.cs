// Stratum.Core/Glyphs.cs
namespace Stratum.Core;

/// <summary>
/// Material Symbols ligature name catalog.
/// Use these constants with <see cref="Canvas.DrawGlyph"/> to render
/// icons via the "Material Symbols Rounded" font loaded in the host HTML.
/// </summary>
/// <remarks>
/// The host page must include the Material Symbols web-font, e.g.:
/// <code>
/// &lt;link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200" /&gt;
/// </code>
/// </remarks>
public static class Glyphs
{
    // Navigation &amp; window chrome
    public const string Close         = "close";
    public const string Minimize      = "remove";
    public const string Maximize      = "crop_square";
    public const string Restore       = "filter_none";
    public const string Menu          = "menu";
    public const string Back          = "arrow_back";
    public const string Forward       = "arrow_forward";
    public const string Up            = "arrow_upward";
    public const string Down          = "arrow_downward";
    public const string MoreVert      = "more_vert";
    public const string MoreHoriz     = "more_horiz";

    // Common actions
    public const string Search        = "search";
    public const string Settings      = "settings";
    public const string Add           = "add";
    public const string Remove        = "remove";
    public const string Edit          = "edit";
    public const string Delete        = "delete";
    public const string Save          = "save";
    public const string Copy          = "content_copy";
    public const string Paste         = "content_paste";
    public const string Undo          = "undo";
    public const string Redo          = "redo";
    public const string Refresh       = "refresh";
    public const string Done          = "done";
    public const string Check         = "check";
    public const string Clear         = "clear";

    // File &amp; folder
    public const string Folder        = "folder";
    public const string FolderOpen    = "folder_open";
    public const string File          = "description";
    public const string Image         = "image";
    public const string Audio         = "audio_file";
    public const string Video         = "video_file";
    public const string Download      = "download";
    public const string Upload        = "upload";

    // Apps &amp; system
    public const string Apps          = "apps";
    public const string Home          = "home";
    public const string Dashboard     = "dashboard";
    public const string Terminal      = "terminal";
    public const string Code          = "code";
    public const string Browser       = "public";
    public const string Mail          = "mail";
    public const string Calendar      = "calendar_today";
    public const string Clock         = "schedule";
    public const string Notifications = "notifications";
    public const string Person        = "person";
    public const string Group         = "group";
    public const string Star          = "star";
    public const string Bookmark      = "bookmark";
    public const string Share         = "share";
    public const string Link          = "link";
    public const string Lock          = "lock";
    public const string Unlock        = "lock_open";
    public const string Visibility    = "visibility";
    public const string VisibilityOff = "visibility_off";
    public const string Info          = "info";
    public const string Warning       = "warning";
    public const string Error         = "error";
    public const string Help          = "help";
    public const string LightMode     = "light_mode";
    public const string DarkMode      = "dark_mode";
    public const string Wifi          = "wifi";
    public const string Bluetooth     = "bluetooth";
    public const string Battery       = "battery_full";
    public const string Volume        = "volume_up";
    public const string VolumeMute    = "volume_off";
    public const string Power         = "power_settings_new";
}
