﻿using Sandbox.Game.Entities.Cube;
using HarmonyLib;
using VRage.Game;

namespace DePatch
{
    [HarmonyPatch(typeof(MyAssembler), "UpdateBeforeSimulation100")]
    internal class MyAssemblerPatch
    {
        private static void Prefix(MyAssembler __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;
            if (__instance.IsSlave && DePatchPlugin.Instance.Config.DisableAssemblerCoop)
            {
                __instance.IsSlave = false;
                __instance.ClearQueue();
            }
            if (__instance.RepeatEnabled && DePatchPlugin.Instance.Config.DisableAssemblerLoop)
            {
                __instance.RequestRepeatEnabled(false);
                __instance.ClearQueue();
            }
            if (!__instance.CubeGrid.IsStatic && __instance.CubeGrid.GridSizeEnum == MyCubeSize.Large && DePatchPlugin.Instance.Config.DisableProductionOnShip)
            {
                if (__instance.Enabled)
                {
                    __instance.Enabled = false;
                    __instance.ClearQueue();
                }
            }
        }
    }
}
