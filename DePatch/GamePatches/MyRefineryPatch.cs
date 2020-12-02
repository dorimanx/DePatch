using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using System;
using VRage.Game;

namespace DePatch
{
    [HarmonyPatch(typeof(MyRefinery), "DoUpdateTimerTick")]
    internal class MyRefineryPatch
    {
        private static void Prefix(MyRefinery __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (!__instance.CubeGrid.IsStatic &&
                    (__instance.CubeGrid.GridSizeEnum == MyCubeSize.Large ||
                    __instance.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                    && DePatchPlugin.Instance.Config.DisableProductionOnShip)
            {
                var blockSubType = __instance.BlockDefinition.Id.SubtypeName;
                string LargeSmallSheld = "LargeShipSmallShieldGeneratorBase";
                string LargeLargeShield = "LargeShipLargeShieldGeneratorBase";
                string SmallSmallShield = "SmallShipSmallShieldGeneratorBase";
                string SmallMicroShield = "SmallShipMicroShieldGeneratorBase";
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
