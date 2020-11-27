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
                    }
                }
                catch
                {
                }
            }
        }
    }
}
