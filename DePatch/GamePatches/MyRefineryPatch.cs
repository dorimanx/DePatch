using System;
using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using VRage.Game;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyRefinery), "DoUpdateTimerTick")]
    internal class MyRefineryPatch
    {
        private static void Prefix(MyRefinery __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if (!__instance.CubeGrid.IsStatic &&
                        (__instance.CubeGrid.GridSizeEnum == MyCubeSize.Large ||
                        __instance.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                        && DePatchPlugin.Instance.Config.DisableProductionOnShip)
                {
                    var blockSubType = __instance.BlockDefinition.Id.SubtypeName;
                    var LargeSmallSheld = "LargeShipSmallShieldGeneratorBase";
                    var LargeLargeShield = "LargeShipLargeShieldGeneratorBase";
                    var SmallSmallShield = "SmallShipSmallShieldGeneratorBase";
                    var SmallMicroShield = "SmallShipMicroShieldGeneratorBase";
                    if (__instance != null && (
                            string.Compare(LargeSmallSheld, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare(LargeLargeShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare(SmallSmallShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare(SmallMicroShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                    }
                    else
                    {
                        if (__instance.Enabled)
                        {
                            __instance.Enabled = false;
                        }
                    }
                }
            }
        }
    }
}
