using DePatch.BlocksDisable;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System;
using System.Linq;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyCubeGridPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyCubeGrid).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(MyCubeGridPatch).GetMethod(nameof(UpdateAfterSimulation100), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateAfterSimulation100(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (__instance == null)
                return;

            if (DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                try
                {
                    if (__instance.MarkedForClose || __instance.Closed)
                    {
                        if (PVEGrid.Grids.ContainsKey(__instance))
                            PVEGrid.Grids.Remove(__instance);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVEGrid2.Grids2.ContainsKey(__instance))
                            PVEGrid2.Grids2.Remove(__instance);

                        if (PVE.EntitiesInZone.Contains(__instance.EntityId))
                            PVE.EntitiesInZone.Remove(__instance.EntityId);

                        if (PVE.EntitiesInZone2.Contains(__instance.EntityId))
                            PVE.EntitiesInZone2.Remove(__instance.EntityId);
                    }

                    var HasOwner = (__instance.BigOwners.Count > 0) ? __instance.BigOwners.FirstOrDefault() : 0L;
                    var NPC_Grid = false;

                    if (HasOwner != 0L && MySession.Static.Players.IdentityIsNpc(HasOwner))
                        NPC_Grid = true;

                    if (NPC_Grid && PVEGrid.Grids.ContainsKey(__instance))
                        PVEGrid.Grids.Remove(__instance);

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && NPC_Grid && PVEGrid2.Grids2.ContainsKey(__instance))
                        PVEGrid2.Grids2.Remove(__instance);

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
                            PVE.EntitiesInZone.Remove(__instance.EntityId);
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
                                PVE.EntitiesInZone2.Remove(__instance.EntityId);
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
