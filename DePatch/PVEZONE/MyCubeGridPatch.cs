using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "UpdateAfterSimulation100")]
    internal class MyCubeGridPatch
    {
        private static void Prefix(MyCubeGrid __instance)
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled && DePatchPlugin.Instance.Config.Enabled)
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
                        if (++pVEGrid.Tick <= 10)
                        {
                            return;
                        }
                        pVEGrid.Tick = 0;
                        bool flag = pVEGrid.InPVEZone();
                        bool flag2 = PVE.EntitiesInZone.Contains(__instance.EntityId);
                        if (!(flag && flag2))
                        {
                            if (__instance != null && !flag && flag2)
                            {
                                PVE.EntitiesInZone.Remove(__instance.EntityId);
                                pVEGrid?.OnGridLeft();
                            }
                            if (__instance != null && flag && !flag2)
                            {
                                PVE.EntitiesInZone.Add(__instance.EntityId);
                                pVEGrid?.OnGridEntered();
                            }
                        }
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                        {
                            PVEGrid2 pVEGrid2 = PVEGrid2.Grids2[__instance];
                            bool flag3 = pVEGrid2.InPVEZone2();
                            bool flag4 = PVE.EntitiesInZone2.Contains(__instance.EntityId);
                            if (!(flag3 && flag4))
                            {
                                if (__instance != null && !flag3 && flag4)
                                {
                                    PVE.EntitiesInZone2.Remove(__instance.EntityId);
                                    pVEGrid2?.OnGridLeft2();
                                }
                                if (__instance != null && flag3 && !flag4)
                                {
                                    PVE.EntitiesInZone2.Add(__instance.EntityId);
                                    pVEGrid2?.OnGridEntered2();
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
