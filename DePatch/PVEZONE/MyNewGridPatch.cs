using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyNewGridPatch
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyCubeGrid), "Init", typeof(MyNewGridPatch), nameof(CubeGridInit), new[] { "objectBuilder" });

        internal static void CubeGridInit(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PveZoneEnabled || !DePatchPlugin.GameIsReady || __instance == null)
                return;
 
            if (!PVEGrid.Grids.ContainsKey(__instance))
                PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));

            var pVEGrid = PVEGrid.Grids[__instance];
            if (pVEGrid.InPVEZone())
            {
                if (!PVE.EntitiesInZone.Contains(__instance.EntityId))
                {
                    PVE.EntitiesInZone.Add(__instance.EntityId);
                    pVEGrid.OnGridEntered();
                }
            }

            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                if (!PVEGrid2.Grids2.ContainsKey(__instance))
                    PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));

                var pVEGrid2 = PVEGrid2.Grids2[__instance];
                if (pVEGrid2.InPVEZone2())
                {
                    if (!PVE.EntitiesInZone2.Contains(__instance.EntityId))
                    {
                        PVE.EntitiesInZone2.Add(__instance.EntityId);
                        pVEGrid2.OnGridEntered2();
                    }
                }
            }
        }
    }
}
