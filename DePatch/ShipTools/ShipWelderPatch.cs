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

            MyInventory inventory = MyEntityExtensions.GetInventory(__instance);

            foreach (KeyValuePair<string, int> missingComponent in m_missingComponents)
            {
                MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), missingComponent.Key);
                if (Math.Max(missingComponent.Value - (int)inventory.GetItemAmount(myDefinitionId, MyItemFlags.None, false), 0) != 0 && Sync.IsServer && __instance.UseConveyorSystem)
                    __instance.CubeGrid.GridSystems.ConveyorSystem.PullItem(myDefinitionId, new MyFixedPoint?(missingComponent.Value), __instance, MyEntityExtensions.GetInventory(__instance), false, false);
            }

            if (Sync.IsServer)
            {
                float num1 = 0.25f / Math.Min(4, count > 0 ? count : 1);

                foreach (MySlimBlock target in targets)
                {
                    if (target.CubeGrid.Physics != null && target.CubeGrid.Physics.Enabled && target != __instance.SlimBlock)
                    {
                        float num2 = MySession.Static.WelderSpeedMultiplier * MyShipWelder.WELDER_AMOUNT_PER_SECOND * num1;

                        if (num2 < shipTool.Speed)
                            num2 = shipTool.Speed;

                        bool? nullable = target.ComponentStack.WillFunctionalityRise(num2);
                        if (!nullable.HasValue || !nullable.Value || MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, target.BlockDefinition.BlockPairName, target.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST))
                        {
                            if (target.CanContinueBuild(inventory))
                                flag = true;

                            target.MoveItemsToConstructionStockpile(inventory);
                            target.MoveUnneededItemsFromConstructionStockpile(inventory);

                            if (target.HasDeformation || target.MaxDeformation > 9.99999974737875E-05 || !target.IsFullIntegrity)
                            {
                                float maxAllowedBoneMovement = (float)(MyShipWelder.WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250.0 * (1.0 / 1000.0));
                                target.IncreaseMountLevel(num2, __instance.OwnerId, inventory, maxAllowedBoneMovement, __instance.HelpOthers, __instance.IDModule.ShareMode);
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

                if (__instance.UseConveyorSystem)
                {
                    foreach (MyWelder.ProjectionRaycastData projectionRaycastData in projectedBlocks)
                    {
                        MyCubeBlockDefinition.Component[] components = projectionRaycastData.hitCube.BlockDefinition.Components;
                        if (components != null && components.Length != 0)
                            __instance.CubeGrid.GridSystems.ConveyorSystem.PullItem(components[0].Definition.Id, new MyFixedPoint?(1), __instance, inventory, false, false);
                    }
                }

                HashSet<MyCubeGrid.MyBlockLocation> myBlockLocationSet = new HashSet<MyCubeGrid.MyBlockLocation>();
                bool creativeMode = MySession.Static.CreativeMode;

                if (MySession.Static.Players.TryGetPlayerId(__instance.BuiltBy, out MyPlayer.PlayerId result) && MySession.Static.Players.TryGetPlayerById(result, out MyPlayer _))
                    creativeMode |= MySession.Static.CreativeToolsEnabled(Sync.MyId);

                foreach (MyWelder.ProjectionRaycastData projectionRaycastData in projectedBlocks)
                {
                    if (__instance.IsWithinWorldLimits(projectionRaycastData.cubeProjector, projectionRaycastData.hitCube.BlockDefinition.BlockPairName, creativeMode ? projectionRaycastData.hitCube.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST) && (MySession.Static.CreativeMode || inventory.ContainItems(new MyFixedPoint?(1), projectionRaycastData.hitCube.BlockDefinition.Components[0].Definition.Id)))
                    {
                        MyWelder.ProjectionRaycastData invokedBlock = projectionRaycastData;
                        MySandboxGame.Static.Invoke(() =>
                        {
                            if (invokedBlock.cubeProjector.Closed || invokedBlock.cubeProjector.CubeGrid.Closed || invokedBlock.hitCube.FatBlock != null && invokedBlock.hitCube.FatBlock.Closed)
                                return;

                            invokedBlock.cubeProjector.Build(invokedBlock.hitCube, __instance.OwnerId, __instance.EntityId, builtBy: __instance.BuiltBy);

                        }, "ShipWelder BuildProjection");
                        flag = true;
                    }
                }
            }

            if (flag)
                SetBuildingMusic.Invoke(__instance, new object[] { 150 });

            __result = flag;

            return false;
        }
    }
}