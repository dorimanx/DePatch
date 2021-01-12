using System.Linq;
using DePatch.CoolDown;
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

        private static readonly SteamIdCooldownKey LoopRequestID = new SteamIdCooldownKey(76000000000000004);
        private static readonly int LoopCooldown = 180 * 1000;
        private static bool ServerBoot = true;
        private static bool ServerBootLoopStart = true;

        public static void Init()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return;

            if (_init)
                return;

            _init = true;
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, ProcessDamage);
        }

        private static void ProcessDamage(object target, ref MyDamageInformation info)
        {
            var num1 = info.AttackerId;
            var mySlimBlock = target as MySlimBlock;
            long num2;
            long num4 = 10L;
            var num3 = 10f;

            if (ServerBoot)
            {
                if (ServerBootLoopStart)
                {
                    CooldownManager.StartCooldown(LoopRequestID, null, LoopCooldown);
                    ServerBootLoopStart = false;
                }

                // loop for 180 sec after boot to block weapons.
                if (CooldownManager.CheckCooldown(LoopRequestID, null, out long remainingSecondsBoot))
                {
                }

                if (remainingSecondsBoot < 2)
                    ServerBoot = false;

                // block weapons
                info.Amount = 0f;
                info.IsDeformation = false;

                return;
            }

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

                if (AttackerEntity is MyUserControllableGun myUserControllableGun)
                    num1 = myUserControllableGun.OwnerId;

                if (AttackerEntity is MyCubeGrid myCubeGrid)
                    num1 = (myCubeGrid.BigOwners.Count > 0) ? myCubeGrid.BigOwners[0] : 0L;

                if (AttackerEntity is MyLargeTurretBase myLargeTurretBase)
                    num1 = myLargeTurretBase.OwnerId;

                if (AttackerEntity is MyShipToolBase myShipToolBase)
                    num1 = myShipToolBase.OwnerId;

                if (AttackerEntity is MyThrust myThrust)
                    num4 = myThrust.CubeGrid.BigOwners[0];

                if (AttackerEntity is MyCharacter character && character != null)
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

                if (AttackerEntity is MyCubeGrid myCubeGridZone)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (!PVE.EntitiesInZone.Contains(myCubeGridZone.EntityId))
                            zone1 = true;
                        if (!PVE.EntitiesInZone2.Contains(myCubeGridZone.EntityId))
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (!PVE.EntitiesInZone.Contains(myCubeGridZone.EntityId))
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

                if (AttackerEntity is MyUserControllableGun myUserControllableGunZone)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (!PVE.EntitiesInZone.Contains(myUserControllableGunZone.CubeGrid.EntityId))
                            zone1 = true;
                        if (!PVE.EntitiesInZone2.Contains(myUserControllableGunZone.CubeGrid.EntityId))
                            zone2 = true;

                        if (zone1 && zone2)
                            return;
                    }
                    else
                    {
                        if (!PVE.EntitiesInZone.Contains(myUserControllableGunZone.CubeGrid.EntityId))
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

            if (num4 != 10L && info.Type == MyDamageType.Environment)
            {
                // burn no owner
                if (num2 == 0L)
                    return;

                // Allow damage from NPC thrusters
                if (MySession.Static.Players.IdentityIsNpc(num4) || MySession.Static.Players.IdentityIsNpc(num2))
                    return;

                // allow thruster damage in PVE zone. or there are sync issues.
                var steamId1 = MySession.Static.Players.TryGetSteamId(num4);
                var steamId2 = MySession.Static.Players.TryGetSteamId(num2);
                var Thrusterfaction = MySession.Static.Factions.TryGetPlayerFaction(num4);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(num2);

                // burn owned blocks to prevent DySync
                if ((steamId1 != 0L && steamId2 != 0L && (steamId1 == steamId2)) || (Thrusterfaction != null && gridFaction != null && (Thrusterfaction == gridFaction)))
                    return;

                // Prevent attack with thrusters as weapon in PVE, Nigate Damage with DySync.
                info.Amount = 0f;
                info.IsDeformation = false;

                return;
            }

            if (num1 == 0L)
            {
                // grind no owner
                if (num2 == 0L)
                    return;

                info.Amount = 0f;
                info.IsDeformation = false;

                // grid to grid ram on high speed, allow small amount of damage.
                if (num3 == 1f)
                {
                    info.Amount = 0.3f;
                    info.IsDeformation = false;
                }
            }
            else
            {
                var steamId1 = MySession.Static.Players.TryGetSteamId(num1);
                var steamId2 = MySession.Static.Players.TryGetSteamId(num2);
                var Playerfaction = MySession.Static.Factions.TryGetPlayerFaction(num1);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(num2);

                // grind no owner
                if (num2 == 0L)
                    return;

                // self damage
                if (num2 == info.AttackerId)
                    return;

                // grid to grid ram on high speed, allow small amount of damage.
                if (num3 == 1f)
                {
                    info.Amount = 0.3f;
                    info.IsDeformation = false;
                }

                // allow damage to and from NPC!
                if ((num1 != 0L && MySession.Static.Players.IdentityIsNpc(num1)) || (num2 != 0L && MySession.Static.Players.IdentityIsNpc(num2)))
                    return;

                // allow damage if owned grid or faction owner.
                if ((steamId1 != 0L && steamId2 != 0L && (steamId1 == steamId2)) || (Playerfaction != null && gridFaction != null && (Playerfaction == gridFaction)))
                    return;

                info.Amount = 0f;
                info.IsDeformation = false;

                // grid to grid ram on high speed, allow small amount of damage.
                if (num3 == 1f)
                {
                    info.Amount = 0.3f;
                    info.IsDeformation = false;
                }
            }
        }
    }
}
