using HarmonyLib;
using VRage.GameServices;

namespace DePatch.KEEN_BUG_FIXES
{
    [HarmonyPatch(typeof(MyWorkshopItem))]
    [HarmonyPatch("IsUpToDate")]
    class KEENModUpdateFix
    {
        static void Postfix(MyWorkshopItem __instance, ref bool __result)
        {
            if (__instance.TimeUpdated > __instance.LocalTimeUpdated)
            {
                __result = false;
            }
        }
    }
}
