// Stratum.Core/JsAudio.cs
using System.Runtime.InteropServices.JavaScript;

namespace Stratum.Core;

public static partial class JsAudio
{
    [JSImport("playUiSound", "Stratum.js")]
    public static partial void PlayUiSound(string soundId, double volume);
}
