using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace DePatch.PVEZONE
{
    public static class DamageHandler
    {
        private static bool _init;

        public static void Patch(PatchContext ctx)
        {
            if (DePatchPlugin.Instance.Config.Enabled && DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                ctx.Prefix(typeof(MySlimBlock), typeof(DamageHandler), nameof(DoDamage));

                if (_init)
                    return;

                if (MyAPIGateway.Session != null)
                {
                    _init = true;
                    MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, ProcessDamage);
                }
            }
        }

        public static bool DoDamage(MySlimBlock __instance, ref float damage, MyStringHash damageType,
                                    bool sync, MyHitInfo? hitInfo, long attackerId, long realHitEntityId = 0, bool shouldDetonateAmmo = true)
        {
            if (__instance == null || !DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            // if no damage detected, then just mone on.
            if (damage <= 0)
                return true;

            var AttackerId = attackerId;
            var mySlimBlock = __instance;
            long UnderAttackId = 0L;
            long ThrusterDamageId = 10L;
            var RammingGrid = false;
            var zone1 = false;
            var zone2 = false;
            var OnlinePlayersList = MySession.Static.Players.GetOnlinePlayers().ToList();

            if (OnlinePlayersList.Count == 0 || mySlimBlock.CubeGrid == null)
                return false;

            if (mySlimBlock.CubeGrid.DisplayName.Contains("Container MK-"))
                return true;

            if (PVE.EntitiesInZone.Contains(mySlimBlock.CubeGrid.EntityId))
                zone1 = true;
            if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(mySlimBlock.CubeGrid.EntityId))
                zone2 = true;

            // if grid is not in PVE zones just skip here.
            if (!zone1 && !zone2)
                return true;

            UnderAttackId = (mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L;

            if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
            {
                if (attackerId != 0 && UnderAttackId != 0)
                {
                    // Allow damage from NPC to NPC
                    if (MySession.Static.Players.IdentityIsNpc(AttackerId) && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                        return true;

                    // Allow damage from Players to NPC
                    if (!MySession.Static.Players.IdentityIsNpc(AttackerId) && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                        return true;

                    // Allow damage from NPC to Players
                    if (!MySession.Static.Players.IdentityIsNpc(UnderAttackId) && MySession.Static.Players.IdentityIsNpc(AttackerId))
                        return true;
                }
            }

            if (MyEntities.TryGetEntityById(AttackerId, out var AttackerEntity, allowClosed: true))
            {
                if (AttackerEntity is MyVoxelBase)
                    return true;

                if (AttackerEntity is MyAutomaticRifleGun myAutomaticRifleGun && myAutomaticRifleGun != null)
                {
                    if (myAutomaticRifleGun.OwnerIdentityId == 0)
                        return true;

                    zone1 = false;
                    zone2 = false;

                    if (PVE.PVESphere.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                        zone1 = true;

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == myAutomaticRifleGun.OwnerIdentityId).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return true;

                    AttackerId = myAutomaticRifleGun.OwnerIdentityId;
                }

                if (AttackerEntity is MyUserControllableGun myUserControllableGun && myUserControllableGun != null)
                    AttackerId = myUserControllableGun.OwnerId;

                if (AttackerEntity is MyCubeGrid myCubeGrid && myCubeGrid != null)
                    AttackerId = (myCubeGrid.BigOwners.Count > 0) ? myCubeGrid.BigOwners[0] : 0L;

                if (AttackerEntity is MyLargeTurretBase myLargeTurretBase && myLargeTurretBase != null)
                    AttackerId = myLargeTurretBase.OwnerId;

                if (AttackerEntity is MyThrust myThrust && myThrust.CubeGrid != null)
                    ThrusterDamageId = (myThrust.CubeGrid.BigOwners.Count > 0) ? myThrust.CubeGrid.BigOwners[0] : 0L;

                if (AttackerEntity is MyCharacter character && character != null)
                {
                    if (character.GetPlayerIdentityId() == 0)
                        return true;

                    zone1 = false;
                    zone2 = false;

                    if (PVE.PVESphere.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == character.GetPlayerIdentityId()).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                            b.Identity.IdentityId == character.GetPlayerIdentityId()).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                        zone2 = true;

                    if (!zone1 && !zone2)
                        return true;

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
                        return true;

                    if (mySlimBlock.CubeGrid.Physics != null && (damageType == MyDamageType.Fall || damageType == MyDamageType.Deformation))
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
                        return true;
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
                        return true;
                }
            }

            if (ThrusterDamageId != 10L && damageType == MyDamageType.Environment)
            {
                // burn no owner
                if (UnderAttackId == 0L)
                    return true;

                // Allow damage from NPC thrusters
                if (MySession.Static.Players.IdentityIsNpc(ThrusterDamageId) || MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                    return true;

                // allow thruster damage in PVE zone. or there are sync issues.
                var steamId1 = MySession.Static.Players.TryGetSteamId(ThrusterDamageId);
                var steamId2 = MySession.Static.Players.TryGetSteamId(UnderAttackId);
                var Thrusterfaction = MySession.Static.Factions.TryGetPlayerFaction(ThrusterDamageId);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(UnderAttackId);

                // burn owned blocks to prevent DySync
                if ((steamId1 != 0L && steamId2 != 0L && (steamId1 == steamId2)) || (Thrusterfaction != null && gridFaction != null && (Thrusterfaction == gridFaction)))
                    return true;

                // Prevent attack with thrusters as weapon in PVE, Nigate Damage with DySync.
                damage = 0f;

                return false;
            }

            if (AttackerId == 0L)
            {
                // grind no owner
                if (UnderAttackId == 0L)
                    return true;

                // grid to grid ram on high speed, allow small amount of damage.
                if (RammingGrid)
                {
                    if (damage > 0.3f)
                    {
                        damage = 0.01f;
                        return true;
                    }

                    if (mySlimBlock.CubeGrid.IsStatic)
                        damage = 0f;
                }
                else
                {
                    if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                        return true;
                    else
                    {
                        damage = 0f;
                        return false;
                    }
                }
            }
            else
            {
                // grind no owner or self damage
                if (UnderAttackId == 0L || UnderAttackId == AttackerId)
                    return true;

                // allow damage to and from NPC!
                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone && (MySession.Static.Players.IdentityIsNpc(AttackerId) || MySession.Static.Players.IdentityIsNpc(UnderAttackId)))
                    return true;

                var steamId1 = MySession.Static.Players.TryGetSteamId(AttackerId);
                var steamId2 = MySession.Static.Players.TryGetSteamId(UnderAttackId);
                var Playerfaction = MySession.Static.Factions.TryGetPlayerFaction(AttackerId);
                var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(UnderAttackId);

                // allow damage if owned grid or faction owner.
                if ((steamId1 != 0UL && steamId2 != 0UL && (steamId1 == steamId2)) || (Playerfaction != null && gridFaction != null && (Playerfaction.Tag == gridFaction.Tag)))
                    return true;

                // grid to grid ramming on high speed, allow small amount of damage.
                if (RammingGrid)
                {
                    if (damage > 0.3f && mySlimBlock.CubeGrid.IsStatic)
                    {
                        damage = 0f;
                        return false;
                    }
                    else
                    {
                        damage = 0.01f;
                        return true;
                    }
                }
            }

            // if no damage allow till now, then block damage in PVE.
            damage = 0f;

            return false;
        }

        private static void ProcessDamage(object target, ref MyDamageInformation info)
        {
            if (target == null)
                return;

            // if no damage detected, then just mone on.
            if (info.Amount == 0)
            {
                info.IsDeformation = false;
                return;
            }

            var AttackerId = info.AttackerId;
            var mySlimBlock = target as MySlimBlock;
            long UnderAttackId = 0L;
            var RammingGrid = false;
            var zone1 = false;
            var zone2 = false;
            var OnlinePlayersList = MySession.Static.Players.GetOnlinePlayers().ToList();

            if (OnlinePlayersList.Count == 0)
                return;

            if (target is MyCharacter || mySlimBlock == null)
                return;

            if (info.Type == MyDamageType.Grind || info.Type == MyDamageType.Drill || info.Type == MyDamageType.Explosion || info.Type == MyDamageType.Deformation)
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

                UnderAttackId = (mySlimBlock.CubeGrid.BigOwners.Count > 0) ? mySlimBlock.CubeGrid.BigOwners[0] : 0L;

                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                {
                    if (info.AttackerId != 0 && UnderAttackId != 0)
                    {
                        // Allow damage from NPC to NPC
                        if (MySession.Static.Players.IdentityIsNpc(AttackerId) && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                            return;

                        // Allow damage from Players to NPC
                        if (!MySession.Static.Players.IdentityIsNpc(AttackerId) && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                            return;

                        // Allow damage from NPC to Players
                        if (!MySession.Static.Players.IdentityIsNpc(UnderAttackId) && MySession.Static.Players.IdentityIsNpc(AttackerId))
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

                    if (AttackerEntity is MyEngineerToolBase toolBase && toolBase != null)
                    {
                        if (toolBase.OwnerIdentityId == 0)
                            return;

                        zone1 = false;
                        zone2 = false;

                        if (PVE.PVESphere.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                                b.Identity.IdentityId == toolBase.OwnerIdentityId).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                            zone1 = true;
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                                b.Identity.IdentityId == toolBase.OwnerIdentityId).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                            zone2 = true;

                        if (!zone1 && !zone2)
                            return;

                        AttackerId = toolBase.OwnerIdentityId;
                    }

                    if (AttackerEntity is MyShipToolBase myShipToolBase && myShipToolBase != null)
                        AttackerId = myShipToolBase.OwnerId;

                    if (AttackerEntity is MyCharacter character && character != null)
                    {
                        if (character.GetPlayerIdentityId() == 0)
                            return;

                        zone1 = false;
                        zone2 = false;

                        if (PVE.PVESphere.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                                b.Identity.IdentityId == character.GetPlayerIdentityId()).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                            zone1 = true;
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.PVESphere2.Contains((Vector3D)(OnlinePlayersList.Find((MyPlayer b) =>
                                b.Identity.IdentityId == character.GetPlayerIdentityId()).Character?.PositionComp.GetPosition())) == ContainmentType.Contains)
                            zone2 = true;

                        if (!zone1 && !zone2)
                            return;

                        AttackerId = character.GetPlayerIdentityId();
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

                        if (mySlimBlock?.CubeGrid?.Physics != null && (info.Type == MyDamageType.Fall || info.Type == MyDamageType.Deformation))
                        {
                            if (mySlimBlock.CubeGrid.Physics.LinearVelocity.Length() > 30 || mySlimBlock.CubeGrid.Physics.AngularVelocity.Length() > 30)
                                RammingGrid = true;
                        }
                    }
                }

                if (AttackerId == 0L)
                {
                    // grind no owner
                    if (UnderAttackId == 0L)
                        return;

                    // grid to grid ram on high speed, allow small amount of damage.
                    if (RammingGrid)
                    {
                        info.IsDeformation = false;

                        if (info.Amount > 0.3f)
                        {
                            info.Amount = 0.01f;
                            return;
                        }

                        if (mySlimBlock.CubeGrid.IsStatic)
                            info.Amount = 0f;
                    }
                    else
                    {
                        if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone && MySession.Static.Players.IdentityIsNpc(UnderAttackId))
                            return;
                        else
                        {
                            // Here damage type is Deformation, to save SIM and disallow to deform other player blocks, block deformation here.
                            info.IsDeformation = false;
                            info.Amount = 0f;
                            return;
                        }
                    }
                }
                else
                {
                    // grind no owner or self damage
                    if (UnderAttackId == 0L || UnderAttackId == AttackerId)
                        return;

                    // allow damage to and from NPC!
                    if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone && (MySession.Static.Players.IdentityIsNpc(AttackerId) || MySession.Static.Players.IdentityIsNpc(UnderAttackId)))
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
                        info.IsDeformation = false;

                        if (info.Amount > 0.3f)
                            info.Amount = 0.01f;

                        if (mySlimBlock != null && mySlimBlock.CubeGrid.IsStatic)
                            info.Amount = 0f;

                        return;
                    }

                    // Didnt found reason to allow damage in PVE
                    info.Amount = 0f;
                    info.IsDeformation = false;
                }
            }
        }
    }
}