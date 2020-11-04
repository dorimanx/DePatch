using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Havok;
using Sandbox.Engine.Physics;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Entities.Debris;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.WorldEnvironment;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRage.ModAPI;

namespace DePatch
{
	internal class MyVoxelDefenderPatch
	{
		// Token: 0x060000A3 RID: 163 RVA: 0x000049D4 File Offset: 0x00002BD4
		private static bool Prefix(MyGridPhysics __instance, HkBreakOffLogicResult __result, HkRigidBody otherBody, uint shapeKey, ref float maxImpulse)
		{
			__result = MyVoxelDefenderPatch.Logic(__instance, otherBody, shapeKey, ref maxImpulse);
			return false;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000049E4 File Offset: 0x00002BE4
		private static HkBreakOffLogicResult Logic(MyGridPhysics __instance, HkRigidBody otherBody, uint shapeKey, ref float maxImpulse)
		{
			if (maxImpulse == 0f)
			{
				maxImpulse = __instance.Shape.BreakImpulse;
			}
			ulong user = 0UL;
			IMyEntity entity = otherBody.GetEntity(0U);
			if (entity is MyVoxelBase)
			{
				return HkBreakOffLogicResult.DoNotBreakOff;
			}
			MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entity as MyEntity);
			if (controllingPlayer != null)
			{
				user = controllingPlayer.Id.SteamId;
			}
			if (!MySessionComponentSafeZones.IsActionAllowed((MyEntity)MyVoxelDefenderPatch.m_grid.GetValue(__instance), MySafeZoneAction.Damage, 0L, user) || (MySession.Static.Settings.EnableVoxelDestruction && entity is MyVoxelBase))
			{
				return HkBreakOffLogicResult.DoNotBreakOff;
			}
			HkBreakOffLogicResult result = HkBreakOffLogicResult.UseLimit;
			if (!Sync.IsServer)
			{
				result = HkBreakOffLogicResult.DoNotBreakOff;
			}
			else if (__instance.RigidBody == null || __instance.Entity.MarkedForClose || otherBody == null)
			{
				result = HkBreakOffLogicResult.DoNotBreakOff;
			}
			else
			{
				IMyEntity entity2 = otherBody.GetEntity(0U);
				if (entity2 == null)
				{
					return HkBreakOffLogicResult.DoNotBreakOff;
				}
				if (entity2 is MyEnvironmentSector || entity2 is MyFloatingObject || entity2 is MyDebrisBase)
				{
					result = HkBreakOffLogicResult.DoNotBreakOff;
				}
				else if (entity2 is MyCharacter)
				{
					result = HkBreakOffLogicResult.DoNotBreakOff;
				}
				else if (entity2.GetTopMostParent(null) == __instance.Entity)
				{
					result = HkBreakOffLogicResult.DoNotBreakOff;
				}
				else
				{
					MyCubeGrid myCubeGrid = entity2 as MyCubeGrid;
					if (!MySession.Static.Settings.EnableSubgridDamage && myCubeGrid != null && MyCubeGridGroups.Static.Physical.HasSameGroup((MyCubeGrid)MyVoxelDefenderPatch.m_grid.GetValue(__instance), myCubeGrid))
					{
						result = HkBreakOffLogicResult.DoNotBreakOff;
					}
					else if (__instance.Entity is MyCubeGrid || myCubeGrid != null)
					{
						result = HkBreakOffLogicResult.UseLimit;
					}
				}
				if (__instance.WeldInfo.Children.Count > 0)
				{
					__instance.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(__instance.RigidBody, __instance.Shape.BreakImpulse);
				}
			}
			bool deformation_LOGGING = MyFakes.DEFORMATION_LOGGING;
			return result;
		}

		// Token: 0x0400005D RID: 93
		private static FieldInfo m_grid = ReflectionUtils.GetField<MyGridPhysics>("m_grid", true);
	}
}
