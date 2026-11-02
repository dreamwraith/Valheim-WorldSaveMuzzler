using BepInEx.Configuration;

namespace WorldSaveMuzzler.Configuration
{
    public enum SaveNoticeMode
    {
        RelocateToTopLeft,
        Mute,
        Native
    }

    internal static class ModConfig
    {
        public static ConfigEntry<SaveNoticeMode> NoticeMode = null!;

        public static void Initialize(ConfigFile config)
        {
            NoticeMode = config.Bind(
                "1 - General",
                "WorldSaveNoticeMode",
                SaveNoticeMode.RelocateToTopLeft,
                new ConfigDescription(
                    "Controls how autosave countdown warnings and completion notifications are displayed.\n" +
                    "RelocateToTopLeft moves them to the subtle top-left corner feed (default).\n" +
                    "Mute hides them completely.\n" +
                    "Native leaves them centered across the screen.")
            );
        }
    }
}
