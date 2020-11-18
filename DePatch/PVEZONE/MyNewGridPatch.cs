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
        }
    }
}
