using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch.PVEZONE
{
    [HarmonyPatch(typeof(MyCubeGrid), "Init")]
    internal class MyNewGridPatch
    {
        private static void Postfix(MyCubeGrid __instance)
        {
            var cubegrid = __instance;
            if (cubegrid == null) return;

            if (!DePatchPlugin.GameIsReady || !DePatchPlugin.Instance.Config.Enabled) return;

            if (DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                if (!PVEGrid.Grids.ContainsKey(cubegrid))
                {
                    PVEGrid.Grids?.Add(cubegrid, new PVEGrid(cubegrid));
                }
                var pVEGrid = PVEGrid.Grids[cubegrid];
                if (pVEGrid.InPVEZone())
                {
                    PVE.EntitiesInZone.Add(cubegrid.EntityId);
                    pVEGrid?.OnGridEntered();
                }
            }

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled2) return;
            if (!PVEGrid2.Grids2.ContainsKey(cubegrid))
            {
                PVEGrid2.Grids2?.Add(cubegrid, new PVEGrid2(cubegrid));
            }
            var pVEGrid2 = PVEGrid2.Grids2[cubegrid];
            if (pVEGrid2.InPVEZone2())
            {
                PVE.EntitiesInZone2.Add(cubegrid.EntityId);
                pVEGrid2?.OnGridEntered2();
            }
        }
    }
}
