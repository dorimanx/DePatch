using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DePatch.PVEZONE
{
    public static class DamageHandler
    {
        private static bool _init;

        public static void Init()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                return;
            }
            if (_init)
            {
                return;
            }
            _init = true;
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, ProcessDamage);
        }

        private static void ProcessDamage(object target, ref MyDamageInformation info)
        {
            var num1 = info.AttackerId;
            var mySlimBlock = target as MySlimBlock;
            long num2;
            var num3 = 2f;

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
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                {
                    var zone1 = false;
                    var zone2 = false;
                    if (!PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId))
                        zone1 = true;
                    if (!PVE.EntitiesInZone2.Contains(mySlimBlock.CubeGrid.EntityId))
                        zone2 = true;

                    if (zone1 && zone2)
                        return;
                }
                else
                {
                    if (!PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId))
                        return;
                }

                num2 = (mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L;
            }

            /* Warhead Protection inside PVE Zones */
            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                var zone1 = false;
                var zone2 = false;
                if (mySlimBlock != null && PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId) && info.Type == MyDamageType.Explosion)
                    zone1 = true;
                if (mySlimBlock != null && PVE.EntitiesInZone2.Contains(mySlimBlock.CubeGrid.EntityId) && info.Type == MyDamageType.Explosion)
                    zone2 = true;

                if (zone1 || zone2)
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                    return;
                }
            }
            else
            {
                if (mySlimBlock != null && PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId) && info.Type == MyDamageType.Explosion)
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                    return;
                }
            }

            if (MyEntities.TryGetEntityById(info.AttackerId, out var AttackerEntity, allowClosed: true))
            {
                if (AttackerEntity is MyVoxelBase)
                    return;

                if (mySlimBlock != null && mySlimBlock.CubeGrid.DisplayName.Contains("Container MK-"))
                    return;

                if (AttackerEntity is MyAngleGrinder myAngleGrinder)
                {
                    num1 = myAngleGrinder.OwnerIdentityId;
                }

                if (AttackerEntity is MyHandDrill myHandDrill)
                {
                    num1 = myHandDrill.OwnerIdentityId;
                }

                if (AttackerEntity is MyAutomaticRifleGun myAutomaticRifleGun)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone1 = true;
                        if (PVE.PVESphere2.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            return;
                    }
                    num1 = myAutomaticRifleGun.OwnerIdentityId;
                }

                if (AttackerEntity is MyEngineerToolBase toolBase)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone1 = true;
                        if (PVE.PVESphere2.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            return;
                    }
                    num1 = toolBase.OwnerIdentityId;
                }

                if ((AttackerEntity as MyUserControllableGun) != null)
                    num1 = (AttackerEntity as MyUserControllableGun).OwnerId;

                if ((AttackerEntity as MyCubeGrid) != null)
                    num1 = ((AttackerEntity as MyCubeGrid).BigOwners.Count > 0) ? (AttackerEntity as MyCubeGrid).BigOwners[0] : 0L;

                if ((AttackerEntity as MyLargeTurretBase) != null)
                    num1 = (AttackerEntity as MyLargeTurretBase).OwnerId;

                if (AttackerEntity is MyCharacter character)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone1 = true;
                        if (PVE.PVESphere2.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) != ContainmentType.Contains)
                            return;
                    }
                    num1 = character.GetPlayerIdentityId();
                }

                if ((AttackerEntity as MyCubeGrid) != null)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (!PVE.EntitiesInZone.Contains((AttackerEntity as MyCubeGrid).EntityId))
                            zone1 = true;
                        if (!PVE.EntitiesInZone2.Contains((AttackerEntity as MyCubeGrid).EntityId))
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (!PVE.EntitiesInZone.Contains((AttackerEntity as MyCubeGrid).EntityId))
                            return;
                    }

                    if (mySlimBlock != null && mySlimBlock.CubeGrid.Physics != null && (info.Type == MyDamageType.Fall || info.Type == MyDamageType.Deformation))
                    {
                        var LinearVelocity = mySlimBlock.CubeGrid.Physics.LinearVelocity.Length();
                        var AngularVelocity = mySlimBlock.CubeGrid.Physics.AngularVelocity.Length();

                        if (mySlimBlock.CubeGrid.Physics != null && (LinearVelocity > 30 || AngularVelocity > 30))
                            num3 = 1f;
                    }
                }

                if ((AttackerEntity as MyUserControllableGun) != null)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (!PVE.EntitiesInZone.Contains((AttackerEntity as MyUserControllableGun).CubeGrid.EntityId))
                            zone1 = true;
                        if (!PVE.EntitiesInZone2.Contains((AttackerEntity as MyUserControllableGun).CubeGrid.EntityId))
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (!PVE.EntitiesInZone.Contains((AttackerEntity as MyUserControllableGun).CubeGrid.EntityId))
                            return;
                    }
                }

                if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                {
                    var zone1 = false;
                    var zone2 = false;
                    if ((AttackerEntity as MyLargeTurretBase) != null && !PVE.EntitiesInZone.Contains((AttackerEntity as MyLargeTurretBase).CubeGrid.EntityId) ||
                        (AttackerEntity as MyWarhead) != null && !PVE.EntitiesInZone.Contains((AttackerEntity as MyWarhead).CubeGrid.EntityId))
                        zone1 = true;

                    if ((AttackerEntity as MyLargeTurretBase) != null && !PVE.EntitiesInZone2.Contains((AttackerEntity as MyLargeTurretBase).CubeGrid.EntityId) ||
                        (AttackerEntity as MyWarhead) != null && !PVE.EntitiesInZone2.Contains((AttackerEntity as MyWarhead).CubeGrid.EntityId))
                        zone2 = true;

                    if (zone1 && zone2)
                        return;
                }
                else
                {
                    if ((AttackerEntity as MyLargeTurretBase) != null && !PVE.EntitiesInZone.Contains((AttackerEntity as MyLargeTurretBase).CubeGrid.EntityId) ||
                        (AttackerEntity as MyWarhead) != null && !PVE.EntitiesInZone.Contains((AttackerEntity as MyWarhead).CubeGrid.EntityId))
                        return;
                }
            }

            if (num1 == 0L)
            {
                info.Amount = 0f;
                info.IsDeformation = false;
            }
            else
            {
                var steamId1 = MySession.Static.Players.TryGetSteamId(num1);
                var steamId2 = MySession.Static.Players.TryGetSteamId(num2);
                if (!MySession.Static.Players.IdentityIsNpc(num1) && num2 != 0L &&
                    !MySession.Static.Players.IdentityIsNpc(num2) && num2 != info.AttackerId && steamId1 != steamId2 &&
                    MySession.Static.Factions.TryGetPlayerFaction(num1) != MySession.Static.Factions.TryGetPlayerFaction(num2))
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                }
            }
            if (num3 == 1f)
            {
                info.Amount = 0.5f;
                info.IsDeformation = false;
            }
        }
    }
}
