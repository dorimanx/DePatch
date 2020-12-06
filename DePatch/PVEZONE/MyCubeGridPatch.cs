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

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && !PVEGrid2.Grids2.ContainsKey(__instance))
                    {
                        PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));
                    }

                    if (__instance == null)
                    {
                        PVEGrid.Grids.Remove(__instance);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                            PVEGrid2.Grids2.Remove(__instance);
                    }
                    else
                    {
                        PVEGrid pVEGrid = PVEGrid.Grids[__instance];
                        if (++PVE.Tick <= 10)
                        {
                            return;
                        }
                        PVE.Tick = 0;
                        bool flag = pVEGrid.InPVEZone();
                        bool flag2 = PVE.EntitiesInZone.Contains(__instance.EntityId);
                        if (!(flag && flag2))
                        {
                            if (!flag && flag2)
                            {
                                pVEGrid.OnGridLeft();
                                PVE.EntitiesInZone.Remove(__instance.EntityId);
                            }
                            if (flag && !flag2)
                            {
                                pVEGrid.OnGridEntered();
                                PVE.EntitiesInZone.Add(__instance.EntityId);
                            }
                        }
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                        {
                            PVEGrid2 pVEGrid2 = PVEGrid2.Grids2[__instance];
                            bool flag3 = pVEGrid2.InPVEZone2();
                            bool flag4 = PVE.EntitiesInZone2.Contains(__instance.EntityId);
                            if (!(flag3 && flag4))
                            {
                                if (!flag3 && flag4)
                                {
                                    pVEGrid2.OnGridLeft2();
                                    PVE.EntitiesInZone2.Remove(__instance.EntityId);
                                }
                                if (flag3 && !flag4)
                                {
                                    pVEGrid2.OnGridEntered2();
                                    PVE.EntitiesInZone2.Add(__instance.EntityId);
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
