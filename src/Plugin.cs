using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using WorldSaveMuzzler.Configuration;

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

        internal static Plugin? Instance { get; private set; }
        internal static ManualLogSource Log = null!;
        private Harmony? _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            if (Chainloader.PluginInfos.ContainsKey(CommunityPatchExtrasGUID))
            {
                Log.LogWarning(
                    $"{ModName} v{ModVersion} is self-disabling: Valheim Community Patch Extras is installed, " +
                    "which natively handles world save notifications.");
                enabled = false;
                return;
            }

            ModConfig.Initialize(Config);

            // same check as jotunns GUIManager.IsHeadless(), jotunn isnt here at runtime
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return;
            }

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
