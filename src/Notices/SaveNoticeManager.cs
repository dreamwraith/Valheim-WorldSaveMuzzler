using System;
using WorldSaveMuzzler.Configuration;

namespace WorldSaveMuzzler.Notices
{
    internal static class SaveNoticeManager
    {
        private const string WorldSavedKey = "$msg_worldsaved";
        private const string WorldSaveWarningKey = "$msg_worldsavewarning";

        public static bool HandleShowMessage(ref MessageHud.MessageType type, ref string text)
        {
            if (ModConfig.NoticeMode.Value == SaveNoticeMode.Native)
            {
                return true;
            }

            // i was only capturing centered messages before, so this also catches the save finished notice in the top-left feed
            if (ModConfig.NoticeMode.Value == SaveNoticeMode.Mute &&
                !string.IsNullOrEmpty(text) &&
                HasMessageKey(text, WorldSavedKey))
            {
                return false;
            }

            if (type != MessageHud.MessageType.Center || string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (IsWorldSaveMessage(text))
            {
                if (ModConfig.NoticeMode.Value == SaveNoticeMode.Mute)
                {
                    return false;
                }

                type = MessageHud.MessageType.TopLeft;
            }

            return true;
        }

        private static bool IsWorldSaveMessage(string text)
        {
            // i was clobbering server low-disk alerts and other valuable messages here
            // moved to more deterministic check to avoid clobbering and still silence messages
            if (HasMessageKey(text, WorldSaveWarningKey) || HasMessageKey(text, WorldSavedKey))
            {
                return true;
            }

            // Localized text checks
            if (Localization.instance != null)
            {
                string warning = Localization.instance.Localize("$msg_worldsavewarning");
                if (!string.IsNullOrEmpty(warning) && text.Contains(warning))
                {
                    return true;
                }

                string saved = Localization.instance.Localize("$msg_worldsaved");
                if (!string.IsNullOrEmpty(saved) && text.Contains(saved))
                {
                    return true;
                }
            }

            // Dedicated server scripts or mods broadcasting plain English countdowns (e.g. "World save in 30s")
            if (text.IndexOf("world save", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("saving world", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool HasMessageKey(string text, string key) =>
            text.StartsWith(key, StringComparison.Ordinal) &&
            (text.Length == key.Length || text[key.Length] == ' ');
    }
}
