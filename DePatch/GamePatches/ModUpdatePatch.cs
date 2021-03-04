using HarmonyLib;
using VRage.GameServices;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyWorkshopItem))]
    [HarmonyPatch("IsUpToDate")] 
    class ModUpdatePatch
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
