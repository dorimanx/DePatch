using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System;
using Torch.Managers.PatchManager;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyNewGridPatch
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyCubeGrid), "Init", typeof(MyNewGridPatch), nameof(CubeGridInit), new[] { "objectBuilder" });

        internal static void CubeGridInit(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.GameIsReady || __instance == null)
                return;

            if (DePatchPlugin.Instance.Config.ForbiddenBlocks && !__instance.IsStatic && __instance.Physics != null)
            {
                foreach (var FirstBlock in __instance.CubeBlocks)
                {
                    if (FirstBlock == null || FirstBlock.BlockDefinition == null)
                        continue;

                    if (CubeGridExtensions.IsMatchForbidden(FirstBlock.BlockDefinition))
                    {
                        __instance.Physics.ClearSpeed();
                        MyMultiplayer.RaiseEvent(__instance, (MyCubeGrid x) => new Action(x.ConvertToStatic), default);
                        __instance.ConvertToStatic();

                        var playerId = FirstBlock.BuiltBy;

                        if (!MySession.Static.Players.IsPlayerOnline(playerId))
                            break;

                        var BlockName = FirstBlock.BlockDefinition.DisplayNameText;

                        if (BlockName == string.Empty || BlockName.Equals(null))
                        {
                            BlockName = FirstBlock.BlockDefinition.Id.SubtypeId.ToString();
                            if (BlockName == string.Empty || BlockName.Equals(null))
                                BlockName = FirstBlock.BlockDefinition.Id.TypeId.ToString();
                        }

                        var DenyAlert = $"This Block >>{BlockName}<< can be only on static grid. New grid is now Static!.";
                        MyVisualScriptLogicProvider.ShowNotification(DenyAlert, 10000, "Red", playerId);

                        break;
                    }
                }
            }

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
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