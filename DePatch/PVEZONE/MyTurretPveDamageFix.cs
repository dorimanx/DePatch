using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DePatch.CoolDown;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRageMath;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyTurretPveDamageFix
    {
        private static readonly SteamIdCooldownKey LoopOnBootRequestID = new SteamIdCooldownKey(76000000000000001);
        private static bool ServerBoot = true;
        private static MyCubeGrid LockingGrid;
        private static bool LockingGridHasOwner = false;
        public static bool BootTickStarted = false;

        private static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MySessionComponentSafeZones).GetMethod("IsActionAllowed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null,
                new Type[4]
                {
                    typeof(MyEntity),
                    typeof(MySafeZoneAction),
                    typeof(long),
                    typeof(ulong)
                }, new ParameterModifier[0])).
                Prefixes.Add(typeof(MyTurretPveDamageFix).GetMethod(nameof(IsActionAllowedPatch), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

        private static bool CheckAllowedToLock(MyCubeGrid Grid)
        {
            if (Grid.BigOwners.Count > 0 && LockingGrid.BigOwners.FirstOrDefault() == Grid.BigOwners.FirstOrDefault())
                return true;

            if (Grid.BigOwners.Count > 0 && MySession.Static.Players.IdentityIsNpc(Grid.BigOwners.FirstOrDefault()))
                return true;

            var GridFactionID = MySession.Static.Factions.TryGetPlayerFaction(Grid.BigOwners.FirstOrDefault());
            var LockingGridFaction = MySession.Static.Factions.TryGetPlayerFaction(LockingGrid.BigOwners.FirstOrDefault());

            if (LockingGridHasOwner)
            {
                if (GridFactionID != null && LockingGridFaction != null)
                {
                    var FactionsRelationship = MySession.Static.Factions.GetRelationBetweenFactions(GridFactionID.FactionId, LockingGridFaction.FactionId);

                    if (LockingGridFaction.FactionId == GridFactionID.FactionId)
                        return true;

                    switch (FactionsRelationship.Item1)
                    {
                        case MyRelationsBetweenFactions.Neutral:
                        case MyRelationsBetweenFactions.Friends:
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool IsActionAllowedPatch(MyEntity entity, MySafeZoneAction action, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (entity is MyCubeGrid grid)
            {
                switch (action)
                {
                    case MySafeZoneAction.LandingGearLock:
                        {
                            if (!DePatchPlugin.Instance.Config.NoEnemyPlayerLandingGearLocks)
                                return true;

                            // allow grab with landing gears in PVE zone, if found will return false and here true.
                            if (DePatchPlugin.Instance.Config.PveZoneEnabled && !PVE.CheckEntityInZone(entity, ref __result))
                                return true;

                            var LockingBondingBox = entity.PositionComp.LocalAABB;
                            var rad = (LockingBondingBox.Center - LockingBondingBox.Min).Length();
                            rad = (float)(rad * 0.7);
                            BoundingBoxD ExtendedBox = new BoundingSphereD(LockingBondingBox.Center, rad).GetBoundingBox();
                            var GridBox = new MyOrientedBoundingBoxD(ExtendedBox, entity.WorldMatrix);
                            var Entities = new List<MyEntity>();
                            var CleanList = new List<MyEntity>();
                            var AttachedFilterList = new List<MyEntity>();
                            var ExcludeSubGrids = new List<MyEntity>();
                            MyGamePruningStructure.GetAllEntitiesInOBB(ref GridBox, Entities);

                            // check if any voxels or planets are in list, if yes, just lock.
                            if (Entities.OfType<MyVoxelMap>().Count() > 0 || Entities.OfType<MyPlanet>().Count() > 0)
                                return true;

                            // find the locking grid first.
                            foreach (var LockingGear in Entities.OfType<MyCubeGrid>())
                            {
                                if (entity.EntityId == LockingGear.EntityId)
                                    LockingGrid = LockingGear;
                            }

                            // no owner or nobody cant lock
                            if (LockingGrid != null && LockingGrid.BigOwners.Count > 0)
                                LockingGridHasOwner = true;
                            else
                                return false;

                            // if locking grid is NPC and have less than 30 blocks deny lock.
                            if (LockingGridHasOwner && MySession.Static.Players.IdentityIsNpc(LockingGrid.BigOwners.FirstOrDefault()) && LockingGrid.BlocksCount < 30)
                                return false;

                            // add new list without the locking grid and keep filtering.
                            foreach (var CopyList in Entities.OfType<MyCubeGrid>())
                            {
                                if (LockingGrid.EntityId == CopyList.EntityId)
                                    continue;

                                CleanList.Add(CopyList);
                            }

                            // check attached to locking grid other grids and filer them out from list.
                            var AttachedList = LockingGrid.GridSystems.LandingSystem.GetAttachedEntities();
                            if (AttachedList != null && AttachedList.Count > 0)
                            {
                                foreach (var AlreadyAttached in AttachedList)
                                {
                                    if (AlreadyAttached != null && CleanList.OfType<MyCubeGrid>().Contains((MyEntity)AlreadyAttached))
                                        CleanList.Remove((MyEntity)AlreadyAttached);
                                }
                            }

                            // check and filer all attached to locking grid other grids from list.
                            if (CleanList.Count > 1)
                            {
                                foreach (var AttachedFilter in CleanList.OfType<MyCubeGrid>())
                                {
                                    var AnyAttached = AttachedFilter.GridSystems.LandingSystem.GetAttachedEntities();

                                    if (AnyAttached != null && AnyAttached.OfType<MyCubeGrid>().Contains(LockingGrid))
                                        AttachedFilterList.Add(AttachedFilter);
                                }

                                // if found other grid locked on main locking grid, remove it from list.
                                if (AttachedFilterList != null && AttachedFilterList.Count > 0)
                                {
                                    foreach (var CleanUpA in AttachedFilterList.OfType<MyCubeGrid>())
                                    {
                                        if (CleanUpA != null && CleanList.OfType<MyCubeGrid>().Contains(CleanUpA))
                                            CleanList.Remove(CleanUpA);
                                    }
                                }
                            }

                            // remove all Rotor subgrids from list if they are on locking grid.
                            if (CleanList.Count > 1)
                            {
                                var AttachedRotorSubGrids = LockingGrid.GridSystems.TerminalSystem.Blocks.OfType<MyMotorStator>();
                                if (AttachedRotorSubGrids != null)
                                {
                                    foreach (var SubGridBlock in AttachedRotorSubGrids)
                                    {
                                        if (SubGridBlock != null & SubGridBlock.Rotor != null && CleanList.OfType<MyCubeGrid>().Contains(SubGridBlock.Rotor.CubeGrid))
                                            CleanList.Remove(SubGridBlock.Rotor.CubeGrid);
                                    }
                                }
                            }

                            // remove all Piston subgrids from list if they are on locking grid.
                            if (CleanList.Count > 1)
                            {
                                var AttachedPistonSubGrids = LockingGrid.GridSystems.TerminalSystem.Blocks.OfType<MyExtendedPistonBase>();
                                if (AttachedPistonSubGrids != null)
                                {
                                    foreach (var SubGridBlock in AttachedPistonSubGrids)
                                    {
                                        if (SubGridBlock != null && SubGridBlock.TopGrid != null && CleanList.OfType<MyCubeGrid>().Contains(SubGridBlock.TopGrid))
                                            CleanList.Remove(SubGridBlock.TopGrid);
                                    }
                                }
                            }

                            // remove all Connected With connector subgrids from list if they are on locking grid.
                            if (CleanList.Count > 1)
                            {
                                var ConnectedSubGrids = LockingGrid.GridSystems.TerminalSystem.Blocks.OfType<MyShipConnector>();
                                if (ConnectedSubGrids != null)
                                {
                                    foreach (var SubGridBlock in ConnectedSubGrids)
                                    {
                                        if (SubGridBlock != null && SubGridBlock.Other != null && CleanList.OfType<MyCubeGrid>().Contains(SubGridBlock.Other.CubeGrid))
                                            CleanList.Remove(SubGridBlock.Other.CubeGrid);
                                    }
                                }
                            }

                            // remove all subgrids from other grids in list.
                            if (CleanList.Count > 1)
                            {
                                foreach (var CleanUpSubsA in CleanList.OfType<MyCubeGrid>())
                                {
                                    foreach (var CheckAllBlocks in CleanUpSubsA.GridSystems.TerminalSystem.Blocks.OfType<MyMotorStator>())
                                    {
                                        if (CheckAllBlocks != null && CheckAllBlocks.Rotor != null && CleanList.OfType<MyCubeGrid>().Contains(CheckAllBlocks.Rotor.CubeGrid))
                                            ExcludeSubGrids.Add(CheckAllBlocks.Rotor.CubeGrid);
                                    }
                                    foreach (var CheckAllBlocks in CleanUpSubsA.GridSystems.TerminalSystem.Blocks.OfType<MyExtendedPistonBase>())
                                    {
                                        if (CheckAllBlocks != null && CheckAllBlocks.TopGrid != null && CleanList.OfType<MyCubeGrid>().Contains(CheckAllBlocks.TopGrid))
                                            ExcludeSubGrids.Add(CheckAllBlocks.TopGrid);
                                    }
                                    foreach (var CheckAllBlocks in CleanUpSubsA.GridSystems.TerminalSystem.Blocks.OfType<MyShipConnector>())
                                    {
                                        if (CheckAllBlocks != null && CheckAllBlocks.Other != null && CleanList.OfType<MyCubeGrid>().Contains(CheckAllBlocks.Other.CubeGrid))
                                            ExcludeSubGrids.Add(CheckAllBlocks.Other.CubeGrid);
                                    }
                                }

                                if (ExcludeSubGrids != null && ExcludeSubGrids.Count > 0)
                                {
                                    foreach (var CleanSubsB in ExcludeSubGrids.OfType<MyCubeGrid>())
                                    {
                                        if (CleanSubsB != null && CleanList.OfType<MyCubeGrid>().Contains(CleanSubsB))
                                        {
                                            CleanList.Remove(CleanSubsB);

                                            if (!CheckAllowedToLock(CleanSubsB))
                                            {
                                                CleanList.Clear();
                                                return false;
                                            }
                                        }
                                    }
                                    if (CleanList.Count > 1)
                                        return true;
                                }
                            }

                            // Simple 1 grid locking to other grid checks.
                            if (CleanList.Count == 1)
                            {
                                if (CleanList.OfType<MyCubeGrid>().FirstOrDefault().BigOwners.Count > 0 && LockingGrid.BigOwners.FirstOrDefault() == CleanList.OfType<MyCubeGrid>().FirstOrDefault().BigOwners.FirstOrDefault())
                                {
                                    CleanList.Clear();
                                    return true;
                                }

                                if (CleanList.OfType<MyCubeGrid>().FirstOrDefault().BigOwners.Count > 0 && MySession.Static.Players.IdentityIsNpc(CleanList.OfType<MyCubeGrid>().FirstOrDefault().BigOwners.FirstOrDefault()))
                                {
                                    CleanList.Clear();
                                    return true;
                                }

                                var GridFactionID = MySession.Static.Factions.TryGetPlayerFaction(CleanList.OfType<MyCubeGrid>().FirstOrDefault().BigOwners.FirstOrDefault());
                                var LockingGridFaction = MySession.Static.Factions.TryGetPlayerFaction(LockingGrid.BigOwners.FirstOrDefault());

                                if (LockingGridHasOwner)
                                {
                                    if (GridFactionID != null && LockingGridFaction != null)
                                    {
                                        var FactionsRelationship = MySession.Static.Factions.GetRelationBetweenFactions(GridFactionID.FactionId, LockingGridFaction.FactionId);

                                        if (LockingGridFaction.FactionId == GridFactionID.FactionId)
                                        {
                                            CleanList.Clear();
                                            return true;
                                        }

                                        switch (FactionsRelationship.Item1)
                                        {
                                            case MyRelationsBetweenFactions.Neutral:
                                            case MyRelationsBetweenFactions.Friends:
                                                CleanList.Clear();
                                                return true;
                                        }
                                    }
                                }
                                CleanList.Clear();
                                return false;
                            }
                            CleanList.Clear();
                            return false;
                        }
                    case MySafeZoneAction.Building:
                        {
                            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                                return true;

                            if (user != 0UL && MySession.Static.IsUserAdmin(user))
                                return true;

                            var myPlayerID = MySession.Static.Players.TryGetIdentityId(user);
                            if (myPlayerID == 0 || myPlayerID < 0)
                                return true;

                            var myPlayerBuilding = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == myPlayerID);

                            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                            {
                                if (entity is MyWelder Welder) return true;

                                // if found will return false this why !PVE.CheckEntityInZone
                                if (myPlayerBuilding != null && !PVE.CheckEntityInZone(myPlayerBuilding, ref __result))
                                {
                                    if (grid.IsFriendlyPlayer(user)) return true;

                                    return false;
                                }
                            }
                            else if (myPlayerBuilding != null && PVE.PVESphere.Contains(myPlayerBuilding.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            {
                                if (entity is MyWelder Welder) return true;

                                if (grid.IsFriendlyPlayer(user)) return true;

                                return false;
                            }
                            return true;
                        }
                    case MySafeZoneAction.Shooting:
                        {
                            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
                            {
                                if (ServerBoot)
                                {
                                    // start is in assembler update 100, if no asseblers in world then skip here.
                                    if (!BootTickStarted)
                                        goto skipweaponblock;

                                    // loop for X sec after boot to block weapons.
                                    _ = CooldownManager.CheckCooldown(LoopOnBootRequestID, null, out var remainingSecondsBoot);

                                    if (remainingSecondsBoot < 3)
                                        ServerBoot = false;

                                    if (entity is MyShipDrill || entity is MyShipToolBase)
                                        return true;

                                    // block weapons
                                    return false;
                                }
                            }

                            skipweaponblock:

                            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                                return true;

                            // if found will return false
                            if (!PVE.CheckEntityInZone(entity, ref __result))
                            {
                                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                                    return true;

                                return false;
                            }
                            return true;
                        }
                }
            }

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            if (action != MySafeZoneAction.Shooting)
            {
                __result = true;
                return true;
            }

            if (entity is MyCharacter character)
            {
                if (character == null)
                    return true;

                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                    return true;

                var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == character.GetPlayerIdentityId());

                // if found will return false
                if (myPlayer != null && DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    return PVE.CheckEntityInZone(myPlayer, ref __result);

                if (myPlayer != null && PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    return false;
            }

            __result = true;
            return true;
        }
    }
}
