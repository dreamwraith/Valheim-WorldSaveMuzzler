using System;
using WorldSaveMuzzler.Configuration;

namespace WorldSaveMuzzler.Core.Notices
{
    internal static class SaveNoticeManager
    {
        private const string WorldSavedKey = "$msg_worldsaved";
        private const string WorldSaveWarningKey = "$msg_worldsavewarning";

        public static bool HandleShowMessage(ref MessageHud.MessageType type, string text)
        {
            SaveNoticeMode noticeMode = ModConfig.Instance.NoticeMode.Value;
            if (noticeMode == SaveNoticeMode.Native || string.IsNullOrEmpty(text))
            {
                return true;
            }

            // i was only capturing centered messages before, so this also catches the save finished notice in the top-left feed
            if (noticeMode == SaveNoticeMode.Mute && HasMessageKey(text, WorldSavedKey))
            {
                return false;
            }

            if (type != MessageHud.MessageType.Center)
            {
                return true;
            }

            if (IsWorldSaveMessage(text))
            {
                if (noticeMode == SaveNoticeMode.Mute)
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

            string warning = Localization.instance.Localize(WorldSaveWarningKey);
            if (!string.IsNullOrEmpty(warning) && text.IndexOf(warning, StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            string saved = Localization.instance.Localize(WorldSavedKey);
            if (!string.IsNullOrEmpty(saved) && text.IndexOf(saved, StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            // server scripts or mods sending plain english countdowns like "world save in 30s"
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
