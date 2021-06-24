using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.BlocksDisable
{
    //[HarmonyPatch(typeof(MyShipDrill), "UpdateAfterSimulation100")]
    [PatchShim]
    internal static class AreaShow_DrillSystem
    {
        private static readonly string DrillProperty = "Drill.ShowArea";

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyShipDrill).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(AreaShow_DrillSystem).GetMethod(nameof(UpdateAfterSimulation100), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateAfterSimulation100(MyTerminalBlock __instance)
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
