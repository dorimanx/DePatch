using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DePatch.BlocksDisable
{
    public enum SpeedingMode
    {
        StopGrid,
        DeleteGrid,
        ShowLogOnly
    }

    //[HarmonyPatch(typeof(MyCubeGrid), nameof(MyCubeGrid.UpdateAfterSimulation100))]
    [PatchShim]

    public static class GridSpeedPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly HashSet<GridOverSpeed> OverSpeeds = new HashSet<GridOverSpeed>(new GridOverSpeed.GridOverSpeedComparer());

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyCubeGrid).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(GridSpeedPatch).GetMethod(nameof(UpdateAfterSimulation100), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateAfterSimulation100(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.EnableGridMaxSpeedPurge) return;

            if (!(__instance?.GetBiggestGridInGroup() is MyCubeGrid topGrid) || topGrid.MarkedAsTrash || topGrid.MarkedForClose || topGrid.Immune || topGrid.Physics == null || topGrid.PlayerPresenceTier != MyUpdateTiersPlayerPresence.Normal) return;

            var purgeSpeed = __instance.GridSizeEnum == MyCubeSize.Large
                ? DePatchPlugin.Instance.Config.LargeGridMaxSpeedPurge
                : DePatchPlugin.Instance.Config.SmallGridMaxSpeedPurge;

            if (topGrid.Physics.LinearVelocity.Length() < purgeSpeed && topGrid.Physics.AngularVelocity.Length() < purgeSpeed) return;

            var overSpeed = OverSpeeds.FirstOrDefault(b => b.Grid.Equals(topGrid));

            if (overSpeed == default(GridOverSpeed))
            {
                overSpeed = new GridOverSpeed(topGrid);
                OverSpeeds.Add(overSpeed);
            }

            overSpeed.WarningsCount++;

            if (overSpeed.WarningsCount < 5)
				return;

            var player = MySession.Static.Players.GetControllingPlayer(topGrid);
            Log.Warn($"{topGrid.GridSizeEnum} grid with name '{topGrid.DisplayNameText}' controlled by '{player?.DisplayName}'({player?.Id.SteamId}) trying fly above max speed!");
            if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.ShowLogOnly)
				return;

            if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.StopGrid)
            {
                if (player != null)
                {
                    MyVisualScriptLogicProvider.ShowNotification("You tried fly above max speed and has been stopped!", 20000, MyFontEnum.Red, player.Identity.IdentityId);
                    MyVisualScriptLogicProvider.SendChatMessageColored("You tried fly above max speed and has been stopped!", Color.Red, "AntiCheat", player.Identity.IdentityId, MyFontEnum.Blue);
                }
                topGrid.Physics.ClearSpeed();
            }
            else
            {
                if (player != null)
                {
                    MyVisualScriptLogicProvider.ShowNotification("You tried fly above max speed and has been deleted!", 20000, MyFontEnum.Red, player.Identity.IdentityId);
                    MyVisualScriptLogicProvider.SendChatMessageColored("You tried fly above max speed and has been deleted!", Color.Red, "AntiCheat", player.Identity.IdentityId, MyFontEnum.Blue);
                    foreach (var a in topGrid.GetFatBlocks<MyCockpit>())
                    {
                    	a?.RemovePilot();
                    }
                    foreach (var b in topGrid.GetFatBlocks<MyCryoChamber>())
                    {
                    	b?.RemovePilot();
                    }
                }
                topGrid.Close();
            }
        }
    }
}