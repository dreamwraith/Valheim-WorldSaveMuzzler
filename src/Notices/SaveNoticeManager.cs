using System;
using WorldSaveMuzzler.Configuration;

namespace WorldSaveMuzzler.Notices
{
    internal static class SaveNoticeManager
    {
        public static bool HandleShowMessage(ref MessageHud.MessageType type, ref string text)
        {
            if (ModConfig.NoticeMode.Value == SaveNoticeMode.Native)
            {
                return true;
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
            // Raw localization keys used by native Game.UpdateSaving and ZNet.PrintWorldSaveMessage
            if (text.Contains("$msg_worldsave"))
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
    }
}
