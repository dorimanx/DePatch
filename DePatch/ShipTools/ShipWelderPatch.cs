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
        private static readonly FieldInfo m_detectorSphere = typeof(MyShipToolBase).GetField("m_detectorSphere", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly HashSet<MySlimBlock> SlimBlocks = new HashSet<MySlimBlock>();

        private static void Prefix(MyShipWelder __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled && __instance != null && __instance.Enabled)
            {
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

                if (DePatchPlugin.Instance.Config.ShipToolsEnabled)
                {
                    IEnumerable<ShipTool> enumerable = ShipTool.shipTools.Where((ShipTool t) => t.Subtype == __instance.DefinitionId.SubtypeId.String);
                    if (enumerable.Count() == 0)
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
                        enumerable.AddItem(new ShipTool
                        {
                            Speed = ShipTool.DEFAULT_SPEED,
                            Subtype = __instance.DefinitionId.SubtypeId.String,
                            Type = ToolType.Welder
                        });
                    }
                	var list = new List<MyEntity>();
                	var boundingSphere = (BoundingSphere)m_detectorSphere.GetValue(__instance);
                	var boundingSphereD = new BoundingSphereD(Vector3D.Transform(boundingSphere.Center, __instance.CubeGrid.WorldMatrix), boundingSphere.Radius);
                	MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphereD, list);

                	if (list.Contains(__instance.CubeGrid))
                	{
                    	list.Remove(__instance.CubeGrid);
                	}
                	foreach (var myEntity in list)
                	{
                    	if (!(myEntity is MyCubeGrid myCubeGrid) || myEntity.Physics == null) continue;

                    	var welderMountAmount = MySession.Static.WelderSpeedMultiplier * enumerable.First().Speed;
                    	var maxAllowedBoneMovement = MyShipWelder.WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250f * 0.001f;
                    	SlimBlocks.Clear();
                    	myCubeGrid.GetBlocksInsideSphere(ref boundingSphereD, SlimBlocks, false);

                    	foreach (var mySlimBlock in SlimBlocks)
                    	{
                        	mySlimBlock.IncreaseMountLevel(welderMountAmount,
                            	__instance.OwnerId, __instance.GetInventoryBase(),
                            	maxAllowedBoneMovement, __instance.HelpOthers,
                            	__instance.IDModule.ShareMode, false, false);
                    	}
                	}
            	}
        	}
    	}
	}
}
