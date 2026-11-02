using HarmonyLib;
using WorldSaveMuzzler.Notices;

namespace WorldSaveMuzzler.Patches
{
    [HarmonyPatch(typeof(MessageHud))]
    internal static class MessageHudPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MessageHud.ShowMessage))]
        private static bool ShowMessagePrefix(ref MessageHud.MessageType type, ref string text) =>
            SaveNoticeManager.HandleShowMessage(ref type, ref text);
    }
}
