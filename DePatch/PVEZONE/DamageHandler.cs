using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace DePatch
{
	public static class DamageHandler
	{
        private static bool _init;

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
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, new BeforeDamageApplied(ProcessDamage));
		}

		private static void ProcessDamage(object target, ref MyDamageInformation info)
		{
			long num1 = info.AttackerId;
			MySlimBlock mySlimBlock = target as MySlimBlock;
            long num2;
            long num3 = 10L;

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
					return;

				num2 = ((mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L);
            }
            if (MyEntities.TryGetEntityById(info.AttackerId, out MyEntity myEntity, true))
            {
                if (myEntity is MyVoxelBase)
                    return;

                if (myEntity is MyAngleGrinder myAngleGrinder)
                {
                    num1 = myAngleGrinder.OwnerIdentityId;
                }
                if (myEntity is MyHandDrill myHandDrill)
                {
                    num1 = myHandDrill.OwnerIdentityId;
                }
                if (myEntity is MyAutomaticRifleGun myAutomaticRifleGun)
                {
                    if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                        return;

                    num1 = myAutomaticRifleGun.OwnerIdentityId;
                }
                if (myEntity is MyEngineerToolBase toolBase)
                {
                    if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                        return;

                    num1 = toolBase.OwnerIdentityId;
                }
                if (myEntity as MyUserControllableGun != null)
                {
                    num1 = (myEntity as MyUserControllableGun).OwnerId;
                }
                if (myEntity as MyCubeGrid != null)
                {
                    num1 = (((myEntity as MyCubeGrid).BigOwners.Count > 0) ? (myEntity as MyCubeGrid).BigOwners[0] : 0L);
                }
                if (myEntity as MyLargeTurretBase != null)
                {
                    num1 = (myEntity as MyLargeTurretBase).OwnerId;
                }
                if (myEntity is MyCharacter character)
                {
                    if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                        return;

                    num1 = character.GetPlayerIdentityId();
                }
                if (myEntity as MyCubeGrid != null)
                {
                    if (!PVE.EntitiesInZone.Contains((myEntity as MyCubeGrid).EntityId))
                        return;

                    if (mySlimBlock.CubeGrid.IsStatic && (info.Type == MyDamageType.Fall || info.Type == MyDamageType.Deformation))
                    {
                        num3 = 0L;
                    }
                }
                else if (myEntity as MyUserControllableGun != null)
                {
                    if (!PVE.EntitiesInZone.Contains((myEntity as MyUserControllableGun).CubeGrid.EntityId))
                        return;
                }
                else if (myEntity as MyLargeTurretBase != null && !PVE.EntitiesInZone.Contains((myEntity as MyLargeTurretBase).CubeGrid.EntityId))
                    return;
            }

            if (num1 == 0L || num3 == 0L)
			{
				info.Amount = 0f;
                info.IsDeformation = false;
			}
            else
            {
                ulong steamId1 = MySession.Static.Players.TryGetSteamId(num1);
                ulong steamId2 = MySession.Static.Players.TryGetSteamId(num2);
                if (!MySession.Static.Players.IdentityIsNpc(num1) && num2 != 0L &&
                    !MySession.Static.Players.IdentityIsNpc(num2) && num2 != info.AttackerId && (long)steamId1 != (long)steamId2 &&
                    MySession.Static.Factions.TryGetPlayerFaction(num1) != MySession.Static.Factions.TryGetPlayerFaction(num2))
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                }
            }
        }
	}
}
