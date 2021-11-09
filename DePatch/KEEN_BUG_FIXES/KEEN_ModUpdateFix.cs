using HarmonyLib;
using VRage.GameServices;

namespace DePatch.KEEN_BUG_FIXES
{
    [HarmonyPatch(typeof(MyWorkshopItem))]
    [HarmonyPatch("IsUpToDate")]
    class KEEN_ModUpdateFix
    {
        static void Postfix(MyWorkshopItem __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (__instance.TimeUpdated > __instance.LocalTimeUpdated)
                __result = false;
        }
    }
}