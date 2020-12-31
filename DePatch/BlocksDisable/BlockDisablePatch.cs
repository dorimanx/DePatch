using System.Reflection;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using System;
using NLog;
using VRage.Game.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;

public enum SpeedingMode
{
    StopGrid,
    DeleteGrid,
    ShowLogOnly
}

namespace DePatch
{
    [PatchShim]
    public static class BlockUpdatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyFunctionalBlock).GetMethod("UpdateBeforeSimulation100", BindingFlags.Instance | BindingFlags.Public)).Prefixes.Add(typeof(BlockUpdatePatch).GetMethod("CheckBlock", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static int StopTick = 1;

        private static void CheckBlock(MyFunctionalBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if (DePatchPlugin.Instance.Config.EnableGridMaxSpeedPurge)
                {
                    if (__instance != null && __instance.CubeGrid.Physics != null && __instance.CubeGrid.PlayerPresenceTier == MyUpdateTiersPlayerPresence.Normal)
                    {
                        float speedlarge = DePatchPlugin.Instance.Config.LargeGridMaxSpeedPurge;
                        float speedsmall = DePatchPlugin.Instance.Config.SmallGridMaxSpeedPurge;
                        switch (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum)
                        {
                            case MyCubeSize.Large when __instance.CubeGrid.Physics.LinearVelocity.Length() > speedlarge || __instance.CubeGrid.Physics.AngularVelocity.Length() > speedlarge:
                            case MyCubeSize.Small when __instance.CubeGrid.Physics.LinearVelocity.Length() > speedsmall || __instance.CubeGrid.Physics.AngularVelocity.Length() > speedsmall:
                                {
                                    if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.StopGrid)
                                    {
                                        if (++StopTick >= 100)
                                        {
                                            StopTick = 1;
                                            __instance.CubeGrid.Physics.ClearSpeed();

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                                Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was STOPPED");

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                                Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was STOPPED");
                                        }
                                    }
                                    else if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.ShowLogOnly)
                                    {
                                        if (++StopTick >= 100)
                                        {
                                            StopTick = 1;
                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                                Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was not DELETED");

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                                Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was not DELETED");
                                        }
                                    }
                                    else if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.DeleteGrid)
                                    {
                                        foreach (var a in __instance.CubeGrid.GetFatBlocks<MyCockpit>())
                                        {
                                            if (a != null)
                                                a.RemovePilot();
                                        }
                                        foreach (var b in __instance.CubeGrid.GetFatBlocks<MyCryoChamber>())
                                        {
                                            if (b != null)
                                                b.RemovePilot();
                                        }

                                        if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                            Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was DELETED");

                                        if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                            Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was DELETED");

                                        __instance.CubeGrid.Close();
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }

                if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
                {
                    if (__instance != null && __instance.IsFunctional && __instance.Enabled)
                    {
                        if (string.Compare("ShipWelder", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare("MyProgrammableBlock", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            // weldertypes are off in ShipWelderPatch.cs
                            // ProgramBlocksTypes are off in MyProgramBlockSlow.cs
                        }
                        else
                        {
                            if (!MySession.Static.Players.IsPlayerOnline(__instance.OwnerId))
                            {
                                if (PlayersUtility.KeepBlockOff(__instance))
                                    __instance.Enabled = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
