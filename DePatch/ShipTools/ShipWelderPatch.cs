using System;
using System.Collections.Generic;
using System.Linq;
using DePatch.BlocksDisable;
using HarmonyLib;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipWelder), "Activate")]
    internal class ShipWelderPatch
    {
        private static void Prefix(MyShipWelder __instance, HashSet<MySlimBlock> targets)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null) return;

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
                        Type = ToolType.Welder
                    });
                });
                return;
            }

            var shipTool = shipTools.FirstOrDefault();
            if (shipTool == null) return;
            var welderMountAmount = MySession.Static.WelderSpeedMultiplier * shipTool.Speed;
            MyInventory inventory = __instance.GetInventory(0);

            foreach (var mySlimBlock in targets)
            {
                if (mySlimBlock.HasDeformation || mySlimBlock.MaxDeformation > 0.0001f || !mySlimBlock.IsFullIntegrity)
                {
                    float maxAllowedBoneMovement = MyShipWelder.WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250f * 0.001f;

                    mySlimBlock.MoveItemsToConstructionStockpile(inventory);
                    mySlimBlock.MoveUnneededItemsFromConstructionStockpile(inventory);

                    if (__instance.OwnerId != 0L)
                    {
                        _ = mySlimBlock.IncreaseMountLevel(welderMountAmount,
                            __instance.OwnerId, inventory, maxAllowedBoneMovement,
                            __instance.HelpOthers,
                            __instance.IDModule.ShareMode, false, false);
                    }
                    else
                    {
                        _ = mySlimBlock.IncreaseMountLevel(welderMountAmount,
                            0L, inventory, maxAllowedBoneMovement,
                            __instance.HelpOthers,
                            __instance.IDModule.ShareMode, false, false);
                    }
                }
            }
        }
	}
}
