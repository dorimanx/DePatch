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
        internal static readonly MethodInfo update;
        internal static readonly MethodInfo updatePatch;

        public static void Patch(PatchContext ctx) => _ = ctx.GetPattern(update).Prefixes.Add(updatePatch);

        public static bool Patchm(MyShipWelder __instance)
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

        static MyWelderPatch()
        {
            MethodInfo method1 = typeof(MyShipWelder).GetMethod("Activate", BindingFlags.Instance | BindingFlags.NonPublic);
            update = method1 ?? throw new Exception("Failed to find method to patch");
            MethodInfo method2 = typeof(MyWelderPatch).GetMethod("Patchm", BindingFlags.Static | BindingFlags.Public);
            updatePatch = method2 ?? throw new Exception("Failed to find patch method");
        }
    }
}
