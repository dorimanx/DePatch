using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "UpdateAfterSimulation100")]
    internal class MyCubeGridPatch
    {
        private static void Prefix(MyCubeGrid __instance)
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                try
                {
                    if (!PVEGrid.Grids.ContainsKey(__instance))
                    {
                        PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
                    }

                    if (__instance == null)
                    {
                        PVEGrid.Grids.Remove(__instance);
                    }
                    else
                    {
                        PVEGrid.Grids[__instance].Tick = PVEGrid.Grids[__instance].Tick + 1;
                        if (PVEGrid.Grids[__instance].Tick + 1 > 10)
                        {
                            PVEGrid.Grids[__instance].Tick = 0;
                            if (!PVEGrid.Grids[__instance].InPVEZone() || !PVE.EntitiesInZone.Contains(__instance.EntityId))
                            {
                                if (!PVEGrid.Grids[__instance].InPVEZone() && PVE.EntitiesInZone.Contains(__instance.EntityId))
                                {
                                    PVEGrid.Grids[__instance].OnGridLeft();
                                    PVE.EntitiesInZone.Remove(__instance.EntityId);
                                }
                                if (PVEGrid.Grids[__instance].InPVEZone() && !PVE.EntitiesInZone.Contains(__instance.EntityId))
                                {
                                    PVEGrid.Grids[__instance].OnGridEntered();
                                    PVE.EntitiesInZone.Add(__instance.EntityId);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
