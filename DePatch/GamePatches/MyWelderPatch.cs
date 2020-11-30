using System;
using System.Reflection;
using NLog;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch
{
    [PatchShim]
    public static class MyWelderPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        internal static readonly MethodInfo update = typeof(MyShipWelder).GetMethod("Activate", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find method to patch");
        internal static readonly MethodInfo updatePatch = typeof(MyWelderPatch).GetMethod("Patchm", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx) => ctx.GetPattern(update).Prefixes.Add(updatePatch);

        private static bool Patchm(MyShipWelder __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            bool result;
            var blockSubType = __instance.BlockDefinition.Id.SubtypeName;
            string subtypeLarge = "SELtdLargeNanobotBuildAndRepairSystem";
            string subtypeSmall = "SELtdSmallNanobotBuildAndRepairSystem";
            if (!__instance.CubeGrid.IsStatic &&
                    (__instance.CubeGrid.GridSizeEnum == MyCubeSize.Large ||
                    __instance.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                    && DePatchPlugin.Instance.Config.DisableNanoBotsOnShip)
            {
                if (__instance != null && (
                        string.Compare(subtypeLarge, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        string.Compare(subtypeSmall, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    if (__instance.Enabled)
                    {
                        __instance.Enabled = false;
                    }
                }
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
