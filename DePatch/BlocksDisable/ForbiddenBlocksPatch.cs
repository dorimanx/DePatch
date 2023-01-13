using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using Torch.Managers.PatchManager;
using VRage.Network;
using VRageMath;

namespace DePatch.BlocksDisable
{
    [PatchShim]

    internal static class ForbiddenBlocksPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyCubeGrid), "OnConvertedToShipRequest", typeof(ForbiddenBlocksPatch), nameof(OnConvertedToShipRequest));
            ctx.Prefix(typeof(MyCubeGrid), "BuildBlocksRequest", typeof(ForbiddenBlocksPatch), nameof(BuildBlocksRequest));
            ctx.Prefix(typeof(MyProjectorBase), "BuildInternal", typeof(ForbiddenBlocksPatch), nameof(BuildInternal));
        }

        // New CubeGridInit part is in MyNewGridPatch file for this feature.

        private static bool OnConvertedToShipRequest(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ForbiddenBlocks || !DePatchPlugin.GameIsReady ||
                    __instance == null || !__instance.IsStatic)
                return true;

            if (__instance.Physics != null && __instance.BlocksCount != 0)
            {
                foreach (var GridBlock in __instance.CubeBlocks)
                {
                    if (GridBlock == null || GridBlock.BlockDefinition == null)
                        continue;

                    if (CubeGridExtensions.IsMatchForbidden(GridBlock.BlockDefinition))
                    {
                        __instance.Physics.ClearSpeed();
                        MyMultiplayer.RaiseEvent(__instance, (MyCubeGrid x) => new Action(x.ConvertToStatic), MyEventContext.Current.Sender);
                        __instance.ConvertToStatic();

                        var remoteUserId = MyEventContext.Current.Sender.Value;
                        var playerId = MySession.Static.Players.TryGetIdentityId(remoteUserId);

                        if (remoteUserId == 0 || !MySession.Static.Players.IsPlayerOnline(playerId))
                            return false;

                        var BlockName = GridBlock.BlockDefinition.DisplayNameText;

                        if (GridBlock.FatBlock != null)
                            BlockName = GridBlock.FatBlock.DisplayNameText;

                        if (BlockName == string.Empty || BlockName.Equals(null))
                        {
                            BlockName = GridBlock.BlockDefinition.Id.SubtypeId.ToString();
                            if (BlockName == string.Empty || BlockName.Equals(null))
                                BlockName = GridBlock.BlockDefinition.Id.TypeId.ToString();
                        }

                        var DenyAlert = $"Your Grid contains block >>{BlockName}<< it's not allowed on dynamic grid, remove it first.";
                        MyVisualScriptLogicProvider.ShowNotification(DenyAlert, 10000, "Red", playerId);

                        return false;
                    }
                }
            }

            return true;
        }

        private static bool BuildBlocksRequest(MyCubeGrid __instance, HashSet<MyCubeGrid.MyBlockLocation> locations)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ForbiddenBlocks || __instance == null || __instance.IsStatic)
                return true;

            var def = MyDefinitionManager.Static.GetCubeBlockDefinition(locations.FirstOrDefault().BlockDefinition);
            if (def == null)
                return true;

            if (CubeGridExtensions.IsMatchForbidden(def))
            {
                var remoteUserId = MyEventContext.Current.Sender.Value;
                var playerId = MySession.Static.Players.TryGetIdentityId(remoteUserId);

                if (remoteUserId == 0 || !MySession.Static.Players.IsPlayerOnline(playerId))
                    return false;

                var BlockName = def.DisplayNameText;

                if (BlockName == string.Empty || BlockName.Equals(null))
                {
                    BlockName = def.Id.SubtypeId.ToString();
                    if (BlockName == string.Empty || BlockName.Equals(null))
                        BlockName = def.Id.TypeId.ToString();
                }

                var DenyAlert = $"This block >>{BlockName}<< can be placed only on static grid!";
                MyVisualScriptLogicProvider.ShowNotification(DenyAlert, 5000, "Red", playerId);

                return false;
            }

            return true;
        }

        private static void BuildInternal(MyProjectorBase __instance, Vector3I cubeBlockPosition, long owner, long builder, bool requestInstant = true, long builtBy = 0)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ForbiddenBlocks || __instance == null ||
                    __instance.CubeGrid == null || __instance.CubeGrid.IsStatic)
                return;

            var def = __instance.ProjectedGrid?.GetCubeBlock(cubeBlockPosition)?.BlockDefinition;
            if (def == null || __instance.CubeGrid.Physics == null)
                return;

            if (CubeGridExtensions.IsMatchForbidden(def))
            {
                var remoteUserId = MyEventContext.Current.Sender.Value;
                var playerId = MySession.Static.Players.TryGetIdentityId(remoteUserId);

                __instance.CubeGrid.Physics.ClearSpeed();
                MyMultiplayer.RaiseEvent(__instance.CubeGrid, (MyCubeGrid x) => new Action(x.ConvertToStatic), default);
                __instance.CubeGrid.ConvertToStatic();

                if (remoteUserId == 0 || !MySession.Static.Players.IsPlayerOnline(playerId))
                    return;

                var BlockName = def.DisplayNameText;

                if (__instance.ProjectedGrid.GetCubeBlock(cubeBlockPosition).FatBlock != null)
                    BlockName = __instance.ProjectedGrid.GetCubeBlock(cubeBlockPosition).FatBlock.DisplayNameText;

                if (BlockName == string.Empty || BlockName.Equals(null))
                {
                    BlockName = def.Id.SubtypeId.ToString();
                    if (BlockName == string.Empty || BlockName.Equals(null))
                        BlockName = def.Id.TypeId.ToString();
                }

                var DenyAlert = $"This block >>{BlockName}<< can be placed only on static grid!, Your grid is now Static!";
                MyVisualScriptLogicProvider.ShowNotification(DenyAlert, 5000, "Red", playerId);
            }
        }
    }
}