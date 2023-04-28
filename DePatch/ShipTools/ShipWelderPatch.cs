using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DePatch.BlocksDisable;
using Sandbox;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
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
        private static readonly Dictionary<string, int> m_missingComponents = new Dictionary<string, int>();

        internal readonly static MethodInfo FindProjectedBlocks = typeof(MyShipWelder).EasyMethod("FindProjectedBlocks");
        internal readonly static MethodInfo SetBuildingMusic = typeof(MyShipToolBase).EasyMethod("SetBuildingMusic");

        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyShipWelder), typeof(ShipWelderPatch), nameof(Activate));
        }

        public static bool Activate(MyShipWelder __instance, HashSet<MySlimBlock> targets, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null || __instance.MarkedForClose || __instance.Closed)
                return true;

            if (!__instance.CubeGrid.IsStatic && DePatchPlugin.Instance.Config.DisableNanoBotsOnShip)
            {
                var subtypeLarge = "SELtdLargeNanobotBuildAndRepairSystem";
                var subtypeSmall = "SELtdSmallNanobotBuildAndRepairSystem";
                var blockSubType = __instance.BlockDefinition.Id.SubtypeName;

                if (string.Compare(subtypeLarge, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                    string.Compare(subtypeSmall, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    if (__instance.Enabled)
                        __instance.Enabled = false;
                }
            }

            if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
            {
                if (__instance.IsFunctional && !MySession.Static.Players.IsPlayerOnline(__instance.OwnerId))
                {
                    if (PlayersUtility.KeepBlockOffWelder(__instance))
                    {
                        if (__instance.Enabled)
                            __instance.Enabled = false;
                    }
                }
            }

            if (!DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return true;

            if (targets.Count == 0)
            {
                __result = false;
                return false;
            }

            var enumerable = ShipTool.shipTools.Where(t => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            var shipTools = enumerable.ToList();

            if (!shipTools.Any())
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                {
                    ShipTool.shipTools.Add(new ShipTool
                    {
                        Speed = ShipTool.DEFAULT_SPEED_W,
                        Subtype = __instance.DefinitionId.SubtypeId.String,
                        Type = ToolType.Welder
                    });
                });
                return true;
            }

            var shipTool = shipTools.FirstOrDefault();
            if (shipTool == null)
                return true;

            var def = (MyShipWelderDefinition)__instance.BlockDefinition;
            if (def.SensorRadius < 0.01f) //NanobotOptimiztion
                return false;

            // prevent self welding
            targets.Remove(__instance.SlimBlock);

            bool flag = false;
            int count = targets.Count;
            m_missingComponents.Clear();

            HashSet<MySlimBlock> targetsReduced = new HashSet<MySlimBlock>();

            if (targets.Count > 30)
            {
                int targetsCount = 0;
                foreach (var NewTraget in targets)
                {
                    if (NewTraget.IsFullIntegrity)
                        continue;

                    targetsReduced.Add(NewTraget);
                    targetsCount++;

                    if (targetsCount == 30)
                        break;
                }

                targets.Clear();
                targets = targetsReduced;
                count = targets.Count;
            }

            foreach (MySlimBlock target in targets)
            {
                if (target.IsFullIntegrity || target == __instance.SlimBlock)
                    count--;
                else
                {
                    MyCubeBlockDefinition.PreloadConstructionModels(target.BlockDefinition);
                    target.GetMissingComponents(m_missingComponents);
                }
            }

            float num1 = 0.25f / Math.Min(4, count > 0 ? count : 1);
            float num2 = MySession.Static.WelderSpeedMultiplier * MyShipWelder.WELDER_AMOUNT_PER_SECOND * num1;

            if (num2 < shipTool.Speed)
                num2 = shipTool.Speed;
            else
                return true;

            MyInventory inventory = __instance.GetInventory(0);

            foreach (KeyValuePair<string, int> missingComponent in m_missingComponents)
            {
                var ShipToolBase = __instance as MyShipToolBase; // base.UseConveyorSystem
                var CubeBlock = __instance as MyCubeBlock; // base.CubeGrid.GridSystems.ConveyorSystem
                MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), missingComponent.Key);
                if (Math.Max(missingComponent.Value - (int)inventory.GetItemAmount(myDefinitionId, MyItemFlags.None, false), 0) != 0 && Sync.IsServer && ShipToolBase.UseConveyorSystem)
                    CubeBlock.CubeGrid.GridSystems.ConveyorSystem.PullItem(myDefinitionId, new MyFixedPoint?(missingComponent.Value), __instance, __instance.GetInventory(0), false, false);
            }

            if (Sync.IsServer)
            {
                foreach (MySlimBlock target in targets)
                {
                    if (target.CubeGrid.Physics != null && target.CubeGrid.Physics.Enabled && target != __instance.SlimBlock)
                    {
                        bool? nullable = target.ComponentStack.WillFunctionalityRise(num2);
                        if (!nullable.HasValue || !nullable.Value || MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, target.BlockDefinition.BlockPairName, target.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST))
                        {
                            if (target.CanContinueBuild(inventory))
                                flag = true;

                            target.MoveItemsToConstructionStockpile(inventory);
                            target.MoveUnneededItemsFromConstructionStockpile(inventory);

                            if (target.HasDeformation || target.MaxDeformation > 0.0001f || !target.IsFullIntegrity)
                            {
                                var CubeBlock = __instance as MyCubeBlock; // base.OwnerId
                                float maxAllowedBoneMovement = (float)(MyShipWelder.WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250f * 0.001f);
                                target.IncreaseMountLevel(num2, CubeBlock.OwnerId, inventory, maxAllowedBoneMovement, __instance.HelpOthers, CubeBlock.IDModule.ShareMode, false, false);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (MySlimBlock target in targets)
                {
                    if (target != __instance.SlimBlock && target.CanContinueBuild(inventory))
                        flag = true;
                }
            }
            m_missingComponents.Clear();

            if (!flag && Sync.IsServer)
            {
                MyWelder.ProjectionRaycastData[] projectedBlocks = (MyWelder.ProjectionRaycastData[])FindProjectedBlocks.Invoke(__instance, new object[] { });

                var ShipToolBase = __instance as MyShipToolBase; // base.UseConveyorSystem

                if (ShipToolBase.UseConveyorSystem)
                {
                    foreach (MyWelder.ProjectionRaycastData projectionRaycastData in projectedBlocks)
                    {
                        var MyCubeBlock2 = __instance as MyCubeBlock;
                        MyCubeBlockDefinition.Component[] components = projectionRaycastData.hitCube.BlockDefinition.Components;
                        if (components != null && components.Length != 0)
                            MyCubeBlock2.CubeGrid.GridSystems.ConveyorSystem.PullItem(components[0].Definition.Id, new MyFixedPoint?(1), __instance, inventory, false, false);
                    }
                }

                HashSet<MyCubeGrid.MyBlockLocation> myBlockLocationSet = new HashSet<MyCubeGrid.MyBlockLocation>();
                bool creativeMode = MySession.Static.CreativeMode;
                var MyCubeBlock = __instance as MyCubeBlock;

                if (MySession.Static.Players.TryGetPlayerId(MyCubeBlock.BuiltBy, out MyPlayer.PlayerId result) && MySession.Static.Players.TryGetPlayerById(result, out MyPlayer _))
                    creativeMode |= MySession.Static.CreativeToolsEnabled(Sync.MyId);

                foreach (MyWelder.ProjectionRaycastData projectionRaycastData in projectedBlocks)
                {
                    if (__instance.IsWithinWorldLimits(projectionRaycastData.cubeProjector, projectionRaycastData.hitCube.BlockDefinition.BlockPairName, creativeMode ? projectionRaycastData.hitCube.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST) && (MySession.Static.CreativeMode || inventory.ContainItems(new MyFixedPoint?(1), projectionRaycastData.hitCube.BlockDefinition.Components[0].Definition.Id)))
                    {
                        MyWelder.ProjectionRaycastData invokedBlock = projectionRaycastData;
                        MySandboxGame.Static.Invoke(() =>
                        {
                            if (!invokedBlock.cubeProjector.Closed && !invokedBlock.cubeProjector.CubeGrid.Closed && (invokedBlock.hitCube.FatBlock == null || !invokedBlock.hitCube.FatBlock.Closed))
                                invokedBlock.cubeProjector.Build(invokedBlock.hitCube, __instance.OwnerId, __instance.EntityId, true, __instance.BuiltBy);

                        }, "ShipWelder BuildProjection", -1, 0);
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                var ShipToolBase = __instance as MyShipToolBase;
                SetBuildingMusic.Invoke(ShipToolBase, new object[] { 150 });
            }

            __result = flag;

            return false;
        }
    }
}