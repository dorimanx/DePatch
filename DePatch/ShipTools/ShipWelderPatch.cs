using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch
{
	[HarmonyPatch(typeof(MyShipWelder), "Activate")]
	internal class ShipWelderPatch
	{
		private static readonly FieldInfo m_detectorSphere = typeof(MyShipToolBase).GetField(nameof (m_detectorSphere), BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly HashSet<MySlimBlock> slimBlocks = new HashSet<MySlimBlock>();

		private static void Prefix(MyShipWelder __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled && !DePatchPlugin.Instance.Config.ShipToolsEnabled)
			{
				return;
			}
			IEnumerable<ShipTool> enumerable = from t in ShipTool.shipTools
			where t.Subtype == __instance.DefinitionId.SubtypeId.String
			select t;
			if (enumerable.Count<ShipTool>() == 0)
			{
				DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate()
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
			List<MyEntity> list = new List<MyEntity>();
			BoundingSphere boundingSphere = (BoundingSphere)ShipWelderPatch.m_detectorSphere.GetValue((object) __instance);
			BoundingSphereD boundingSphereD = new BoundingSphereD(Vector3D.Transform(boundingSphere.Center, __instance.CubeGrid.WorldMatrix), (double)boundingSphere.Radius);
			MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphereD, list, MyEntityQueryType.Both);
			if (list.Contains((MyEntity) __instance.CubeGrid))
			{
				list.Remove(__instance.CubeGrid);
			}
			foreach (MyEntity myEntity in list)
			{
				MyCubeGrid myCubeGrid = myEntity as MyCubeGrid;
				if (myCubeGrid != null && myEntity.Physics != null)
				{
					MyInventoryBase inventoryBase = __instance.GetInventoryBase();
					bool helpOthers = __instance.HelpOthers;
					float welderMountAmount = MySession.Static.WelderSpeedMultiplier * enumerable.First<ShipTool>().Speed;
					ShipWelderPatch.slimBlocks.Clear();
					myCubeGrid.GetBlocksInsideSphere(ref boundingSphereD, ShipWelderPatch.slimBlocks, true);
					foreach (MySlimBlock mySlimBlock in ShipWelderPatch.slimBlocks)
						mySlimBlock.IncreaseMountLevel(welderMountAmount, __instance.OwnerId, inventoryBase, 0.6f, helpOthers, MyOwnershipShareModeEnum.Faction, false);
				}
			}
		}
	}
}
