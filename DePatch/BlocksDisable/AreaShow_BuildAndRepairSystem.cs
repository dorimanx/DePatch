using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Entities.Blocks;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.BlocksDisable
{
    //[HarmonyPatch(typeof(MyShipToolBase), "UpdateAfterSimulation10")]
    [PatchShim]

    internal static class AreaShow_BuildAndRepairSystem
    {
        private static readonly string WelderProperty = "BuildAndRepair.ShowArea";

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyShipToolBase).GetMethod("UpdateAfterSimulation10", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(AreaShow_BuildAndRepairSystem).GetMethod(nameof(UpdateAfterSimulation10), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
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
