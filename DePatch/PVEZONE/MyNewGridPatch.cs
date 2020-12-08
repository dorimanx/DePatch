﻿using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "Init")]
    internal class MyNewGridPatch
    {
        private static void Postfix(MyCubeGrid __instance)
        {
            MyCubeGrid cubegrid = __instance;
            if (cubegrid != null)
            {
                if (DePatchPlugin.GameIsReady && DePatchPlugin.Instance.Config.Enabled)
                {
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled)
                    {
                        if (!PVEGrid.Grids.ContainsKey(cubegrid))
                        {
                            PVEGrid.Grids?.Add(cubegrid, new PVEGrid(cubegrid));
                        }
                        PVEGrid pVEGrid = PVEGrid.Grids[cubegrid];
                        if (pVEGrid.InPVEZone())
                        {
                            PVE.EntitiesInZone.Add(cubegrid.EntityId);
                            pVEGrid?.OnGridEntered();
                        }
                    }

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        if (!PVEGrid2.Grids2.ContainsKey(cubegrid))
                        {
                            PVEGrid2.Grids2?.Add(cubegrid, new PVEGrid2(cubegrid));
                        }
                        PVEGrid2 pVEGrid2 = PVEGrid2.Grids2[cubegrid];
                        if (pVEGrid2.InPVEZone2())
                        {
                            PVE.EntitiesInZone2.Add(cubegrid.EntityId);
                            pVEGrid2?.OnGridEntered2();
                        }
                    }
                }
            }
        }
    }
}
