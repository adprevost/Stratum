// Stratum.Core/SoundService.cs
namespace Stratum.Core;

public static class SoundService
{
    public static void Play(string soundId)
    {
        var profile = Theme.Sounds;
        if (!profile.Enabled) return;
        JsAudio.PlayUiSound(soundId, profile.MasterVolume);
    }
}
