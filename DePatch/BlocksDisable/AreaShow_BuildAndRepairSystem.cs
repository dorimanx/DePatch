using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.BlocksDisable
{
    [PatchShim]

    internal static class AreaShow_BuildAndRepairSystem
    {
        private static readonly string WelderProperty = "BuildAndRepair.ShowArea";

        public static void Patch(PatchContext ctx)
        {
            ctx.Suffix(typeof(MyShipToolBase), typeof(AreaShow_BuildAndRepairSystem), "UpdateAfterSimulation10");
        }

        private static void UpdateAfterSimulation10(MyTerminalBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.NanoBuildArea || __instance == null || __instance.Closed || __instance.MarkedForClose)
                return;

            if (__instance is MyShipWelder && __instance.GetProperty(WelderProperty) != null && __instance.GetValueBool(WelderProperty) && (__instance as MyShipWelder).Enabled == false)
            {
                __instance.SetValueBool(WelderProperty, value: false);
                __instance.CubeGrid.RaiseGridChanged();
            }
        }
    }
}
