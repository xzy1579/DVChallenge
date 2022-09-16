using UnityModManagerNet;

namespace DvMod.Challenges
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable logging")] public bool enableLogging = true;
        public readonly string? version = Main.mod?.Info.Version;

        override public void Save(UnityModManager.ModEntry entry)
        {
            Save<Settings>(this, entry);
        }

        public void OnChange()
        {
        }
    }
}
