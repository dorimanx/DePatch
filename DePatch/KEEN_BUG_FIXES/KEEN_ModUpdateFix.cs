using Torch.Managers.PatchManager;
using VRage.GameServices;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_ModUpdateFix
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyWorkshopItem), typeof(KEEN_ModUpdateFix), nameof(IsUpToDate));

        static void IsUpToDate(MyWorkshopItem __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (__instance.TimeUpdated > __instance.LocalTimeUpdated)
                __result = false;
        }
    }
}