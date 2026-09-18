using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace WorldSaveMuzzler
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(CommunityPatchExtrasGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "dreamwraith.WorldSaveMuzzler";
        public const string ModName = "WorldSaveMuzzler";
        public const string ModVersion = VersionInfo.Version;
        public const string CommunityPatchExtrasGUID = "MidnightsFX.ValheimCommunityPatchExtras";
        private static readonly System.Version MinSupersededExtrasVersion = new System.Version(0, 28, 0);

        internal static Plugin? Instance { get; private set; }
        internal static ManualLogSource Log = null!;
        private Harmony? _harmony;

        public enum SaveNoticeMode
        {
            RelocateToTopLeft,
            Mute,
            Native
        }

        public static ConfigEntry<SaveNoticeMode> NoticeMode = null!;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            if (IsCommunityPatchExtrasSuperseding(out var extrasVersion))
            {
                Log.LogWarning(
                    $"{ModName} v{ModVersion} is self-disabling: Valheim Community Patch Extras v{extrasVersion} " +
                    $"(> {MinSupersededExtrasVersion}) is installed, which natively handles world save notifications.");
                enabled = false;
                return;
            }

            NoticeMode = Config.Bind(
                "1 - General",
                "WorldSaveNoticeMode",
                SaveNoticeMode.RelocateToTopLeft,
                new ConfigDescription(
                    "Controls how autosave countdown warnings and completion notifications are displayed.\n" +
                    "RelocateToTopLeft moves them to the subtle top-left corner feed (default).\n" +
                    "Mute hides them completely.\n" +
                    "Native leaves them centered across the screen.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false })
            );

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");
        }

        private static bool IsCommunityPatchExtrasSuperseding(out System.Version? detectedVersion)
        {
            detectedVersion = null;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos == null)
            {
                return false;
            }

            foreach (var kvp in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (string.Equals(kvp.Key, CommunityPatchExtrasGUID, StringComparison.OrdinalIgnoreCase))
                {
                    detectedVersion = kvp.Value?.Metadata?.Version;
                    if (detectedVersion != null && detectedVersion > MinSupersededExtrasVersion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(MessageHud))]
        internal static class MessageHudPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(MessageHud.ShowMessage))]
            private static bool ShowMessagePrefix(ref MessageHud.MessageType type, ref string text)
            {
                if (NoticeMode == null || NoticeMode.Value == SaveNoticeMode.Native)
                {
                    return true;
                }

                if (type != MessageHud.MessageType.Center || string.IsNullOrEmpty(text))
                {
                    return true;
                }

                if (IsWorldSaveMessage(text))
                {
                    if (NoticeMode.Value == SaveNoticeMode.Mute)
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

    /// <summary>
    /// ConfigurationManager integration attribute definition.
    /// BepInEx ConfigurationManager discovers this class via reflection on ConfigDescription.Tags.
    /// </summary>
    internal class ConfigurationManagerAttributes
    {
        public bool? IsAdvanced { get; set; }
    }
}
