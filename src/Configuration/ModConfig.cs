using BepInEx.Configuration;

namespace WorldSaveMuzzler.Configuration
{
    public enum SaveNoticeMode
    {
        RelocateToTopLeft,
        Mute,
        Native
    }

    internal sealed class ModConfig(ConfigFile config)
    {
        public static ModConfig Instance { get; private set; } = null!;

        public ConfigEntry<SaveNoticeMode> NoticeMode { get; } = config.Bind(
            "1 - General",
            "WorldSaveNoticeMode",
            SaveNoticeMode.RelocateToTopLeft,
            "Sets how world save countdown warnings and completion notices are displayed (RelocateToTopLeft = move center countdowns to top-left feed, Mute = hide both warnings and completion notices, Native = keep base game display).");

        public static void Initialize(ConfigFile configFile) => Instance = new ModConfig(configFile);
    }
}
