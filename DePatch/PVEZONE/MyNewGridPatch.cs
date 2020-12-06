using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "Init")]
    internal class MyNewGridPatch
    {
        private static void Postfix(MyCubeGrid __instance)
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                if (!PVEGrid.Grids.ContainsKey(__instance))
                {
                    PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
                }

                if (PVEGrid.Grids[__instance].InPVEZone())
                {
                    PVEGrid.Grids[__instance].OnGridEntered();
                    PVE.EntitiesInZone.Add(__instance.EntityId);
                }
            }

            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                if (!PVEGrid2.Grids2.ContainsKey(__instance))
                {
                    PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));
                }

                if (PVEGrid2.Grids2[__instance].InPVEZone2())
                {
                    PVEGrid2.Grids2[__instance].OnGridEntered2();
                    PVE.EntitiesInZone2.Add(__instance.EntityId);
                }
            }
        }
    }
}
