using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyConveyorSorterPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyConveyorSorter), "UpdateAfterSimulation10", typeof(MyConveyorSorterPatch), nameof(ConveyorSorterPatch));
        }

        private static bool ConveyorSorterPatch(MyConveyorSorter __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (__instance != null && __instance.OwnerId == 0L && __instance.DrainAll)
            {
                __instance.DrainAll = false;
                return false;
            }

            return true;
        }
    }
}
