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

        public static bool Activate(MyShipWelder __instance, ref bool __result, HashSet<MySlimBlock> targets)
        {
            __result = false;

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
            int num = targets.Count;
            m_missingComponents.Clear();

            foreach (MySlimBlock mySlimBlock in targets)
            {
                if (mySlimBlock.IsFullIntegrity || mySlimBlock == __instance.SlimBlock)
                {
                    num--;
                }
                else
                {
                    MyCubeBlockDefinition.PreloadConstructionModels(mySlimBlock.BlockDefinition);
                    mySlimBlock.GetMissingComponents(m_missingComponents);
                }
            }
            MyInventory inventory = __instance.GetInventory(0);
            foreach (KeyValuePair<string, int> keyValuePair in m_missingComponents)
            {
                MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), keyValuePair.Key);
                if (Math.Max(keyValuePair.Value - (int)inventory.GetItemAmount(myDefinitionId, MyItemFlags.None, false), 0) != 0 && Sync.IsServer && __instance.UseConveyorSystem)
                    __instance.CubeGrid.GridSystems.ConveyorSystem.PullItem(myDefinitionId, new MyFixedPoint?(keyValuePair.Value), __instance, inventory, false, false);
            }
            if (Sync.IsServer)
            {
                float num2 = 0.25f / Math.Min(4, (num > 0) ? num : 1);
                float num3 = MySession.Static.WelderSpeedMultiplier * MyShipWelder.WELDER_AMOUNT_PER_SECOND * num2;

                if (num3 < shipTool.Speed)
                    num3 = shipTool.Speed;

                using (HashSet<MySlimBlock>.Enumerator enumerator = targets.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MySlimBlock mySlimBlock2 = enumerator.Current;
                        if (mySlimBlock2.CubeGrid.Physics != null && mySlimBlock2.CubeGrid.Physics.Enabled && mySlimBlock2 != __instance.SlimBlock)
                        {
                            bool? flag2 = mySlimBlock2.ComponentStack.WillFunctionalityRise(num3);
                            if (flag2 == null || !flag2.Value || MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, mySlimBlock2.BlockDefinition.BlockPairName, mySlimBlock2.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 0, 0, null))
                            {
                                if (mySlimBlock2.CanContinueBuild(inventory))
                                    flag = true;

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
                    goto Exit;
                }
            }
            foreach (MySlimBlock mySlimBlock3 in targets)
            {
                if (mySlimBlock3 != __instance.SlimBlock && mySlimBlock3.CanContinueBuild(inventory))
                    flag = true;
            }
        Exit:
            m_missingComponents.Clear();

            if (!flag && Sync.IsServer)
            {
                MyWelder.ProjectionRaycastData[] array = (MyWelder.ProjectionRaycastData[])FindProjectedBlocks.Invoke(__instance, new object[] { });

                if (__instance.UseConveyorSystem)
                {
                    MyWelder.ProjectionRaycastData[] array2 = array;
                    foreach (MyWelder.ProjectionRaycastData PRayData in array2)
                    {
                        MyCubeBlockDefinition.Component[] components = PRayData.hitCube.BlockDefinition.Components;
                        if (components != null && components.Length != 0)
                        {
                            MyDefinitionId id = components[0].Definition.Id;
                            __instance.CubeGrid.GridSystems.ConveyorSystem.PullItem(id, new MyFixedPoint?(1), __instance, inventory, false, false);
                        }
                    }
                }

                new HashSet<MyCubeGrid.MyBlockLocation>();
                bool flag3 = MySession.Static.CreativeMode;

                if (MySession.Static.Players.TryGetPlayerId(__instance.BuiltBy, out MyPlayer.PlayerId id2) && MySession.Static.Players.TryGetPlayerById(id2, out MyPlayer myPlayer))
                    flag3 |= MySession.Static.CreativeToolsEnabled(Sync.MyId);

                foreach (MyWelder.ProjectionRaycastData projectionRaycastData in array)
                {
                    if (__instance.IsWithinWorldLimits(projectionRaycastData.cubeProjector, projectionRaycastData.hitCube.BlockDefinition.BlockPairName, flag3 ? projectionRaycastData.hitCube.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST) && (MySession.Static.CreativeMode || inventory.ContainItems(new MyFixedPoint?(1), projectionRaycastData.hitCube.BlockDefinition.Components[0].Definition.Id, MyItemFlags.None)))
                    {
                        MyWelder.ProjectionRaycastData invokedBlock = projectionRaycastData;
                        MySandboxGame.Static.Invoke(delegate ()
                        {
                            if (!invokedBlock.cubeProjector.Closed && !invokedBlock.cubeProjector.CubeGrid.Closed && (invokedBlock.hitCube.FatBlock == null || !invokedBlock.hitCube.FatBlock.Closed))
                                invokedBlock.cubeProjector.Build(invokedBlock.hitCube, __instance.OwnerId, __instance.EntityId, true, __instance.BuiltBy);

                        }, "ShipWelder BuildProjection", -1, 0);
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
