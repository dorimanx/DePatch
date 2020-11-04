using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace DePatch
{
	// Token: 0x0200000C RID: 12
	public static class DamageHandler
	{
		// Token: 0x06000018 RID: 24 RVA: 0x0000270A File Offset: 0x0000090A
		public static void Init()
		{
			if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
			{
				return;
			}
			if (DamageHandler._init)
			{
				return;
			}
			DamageHandler._init = true;
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, new BeforeDamageApplied(DamageHandler.ProcessDamage));
		}

		private static void ProcessDamage(object target, ref MyDamageInformation info)
		{
			long num = info.AttackerId;
			MySlimBlock mySlimBlock = target as MySlimBlock;
			long num2;
			if (mySlimBlock == null)
			{
				if (target is MyCharacter)
				{
					return;
				}
				num2 = 0L;
			}
			else
			{
				if (!PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId))
				{
					return;
				}
				num2 = ((mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L);
			}
			MyEntity myEntity;
			if (MyEntities.TryGetEntityById(info.AttackerId, out myEntity, true))
			{
				if (myEntity is MyVoxelBase)
				{
					return;
				}
				MyAngleGrinder myAngleGrinder = myEntity as MyAngleGrinder;
				if (myAngleGrinder != null)
				{
					num = myAngleGrinder.OwnerIdentityId;
				}
				MyHandDrill myHandDrill = myEntity as MyHandDrill;
				if (myHandDrill != null)
				{
					num = myHandDrill.OwnerIdentityId;
				}
				MyAutomaticRifleGun myAutomaticRifleGun = myEntity as MyAutomaticRifleGun;
				if (myAutomaticRifleGun != null)
				{
					MyPlayer myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList<MyPlayer>().Find((MyPlayer b) => b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId);
					if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) != ContainmentType.Contains)
					{
						return;
					}
					num = myAutomaticRifleGun.OwnerIdentityId;
				}
				MyEngineerToolBase toolBase = myEntity as MyEngineerToolBase;
				if (toolBase != null)
				{
					MyPlayer myPlayer2 = MySession.Static.Players.GetOnlinePlayers().ToList<MyPlayer>().Find((MyPlayer b) => b.Identity.IdentityId == toolBase.OwnerIdentityId);
					if (PVE.PVESphere.Contains(myPlayer2.Character.PositionComp.GetPosition()) != ContainmentType.Contains)
					{
						return;
					}
					num = toolBase.OwnerIdentityId;
				}
				MyUserControllableGun myUserControllableGun = myEntity as MyUserControllableGun;
				if (myUserControllableGun != null)
				{
					num = myUserControllableGun.OwnerId;
				}
				MyCubeGrid myCubeGrid = myEntity as MyCubeGrid;
				if (myCubeGrid != null)
				{
					num = ((myCubeGrid.BigOwners.Count > 0) ? myCubeGrid.BigOwners[0] : 0L);
				}
				MyLargeTurretBase myLargeTurretBase = myEntity as MyLargeTurretBase;
				if (myLargeTurretBase != null)
				{
					num = myLargeTurretBase.OwnerId;
				}
				MyCharacter character = myEntity as MyCharacter;
				if (character != null)
				{
					MyPlayer myPlayer3 = MySession.Static.Players.GetOnlinePlayers().ToList<MyPlayer>().Find((MyPlayer b) => b.Identity.IdentityId == character.GetPlayerIdentityId());
					if (PVE.PVESphere.Contains(myPlayer3.Character.PositionComp.GetPosition()) != ContainmentType.Contains)
					{
						return;
					}
					num = character.GetPlayerIdentityId();
				}
				if (myCubeGrid != null)
				{
					if (!PVE.EntitiesInZone.Contains(myCubeGrid.EntityId))
					{
						return;
					}
				}
				else if (myUserControllableGun != null)
				{
					if (!PVE.EntitiesInZone.Contains(myUserControllableGun.CubeGrid.EntityId))
					{
						return;
					}
				}
				else if (myLargeTurretBase != null && !PVE.EntitiesInZone.Contains(myLargeTurretBase.CubeGrid.EntityId))
				{
					return;
				}
			}
			if (num == 0L)
			{
				info.Amount = 0f;
				return;
			}
			ulong num3 = MySession.Static.Players.TryGetSteamId(num);
			ulong num4 = MySession.Static.Players.TryGetSteamId(num2);
			if (MySession.Static.Players.IdentityIsNpc(num) || num2 == 0L || MySession.Static.Players.IdentityIsNpc(num2) || num2 == info.AttackerId || num3 == num4 || MySession.Static.Factions.TryGetPlayerFaction(num) == MySession.Static.Factions.TryGetPlayerFaction(num2))
			{
				return;
			}
			info.Amount = 0f;
		}

		// Token: 0x0400000A RID: 10
		private static bool _init;
	}
}
