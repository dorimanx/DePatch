using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI.Interfaces;
using System.Collections.Concurrent;


namespace DePatch.BlocksDisable
{
    [HarmonyPatch(typeof(MyShipDrill), "UpdateAfterSimulation100")]
    class AreaShow_DrillSystem
    {
        private static readonly ConcurrentDictionary<long, MyTerminalBlock> BlocksToRemoveArea = new ConcurrentDictionary<long, MyTerminalBlock>();

        private static void Prefix(MyTerminalBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.NanoDrillArea || __instance == null || __instance.Closed || __instance.MarkedForClose)
                return;

            bool foundObject = false;
            string propertyId = string.Empty;

            if (__instance is MyShipDrill && __instance.BlockDefinition.Id.SubtypeName.Contains("NanobotDrillSystem"))
            {
                foundObject = true;
                propertyId = "Drill.ShowArea";
            }

            if (foundObject)
            {
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
