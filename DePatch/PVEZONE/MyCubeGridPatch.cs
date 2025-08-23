using DePatch.BlocksDisable;
using NLog;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using System;
using System.Linq;
using Torch;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Utils;
using VRageMath;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyCubeGridPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            ctx.Suffix(typeof(MyCubeGrid), typeof(MyCubeGridPatch), nameof(UpdateAfterSimulation100));
            ctx.Prefix(typeof(MyCubeGrid), typeof(MyCubeGridPatch), nameof(BuildBlock));
            ctx.Prefix(typeof(MyLandingGear), "Attach", typeof(MyCubeGridPatch), nameof(Attach), new[] { "entity", "gearSpacePivot", "otherBodySpacePivot", "exactBlock" });
        }

        private static bool BuildBlock(MyCubeGrid __instance, MyCubeBlockDefinition blockDefinition, Vector3 colorMaskHsv, MyStringHash skinId, Vector3I min, Quaternion orientation, long owner, long entityId, MyEntity builderEntity, out MySlimBlock __result, MyObjectBuilder_CubeBlock blockObjectBuilder = null, bool updateVolume = true, bool testMerge = true, bool buildAsAdmin = false, string localizedDisplayNameBase = "")
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled || DePatchPlugin.Instance.Config.DenyPlacingBlocksOnEnemyGrid || DePatchPlugin.Instance.Config.DenyPlacingBlocksOnNPCGrid)
            {
                if (builderEntity is MyCharacter Player)
                {
                    var IsInPVEZone = false;
                    var SteamId = Player.ControlSteamId;

                    if (__instance == null || SteamId == 0UL || builderEntity is MyWelder Welder || MySession.Static.IsUserAdmin(SteamId))
                    {
                        __result = null;
                        return true;
                    }

                    // allow to place blocks if grid has no owner.
                    if (__instance.BigOwners.Count < 1)
                    {
                        __result = null;
                        return true;
                    }

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled)
                    {
                        var myPlayer = Sync.Players.TryGetPlayerBySteamId(SteamId);
                        if (myPlayer == null)
                        {
                            __result = null;
                            return true;
                        }

                        if (PVE.CheckEntityInZone(myPlayer, ref IsInPVEZone))
                        {
                            // allow to place if grid is owned player or ally!
                            if (__instance.IsFriendlyPlayer(SteamId))
                            {
                                __result = null;
                                return true;
                            }

                            // allow to place if grid is owned by NPC!
                            if (MySession.Static.Players.IdentityIsNpc(__instance.BigOwners.FirstOrDefault()) && !DePatchPlugin.Instance.Config.DenyPlacingBlocksOnNPCGrid)
                            {
                                __result = null;
                                return true;
                            }

                            // deny placing blocks on enemy grin in PVE zones
                            __result = null;
                            return false;
                        }
                    }

                    if (DePatchPlugin.Instance.Config.DenyPlacingBlocksOnNPCGrid)
                    {
                        if (MySession.Static.Players.IdentityIsNpc(__instance.BigOwners.FirstOrDefault()))
                        {
                            __result = null;
                            return false;
                        }
                    }

                    // deny placing blocks if outside of PVE and grid is owned by enemy.
                    if (DePatchPlugin.Instance.Config.DenyPlacingBlocksOnEnemyGrid)
                    {
                        // allow to place if grid owned by same player or his faction member.
                        if (__instance.IsFriendlyPlayer(SteamId))
                        {
                            __result = null;
                            return true;
                        }
                        else
                        {
                            __result = null;
                            return false;
                        }
                    }
                }
            }

            __result = null;
            return true;
        }

        private static bool Attach(MyLandingGear __instance, MyEntity entity, Vector3 gearSpacePivot, Matrix otherBodySpacePivot, MyCubeBlock exactBlock = null)
        {
            if (!DePatchPlugin.Instance.Config.NoEnemyPlayerLandingGearLocks)
                return true;

            if (entity is MyCubeGrid grid)
            {
                bool LockingGridHasOwner;
                MyCubeGrid LockingGrid = __instance.CubeGrid;
                var LockingGridOwner = LockingGrid.BigOwners.FirstOrDefault();
                var LockingToGridOwner = grid.BigOwners.FirstOrDefault();

                // no owner or nobody cant lock but if it's 1 block landing gear to build from it, then ok.
                if (LockingGrid != null && LockingGrid.BigOwners?.Count > 0 && LockingGrid.BigOwners.FirstOrDefault() > 0)
                    LockingGridHasOwner = true;
                else
                {
                    if (LockingGrid != null && LockingGrid.BlocksCount == 1)
                        return true;

                    return false;
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
                                return true;
                        }
                    }
                }

                var DontLock = false;
                __instance.ResetAutoLock();
                __instance.RequestLock(DontLock);

                // no match found then deny
                return false;
            }

            // no grid then allow.
            return true;
        }

        private static void UpdateAfterSimulation100(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null)
                return;

            if (DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                try
                {
                    if (__instance.MarkedForClose || __instance.Closed)
                    {
                        if (PVEGrid.Grids.ContainsKey(__instance))
                            _ = PVEGrid.Grids.Remove(__instance);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVEGrid2.Grids2.ContainsKey(__instance))
                            _ = PVEGrid2.Grids2.Remove(__instance);

                        if (PVE.EntitiesInZone.Contains(__instance.EntityId))
                            _ = PVE.EntitiesInZone.Remove(__instance.EntityId);

                        if (PVE.EntitiesInZone2.Contains(__instance.EntityId))
                            _ = PVE.EntitiesInZone2.Remove(__instance.EntityId);
                    }

                    var HasOwner = (__instance.BigOwners?.Count > 0) ? __instance.BigOwners?.FirstOrDefault() : 0L;
                    var NPC_Grid = false;

                    if (HasOwner != 0L && MySession.Static.Players.IdentityIsNpc((long)HasOwner))
                        NPC_Grid = true;

                    if (NPC_Grid && PVEGrid.Grids.ContainsKey(__instance))
                        _ = PVEGrid.Grids.Remove(__instance);

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && NPC_Grid && PVEGrid2.Grids2.ContainsKey(__instance))
                        _ = PVEGrid2.Grids2.Remove(__instance);

                    if (NPC_Grid)
                        goto exit;

                    if (!PVEGrid.Grids.ContainsKey(__instance))
                        PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && !PVEGrid2.Grids2.ContainsKey(__instance))
                        PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));

                    var pVEGrid = PVEGrid.Grids[__instance];
                    var InPVEZone1 = pVEGrid.InPVEZone();
                    var InPVEZone1Collection = PVE.EntitiesInZone.Contains(__instance.EntityId);

                    if (!InPVEZone1 && InPVEZone1Collection)
                    {
                        if (PVE.EntitiesInZone.Contains(__instance.EntityId))
                        {
                            _ = PVE.EntitiesInZone.Remove(__instance.EntityId);
                            pVEGrid?.OnGridLeft();
                        }
                    }
                    if (InPVEZone1 && !InPVEZone1Collection)
                    {
                        if (!PVE.EntitiesInZone.Contains(__instance.EntityId))
                        {
                            PVE.EntitiesInZone.Add(__instance.EntityId);
                            pVEGrid?.OnGridEntered();
                        }
                    }

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var pVEGrid2 = PVEGrid2.Grids2[__instance];
                        var InPVEZone2 = pVEGrid2.InPVEZone2();
                        var InPVEZone2Collection = PVE.EntitiesInZone2.Contains(__instance.EntityId);

                        if (!InPVEZone2 && InPVEZone2Collection)
                        {
                            if (PVE.EntitiesInZone2.Contains(__instance.EntityId))
                            {
                                _ = PVE.EntitiesInZone2.Remove(__instance.EntityId);
                                pVEGrid2?.OnGridLeft2();
                            }
                        }
                        if (InPVEZone2 && !InPVEZone2Collection)
                        {
                            if (!PVE.EntitiesInZone2.Contains(__instance.EntityId))
                            {
                                PVE.EntitiesInZone2.Add(__instance.EntityId);
                                pVEGrid2?.OnGridEntered2();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

        exit:

            // check grid overspeed.
            if (DePatchPlugin.Instance.Config.EnableGridMaxSpeedPurge)
                GridSpeedPatch.GridOverSpeedCheck(__instance);
        }
    }
}
