using DePatch.CoolDown;
using DePatch.PVEZONE;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
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

    public static class GridSpeedPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void GridOverSpeedCheck(MyCubeGrid __instance)
        {
            if (!(__instance?.GetBiggestGridInGroup() is MyCubeGrid topGrid) || topGrid.MarkedAsTrash || topGrid.MarkedForClose || topGrid.Immune || topGrid.Physics == null || topGrid.PlayerPresenceTier != MyUpdateTiersPlayerPresence.Normal) return;

            var purgeSpeed = __instance.GridSizeEnum == MyCubeSize.Large
                ? DePatchPlugin.Instance.Config.LargeGridMaxSpeedPurge
                : DePatchPlugin.Instance.Config.SmallGridMaxSpeedPurge;

            if (topGrid.Physics.LinearVelocity.Length() < purgeSpeed && topGrid.Physics.AngularVelocity.Length() < purgeSpeed)
                return;

            int ActionCooldown = 2 * 1000;
            EntityIdCooldownKey OverSpeedGridKey = new EntityIdCooldownKey(topGrid.EntityId);

            _ = CooldownManager.CheckCooldown(OverSpeedGridKey, null, out var remainingSecondsToAction);

            if (remainingSecondsToAction != 0)
                return;

            CooldownManager.StartCooldown(OverSpeedGridKey, null, ActionCooldown);

            var player = MySession.Static.Players.GetControllingPlayer(topGrid);
            Log.Warn($"{topGrid.GridSizeEnum} grid with name '{topGrid.DisplayName}' controlled by '{player?.DisplayName}'({player?.Id.SteamId}) trying to fly above max speed!");
            if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.ShowLogOnly)
				return;

            if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.StopGrid)
            {
                if (player != null)
                {
                    MyVisualScriptLogicProvider.ShowNotification("You have tried to fly above max speed! Grid was stopped!", 10000, MyFontEnum.Red, player.Identity.IdentityId);
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have tried to fly above max speed! Grid was stopped!", Color.Red, "AntiCheat", player.Identity.IdentityId, MyFontEnum.Blue);
                }
                topGrid.Physics.ClearSpeed();
                topGrid.Physics.LinearVelocity = Vector3.Zero;
                topGrid.Physics.AngularVelocity = Vector3.Zero;
            }
            else
            {
                if (player != null)
                {
                    MyVisualScriptLogicProvider.ShowNotification("You have tried to fly above max speed! Grid was DELETED!", 30000, MyFontEnum.Red, player.Identity.IdentityId);
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have tried to fly above max speed! Grid was DELETED!", Color.Red, "AntiCheat", player.Identity.IdentityId, MyFontEnum.Blue);
                    foreach (var a in topGrid.GetFatBlocks<MyCockpit>())
                    {
                    	a?.RemovePilot();
                    }
                    foreach (var b in topGrid.GetFatBlocks<MyCryoChamber>())
                    {
                    	b?.RemovePilot();
                    }
                }

                if (DePatchPlugin.Instance.Config.PveZoneEnabled)
                {
                    if (PVE.EntitiesInZone.Contains(topGrid.EntityId))
                        PVE.EntitiesInZone.Remove(topGrid.EntityId);

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(topGrid.EntityId))
                        PVE.EntitiesInZone2.Remove(topGrid.EntityId);
                }

                // Delete the overspeeding grid.
                topGrid.Close();
            }
        }
    }
}