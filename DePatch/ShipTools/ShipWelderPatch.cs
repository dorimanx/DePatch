using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DePatch.BlocksDisable;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipWelder), "Activate")]
    internal class ShipWelderPatch
    {
        private static void Prefix(MyShipWelder __instance, HashSet<MySlimBlock> targets)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null || !__instance.Enabled) return;
            if (!__instance.CubeGrid.IsStatic &&
                (__instance.CubeGrid.GridSizeEnum == MyCubeSize.Large ||
                 __instance.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                && DePatchPlugin.Instance.Config.DisableNanoBotsOnShip)
            {
                var subtypeLarge = "SELtdLargeNanobotBuildAndRepairSystem";
                var subtypeSmall = "SELtdSmallNanobotBuildAndRepairSystem";
                var blockSubType = __instance.BlockDefinition.Id.SubtypeName;

                if (string.Compare(subtypeLarge, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                    string.Compare(subtypeSmall, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    __instance.Enabled = false;
                }
            }

            if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
            {
                if (__instance.IsFunctional && !MySession.Static.Players.IsPlayerOnline(__instance.OwnerId))
                {
                    if (PlayersUtility.KeepBlockOffWelder(__instance))
                        __instance.Enabled = false;
                }
            }

            if (!DePatchPlugin.Instance.Config.ShipToolsEnabled) return;
            var enumerable = ShipTool.shipTools.Where(t => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            var shipTools = enumerable.ToList();
            if (!shipTools.Any())
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                {
                    ShipTool.shipTools.Add(new ShipTool
                    {
                        Speed = ShipTool.DEFAULT_SPEED,
                        Subtype = __instance.DefinitionId.SubtypeId.String,
                        Type = ToolType.Grinder
                    });
                });
                return;
            }

            var shipTool = shipTools.FirstOrDefault();
            if (shipTool == null) return;
            var grinderAmount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;
            foreach (var mySlimBlock in targets)
            {
                mySlimBlock.IncreaseMountLevel(grinderAmount, __instance.OwnerId, __instance.GetInventoryBase());
            }
        }
	}
}
