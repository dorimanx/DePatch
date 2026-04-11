using NLog;
using Sandbox.Game.EntityComponents;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class ResourceSinkPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyResourceSinkComponent), typeof(ResourceSinkPatch), nameof(ResourceAvailableByType));

        private static bool ResourceAvailableByType(MyResourceSinkComponent __instance, ref float __result, MyDefinitionId resourceTypeId)
        {
            // This code was been provided by @fin_mk Discord, great graditude for sharing the fix.

            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            var block = __instance.ParentBlock;

            if (block == null || !block.IsWorking)
                return true;

            if (resourceTypeId == MyResourceDistributorComponent.ElectricityId)
            {
                var distrib = block.CubeGrid.GridSystems.ResourceDistributor;

                if (distrib != null)
                {
                    __result = distrib.MaxAvailableResourceByType(resourceTypeId, block.CubeGrid);
                    return false;
                }
            }

            return true;
        }
    }
}
