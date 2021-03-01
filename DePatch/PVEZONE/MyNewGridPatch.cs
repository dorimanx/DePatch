using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch.PVEZONE
{
    [HarmonyPatch(typeof(MyCubeGrid), "Init")]
    internal class MyNewGridPatch
    {
        internal static void Postfix(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PveZoneEnabled) return;

            if (!DePatchPlugin.GameIsReady)
                return;

            if (__instance == null)
                return;

            if (!PVEGrid.Grids.ContainsKey(__instance))
            {
                PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
            }

            var pVEGrid = PVEGrid.Grids[__instance];
            if (pVEGrid.InPVEZone())
            {
                PVE.EntitiesInZone.Add(__instance.EntityId);
                pVEGrid.OnGridEntered();
            }

            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                if (!PVEGrid2.Grids2.ContainsKey(__instance))
                {
                    PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));
                }
                var pVEGrid2 = PVEGrid2.Grids2[__instance];
                if (pVEGrid2.InPVEZone2())
                {
                    PVE.EntitiesInZone2.Add(__instance.EntityId);
                    pVEGrid2.OnGridEntered2();
                }
            }
        }
    }
}
