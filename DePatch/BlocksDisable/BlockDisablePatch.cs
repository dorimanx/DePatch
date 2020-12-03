using System.Reflection;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using System;
using NLog;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using Sandbox.Game.Entities;
using VRage.Game;
using Sandbox.Game.Entities.Blocks;

namespace DePatch
{
    [PatchShim]
    public static class BlockUpdatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyFunctionalBlock).GetMethod("UpdateBeforeSimulation100", BindingFlags.Instance | BindingFlags.Public)).Prefixes.Add(typeof(BlockUpdatePatch).GetMethod("CheckBlock", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void CheckBlock(MyFunctionalBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if (DePatchPlugin.Instance.Config.EnableGridMaxSpeedPurge)
                {
                    try
                    {
                        float speedlarge = DePatchPlugin.Instance.Config.LargeGridMaxSpeedPurge;
                        float speedsmall = DePatchPlugin.Instance.Config.SmallGridMaxSpeedPurge;
                        MyPhysicsComponentBase GridPhysics = __instance.CubeGrid.Physics;
                        IMyCubeGrid GridCube = __instance.CubeGrid;
                        MyCubeGrid Grid = (MyCubeGrid)GridCube;

                        var LinearVelocity = GridPhysics.LinearVelocity;
                        var AngularVelocity = GridPhysics.AngularVelocity;

                        if (__instance != null && GridPhysics != null && GridCube.GridSizeEnum == MyCubeSize.Large && (LinearVelocity.Length() > speedlarge || AngularVelocity.Length() > speedlarge))
                        {
                            var b = Grid.GetFatBlocks<MyCockpit>();
                            var d = Grid.GetFatBlocks<MyCryoChamber>();
                            foreach (var c in b)
                            {
                                c.RemovePilot();
                            }
                            foreach (var e in d)
                            {
                                e.RemovePilot();
                            }
                            Log.Error("Large Grid Name =" + Grid.DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was DELETED");
                            Grid.Close();
                        }

                        if (__instance != null && GridPhysics != null && GridCube.GridSizeEnum == MyCubeSize.Small && (LinearVelocity.Length() > speedsmall || AngularVelocity.Length() > speedsmall))
                        {
                            var b = Grid.GetFatBlocks<MyCockpit>();
                            var d = Grid.GetFatBlocks<MyCryoChamber>();
                            foreach (var c in b)
                            {
                                c.RemovePilot();
                            }
                            foreach (var e in d)
                            {
                                e.RemovePilot();
                            }
                            Log.Error("Small Grid Name =" + Grid.DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was DELETED");
                            Grid.Close();
                        }
                    }
                    catch
                    {
                    }
                }

                if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
                {
                    if (__instance != null && (string.Compare("ShipWelder", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                    }
                    else
                    {
                        if (__instance != null && PlayersUtility.KeepBlockOff(__instance))
                        {
                            if (__instance.Enabled)
                            {
                                __instance.Enabled = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
