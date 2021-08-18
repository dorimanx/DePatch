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
                return;

            if (_init)
                return;

            _init = true;

            if (MyAPIGateway.Session != null)
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, ProcessDamage);
        }

        private static void ProcessDamage(object target, ref MyDamageInformation info)
        {
            if (target == null)
                return;

            if (info.Amount == 0)
            {
                info.IsDeformation = false;
                return;
            }

            var AttackerId = info.AttackerId;
            var mySlimBlock = target as MySlimBlock;
            long UnderAttackId = 0L;
            long ThrusterDamageId = 10L;
            var RammingGrid = false;
            var zone1 = false;
            var zone2 = false;
            var OnlinePlayersList = MySession.Static.Players.GetOnlinePlayers().ToList();

            if (OnlinePlayersList.Count == 0)
                return;

            if (mySlimBlock == null)
            {
                if (target is MyCharacter)
                    return;
            }
            else
            {
                if (mySlimBlock.CubeGrid.DisplayName.Contains("Container MK-"))
                    return;

                if (PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId))
                    zone1 = true;
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(mySlimBlock.CubeGrid.EntityId))
                    zone2 = true;

                // if grid is not in PVE zones just skip here.
                if (!zone1 && !zone2)
                    return;

                UnderAttackId = ((mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L);

                /* Warhead Protection inside PVE Zones */
                zone1 = false;
                zone2 = false;
                if (PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId) && info.Type == MyDamageType.Explosion)
                    zone1 = true;
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(mySlimBlock.CubeGrid.EntityId) && info.Type == MyDamageType.Explosion)
                    zone2 = true;

                if (zone1 || zone2)
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

                if (AttackerEntity is MyAngleGrinder myAngleGrinder && myAngleGrinder != null)
                    AttackerId = myAngleGrinder.OwnerIdentityId;

                if (AttackerEntity is MyHandDrill myHandDrill && myHandDrill != null)
                    AttackerId = myHandDrill.OwnerIdentityId;

                if (AttackerEntity is MyAutomaticRifleGun myAutomaticRifleGun && myAutomaticRifleGun != null)
                {
                    if (myAutomaticRifleGun.OwnerIdentityId == 0)
                        return;

                    zone1 = false;
                    zone2 = false;
                    if (PVE.PVESphere.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone1 = true;

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;

                    AttackerId = myAutomaticRifleGun.OwnerIdentityId;
                }

                if (AttackerEntity is MyEngineerToolBase toolBase && toolBase != null)
                {
                    if (toolBase.OwnerIdentityId == 0)
                        return;

                    zone1 = false;
                    zone2 = false;
                    if (PVE.PVESphere.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == toolBase.OwnerIdentityId).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;

                    AttackerId = toolBase.OwnerIdentityId;
                }

                if (AttackerEntity is MyUserControllableGun myUserControllableGun && myUserControllableGun != null)
                    AttackerId = myUserControllableGun.OwnerId;

                if (AttackerEntity is MyCubeGrid myCubeGrid && myCubeGrid != null)
                    AttackerId = (myCubeGrid.BigOwners.Count > 0) ? myCubeGrid.BigOwners[0] : 0L;

                if (AttackerEntity is MyLargeTurretBase myLargeTurretBase && myLargeTurretBase != null)
                    AttackerId = myLargeTurretBase.OwnerId;

                if (AttackerEntity is MyShipToolBase myShipToolBase && myShipToolBase != null)
                    AttackerId = myShipToolBase.OwnerId;

                if (AttackerEntity is MyThrust myThrust && myThrust.CubeGrid != null)
                    ThrusterDamageId = (myThrust.CubeGrid.BigOwners.Count > 0) ? myThrust.CubeGrid.BigOwners[0] : 0L;

                if (AttackerEntity is MyCharacter character && character != null)
                {
                    if (character.GetPlayerIdentityId() == 0)
                        return;

                    zone1 = false;
                    zone2 = false;
                    if (PVE.PVESphere.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == character.GetPlayerIdentityId()).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;

                    AttackerId = character.GetPlayerIdentityId();
                }

                if (AttackerEntity is MyCubeGrid myCubeGridZone && myCubeGridZone != null)
                {
                    zone1 = false;
                    zone2 = false;
                    if (PVE.EntitiesInZone.Contains(myCubeGridZone.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(myCubeGridZone.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;

                    if (mySlimBlock != null && mySlimBlock.CubeGrid.Physics != null && (info.Type == MyDamageType.Fall || info.Type == MyDamageType.Deformation))
                    {
                        if (mySlimBlock.CubeGrid.Physics.LinearVelocity.Length() > 30 || mySlimBlock.CubeGrid.Physics.AngularVelocity.Length() > 30)
                            RammingGrid = true;
                    }
                }

                if (AttackerEntity is MyUserControllableGun myUserControllableGunZone && myUserControllableGunZone != null)
                {
                    zone1 = false;
                    zone2 = false;
                    if (PVE.EntitiesInZone.Contains(myUserControllableGunZone.CubeGrid.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(myUserControllableGunZone.CubeGrid.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;
                }

                if (AttackerEntity is MyLargeTurretBase Myturret && Myturret != null)
                {
                    zone1 = false;
                    zone2 = false;
                    if (PVE.EntitiesInZone.Contains(Myturret.CubeGrid.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(Myturret.CubeGrid.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;
                }

                if (AttackerEntity is MyWarhead WarheadBlock && WarheadBlock != null)
                {
                    zone1 = false;
                    zone2 = false;
                    if (PVE.EntitiesInZone.Contains(WarheadBlock.CubeGrid.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(WarheadBlock.CubeGrid.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return;
                }
            }

            if (ThrusterDamageId != 10L && info.Type == MyDamageType.Environment)
            {
                // burn no owner
                if (UnderAttackId == 0L)
                    return;

                // Allow damage from NPC thrusters
                if (MySession.Static.Players.IdentityIsNpc(ThrusterDamageId) || MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                    return;

                // allow thruster damage in PVE zone. or there are sync issues.
                var steamId1 = MySession.Static.Players.TryGetSteamId(ThrusterDamageId);
                var steamId2 = MySession.Static.Players.TryGetSteamId(UnderAttackId);
                var Thrusterfaction = MySession.Static.Factions.TryGetPlayerFaction(ThrusterDamageId);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(UnderAttackId);

                // burn owned blocks to prevent DySync
                if ((steamId1 != 0L && steamId2 != 0L && (steamId1 == steamId2)) || (Thrusterfaction != null && gridFaction != null && (Thrusterfaction == gridFaction)))
                    return;

                // Prevent attack with thrusters as weapon in PVE, Nigate Damage with DySync.
                info.Amount = 0f;
                info.IsDeformation = false;

                return;
            }

            if (AttackerId == 0L)
            {
                // grind no owner
                if (UnderAttackId == 0L)
                    return;

                // grid to grid ram on high speed, allow small amount of damage.
                if (RammingGrid)
                {
                    if (info.Amount > 0.3f)
                        info.Amount = 0.2f;
                    info.IsDeformation = false;
                }
                else
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                }
            }
            else
            {
                // grind no owner or self damage
                if (UnderAttackId == 0L || UnderAttackId == AttackerId)
                    return;

                // allow damage to and from NPC!
                if (MySession.Static.Players.IdentityIsNpc(AttackerId) || MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                    return;

                var steamId1 = MySession.Static.Players.TryGetSteamId(AttackerId);
                var steamId2 = MySession.Static.Players.TryGetSteamId(UnderAttackId);
                var Playerfaction = MySession.Static.Factions.TryGetPlayerFaction(AttackerId);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(UnderAttackId);

                // allow damage if owned grid or faction owner.
                if ((steamId1 != 0UL && steamId2 != 0UL && (steamId1 == steamId2)) || (Playerfaction != null && gridFaction != null && (Playerfaction.Tag == gridFaction.Tag)))
                    return;

                // grid to grid ramming on high speed, allow small amount of damage.
                if (RammingGrid)
                {
                    if (info.Amount > 0.3f)
                        info.Amount = 0.2f;
                    info.IsDeformation = false;
                }
                else
                {
                    info.Amount = 0f;
                    info.IsDeformation = false;
                }
            }
        }
    }
}
