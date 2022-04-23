using System;
using System.Collections.Generic;
using System.Linq;
using DePatch.CoolDown;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Components;
using VRageMath;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyPVESafeZoneAction
    {
        private static MyCubeGrid LockingGrid;
        private static bool LockingGridHasOwner = false;
        public static bool BootTickStarted = true;
        private static bool ServerBootLoopStart = true;

        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MySessionComponentSafeZones), "IsActionAllowed", typeof(MyPVESafeZoneAction), nameof(IsActionAllowedPatch), new[] { "entity", "action", "sourceEntityId", "user" });

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

        public static void UpdateBoot()
        {
            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
            {
                if (ServerBootLoopStart)
                {
                    if (DePatchPlugin.Instance.Config.DelayShootingOnBootTime <= 0)
                        DePatchPlugin.Instance.Config.DelayShootingOnBootTime = 1;

                    int LoopCooldown = DePatchPlugin.Instance.Config.DelayShootingOnBootTime * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, LoopCooldown);
                    ServerBootLoopStart = false;
                    BootTickStarted = true;
                }

                if (BootTickStarted)
                {
                    // loop for X sec after boot to block weapons.
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, out var remainingSecondsBoot);

                    if (remainingSecondsBoot < 1)
                        BootTickStarted = false;
                }
            }
        }

        private static bool IsActionAllowedPatch(MyEntity entity, MySafeZoneAction action, long sourceEntityId, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            var IsInPVEZone = false;
            __result = false;

            if (entity is MyCubeGrid grid)
            {
                switch (action)
                {
                    case MySafeZoneAction.LandingGearLock:
                        {
                            if (!DePatchPlugin.Instance.Config.NoEnemyPlayerLandingGearLocks)
                                return true;

                            // allow grab with landing gears in PVE zone.
                            if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.CheckEntityInZone(entity, ref IsInPVEZone))
                                return true;

                            var LockingBondingBox = entity.PositionComp.LocalAABB;
                            var rad = (LockingBondingBox.Center - LockingBondingBox.Min).Length();
                            rad = (float)(rad * 0.7);
                            BoundingBoxD ExtendedBox = new BoundingSphereD(LockingBondingBox.Center, rad).GetBoundingBox();
                            var GridBox = new MyOrientedBoundingBoxD(ExtendedBox, entity.WorldMatrix);
                            var Entities = new List<MyEntity>();
                            var CleanList = new List<MyCubeGrid>();
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

                            // no owner or nobody cant lock but if it's 1 block landing gear to build from it, then ok.
                            if (LockingGrid != null && LockingGrid.BigOwners?.Count > 0 && LockingGrid.BigOwners.FirstOrDefault() > 0)
                                LockingGridHasOwner = true;
                            else
                            {
                                if (LockingGrid != null && LockingGrid.BlocksCount == 1)
                                    return true;

                                return false;
                            }

                            var LockingGridOwner = LockingGrid.BigOwners.FirstOrDefault();

                            // if locking grid is NPC and have less than 30 blocks deny lock.
                            if (LockingGridHasOwner && MySession.Static.Players.IdentityIsNpc(LockingGridOwner) && LockingGrid.BlocksCount < 30)
                                return false;

                            // only here we can see attached by landing gear grids to main grid!
                            var IMygrids = new List<IMyCubeGrid>();
                            MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical, IMygrids);

                            // convert back to MyCubeGrid
                            var grids = new List<MyCubeGrid>();
                            foreach (var Mygrid in IMygrids)
                            {
                                grids.Add((MyCubeGrid)Mygrid);
                            }

                            // sort the list. largest to smallest
                            grids.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
                            grids.Reverse();
                            grids.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                            // add new list without the locking grid and all that attached to it.
                            foreach (var GridToFilter in Entities.OfType<MyCubeGrid>())
                            {
                                if (grids.Contains(GridToFilter))
                                    continue;

                                if (GridToFilter.IsPreview)
                                    continue;

                                CleanList.Add(GridToFilter);
                            }

                            var LockingToGridOwner = 0L;

                            // sort the list. largest to smallest
                            if (CleanList != null && CleanList.Count > 0)
                            {
                                CleanList.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
                                CleanList.Reverse();
                                CleanList.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                                if (CleanList.FirstOrDefault().BigOwners?.Count > 0)
                                    LockingToGridOwner = CleanList.FirstOrDefault().BigOwners.FirstOrDefault();

                                CleanList.Clear();
                            }

                            // same owner or not owned.
                            if (LockingGridOwner == LockingToGridOwner || LockingToGridOwner == 0)
                                return true;

                            // locking to npc grid
                            if (LockingToGridOwner > 0 && MySession.Static.Players.IdentityIsNpc(LockingToGridOwner))
                                return true;

                            var GridFactionID = MySession.Static.Factions.TryGetPlayerFaction(LockingToGridOwner);
                            var LockingGridFaction = MySession.Static.Factions.TryGetPlayerFaction(LockingGridOwner);

                            if (LockingGridHasOwner)
                            {
                                // both have factions?
                                if (GridFactionID != null && LockingGridFaction != null)
                                {
                                    // same faction?
                                    if (LockingGridFaction.FactionId == GridFactionID.FactionId)
                                        return true;

                                    var FactionsRelationship = MySession.Static.Factions.GetRelationBetweenFactions(GridFactionID.FactionId, LockingGridFaction.FactionId);

                                    // faction relationship
                                    switch (FactionsRelationship.Item1)
                                    {
                                        case MyRelationsBetweenFactions.Enemies:
                                            break;
                                        case MyRelationsBetweenFactions.Neutral:
                                        case MyRelationsBetweenFactions.Friends:
                                            CleanList.Clear();
                                            return true;
                                    }
                                }
                            }
                            // no match then deny.
                            return false;
                        }
                    case MySafeZoneAction.Building:
                        {
                            if (DePatchPlugin.Instance.Config.PveZoneEnabled || DePatchPlugin.Instance.Config.DenyPlacingBlocksOnEnemyGrid)
                            {
                                if (grid == null || user == 0UL || entity is MyWelder Welder || MySession.Static.IsUserAdmin(user))
                                    return true;

                                if (DePatchPlugin.Instance.Config.DenyPlacingBlocksOnEnemyGrid)
                                {
                                    // allow to place blocks if grid has no owner.
                                    if (grid.BigOwners.Count < 1)
                                        return true;

                                    // allow to place if grid is owned by NPC
                                    if (MySession.Static.Players.IdentityIsNpc(grid.BigOwners.FirstOrDefault()))
                                        return true;

                                    // allow to place if grid owned by same player or his faction member.
                                    if (grid.IsFriendlyPlayer(user))
                                        return true;

                                    // deny placing.
                                    return false;
                                }

                                var myPlayer = Sync.Players.TryGetPlayerBySteamId(user);
                                if (myPlayer == null)
                                    return true;

                                if (PVE.CheckEntityInZone(myPlayer, ref IsInPVEZone))
                                {
                                    if (grid.IsFriendlyPlayer(user))
                                        return true;

                                    return false;
                                }
                            }
                            return true;
                        }
                    case MySafeZoneAction.Shooting:
                        {
                            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
                            {
                                if (BootTickStarted)
                                {
                                    // block weapons
                                    return false;
                                }
                            }

                            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                                return true;

                            if (PVE.CheckEntityInZone(entity, ref IsInPVEZone))
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
                return true;

            if (entity is MyCharacter character)
            {
                if (character == null)
                    return true;

                var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == character.GetPlayerIdentityId());

                if (myPlayer != null && PVE.CheckEntityInZone(myPlayer, ref IsInPVEZone))
                    return false;
            }

            return true;
        }
    }
}
