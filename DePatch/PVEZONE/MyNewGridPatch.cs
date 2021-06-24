using HarmonyLib;
using Sandbox.Game.Entities;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.ObjectBuilders;

namespace DePatch.PVEZONE
{
    //[HarmonyPatch(typeof(MyCubeGrid), "Init")]
    [PatchShim]

    internal static class MyNewGridPatch
    {
        private static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyCubeGrid).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public, null, new Type[1]
            {
                typeof(MyObjectBuilder_EntityBase),
            }, new ParameterModifier[0])).
            Suffixes.Add(typeof(MyNewGridPatch).GetMethod(nameof(CubeGridInit), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));

        internal static void CubeGridInit(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PveZoneEnabled) return;
            if (!DePatchPlugin.GameIsReady) return;
            if (__instance == null) return;

            if (!PVEGrid.Grids.ContainsKey(__instance))
                PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));

            var pVEGrid = PVEGrid.Grids[__instance];
            if (pVEGrid.InPVEZone())
            {
                PVE.EntitiesInZone.Add(__instance.EntityId);
                pVEGrid.OnGridEntered();
            }

            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                if (!PVEGrid2.Grids2.ContainsKey(__instance))
                    PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));

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
