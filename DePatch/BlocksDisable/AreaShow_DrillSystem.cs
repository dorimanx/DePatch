using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;

namespace DePatch.BlocksDisable
{
    [HarmonyPatch(typeof(MyShipDrill), "UpdateAfterSimulation100")]
    class AreaShow_DrillSystem
    {
        private static readonly string DrillProperty = "Drill.ShowArea";

        private static void Prefix(MyTerminalBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.NanoDrillArea || __instance == null || __instance.Closed || __instance.MarkedForClose)
                return;

            if (__instance is MyShipDrill && __instance.GetProperty(DrillProperty) != null && __instance.GetValueBool(DrillProperty) && !ReflectionUtils.PlayersNarby(__instance, 1000))
            {
                __instance.SetValueBool(DrillProperty, value: false);
                __instance.CubeGrid.RaiseGridChanged();
            }
        }
    }
}
