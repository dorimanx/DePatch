using System;
using System.Collections.Generic;
using System.Linq;
using DePatch.BlocksDisable;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;

namespace DePatch.ShipTools
{
    [PatchShim]

    internal static class ShipWelderPatch
    {
        private static Dictionary<string, int> m_missingComponents;

        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyShipWelder), typeof(ShipWelderPatch), "Activate");
        }

        private static void Activate(MyShipWelder __instance, HashSet<MySlimBlock> targets)
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
            m_missingComponents = new Dictionary<string, int>();
            m_missingComponents.Clear();
            MyInventory inventory = __instance.GetInventory(0);

            foreach (MySlimBlock mySlimBlock in targets)
            {
                if (mySlimBlock.IsFullIntegrity || mySlimBlock == __instance.SlimBlock)
                {
                }
                else
                {
                    MyCubeBlockDefinition.PreloadConstructionModels(mySlimBlock.BlockDefinition);
                    mySlimBlock.GetMissingComponents(m_missingComponents);
                }
            }

            foreach (KeyValuePair<string, int> keyValuePair in m_missingComponents)
            {
                MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), keyValuePair.Key);
                if (Math.Max(keyValuePair.Value - (int)inventory.GetItemAmount(myDefinitionId, MyItemFlags.None, false), 0) != 0 && Sync.IsServer && __instance.UseConveyorSystem)
                    __instance.CubeGrid.GridSystems.ConveyorSystem.PullItem(myDefinitionId, new MyFixedPoint?(keyValuePair.Value), __instance, __instance.GetInventory(0), false, false);
            }

            m_missingComponents.Clear();

            if (Sync.IsServer)
            {
                using (HashSet<MySlimBlock>.Enumerator enumerator = targets.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MySlimBlock mySlimBlock2 = enumerator.Current;
                        if (mySlimBlock2.CubeGrid.Physics != null && mySlimBlock2.CubeGrid.Physics.Enabled && mySlimBlock2 != __instance.SlimBlock)
                        {
                            float num3 = MySession.Static.WelderSpeedMultiplier * shipTool.Speed;
                            bool? flag2 = mySlimBlock2.ComponentStack.WillFunctionalityRise(num3);
                            if (flag2 == null || !flag2.Value || MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, mySlimBlock2.BlockDefinition.BlockPairName, mySlimBlock2.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 0, 0, null))
                            {
                                mySlimBlock2.MoveItemsToConstructionStockpile(inventory);
                                mySlimBlock2.MoveUnneededItemsFromConstructionStockpile(inventory);
                                if (mySlimBlock2.HasDeformation || mySlimBlock2.MaxDeformation > 0.0001f || !mySlimBlock2.IsFullIntegrity)
                                {
                                    float maxAllowedBoneMovement = MyShipWelder.WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250f * 0.001f;
                                    mySlimBlock2.IncreaseMountLevel(num3, __instance.OwnerId, inventory, maxAllowedBoneMovement, __instance.HelpOthers, __instance.IDModule.ShareMode, false, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
