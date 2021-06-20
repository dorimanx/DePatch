using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Entities.Blocks;
using System.Collections.Concurrent;


namespace DePatch.BlocksDisable
{
    [HarmonyPatch(typeof(MyShipToolBase), "UpdateAfterSimulation10")]
    class AreaShow_BuildAndRepairSystem
    {
        private static readonly ConcurrentDictionary<long, MyTerminalBlock> BlocksToRemoveArea = new ConcurrentDictionary<long, MyTerminalBlock>();

        public static void Prefix(MyTerminalBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.NanoBuildArea || __instance == null || __instance.Closed || __instance.MarkedForClose)
                return;

            if (__instance is MyShipWelder && __instance.BlockDefinition.Id.SubtypeName.Contains("BuildAndRepairSystem"))
            {
                string propertyId = "BuildAndRepair.ShowArea";

                ITerminalProperty showAreaProperty = __instance.GetProperty(propertyId);
                if (showAreaProperty != null)
                {
                    if (BlocksToRemoveArea.ContainsKey(__instance.EntityId))
                    {
                        __instance.SetValueBool(propertyId, value: false);
                        __instance.CubeGrid.RaiseGridChanged();
                        BlocksToRemoveArea.TryRemove(__instance.EntityId, out var _);
                        return;
                    }

                    if (__instance.GetValueBool(propertyId))
                    {
                        if (!ReflectionUtils.PlayersNarby(__instance, 1500))
                            BlocksToRemoveArea[__instance.EntityId] = __instance;
                    }
                }
            }
        }
    }
}
