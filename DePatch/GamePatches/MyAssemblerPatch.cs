using Sandbox.Game.Entities.Cube;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyAssemblerPatch
    {
        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyAssembler).GetMethod("UpdateBeforeSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).
                Prefixes.Add(typeof(MyAssemblerPatch).GetMethod(nameof(UpdateBeforeSimulation100), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateBeforeSimulation100(MyAssembler __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            try
            {
                if (__instance != null && __instance.Enabled && __instance.IsFunctional)
                {
                    if (__instance.IsSlave && DePatchPlugin.Instance.Config.DisableAssemblerCoop)
                    {
                        __instance.IsSlave = false;

                        if (!__instance.IsQueueEmpty)
                            __instance.ClearQueue();
                    }
                    if (__instance.RepeatEnabled && DePatchPlugin.Instance.Config.DisableAssemblerLoop)
                    {
                        __instance.RequestRepeatEnabled(false);
                        if (!__instance.IsQueueEmpty)
                            __instance.ClearQueue();
                    }
                    if (DePatchPlugin.Instance.Config.DisableProductionOnShip)
                    {
                        if (!__instance.CubeGrid.IsStatic && __instance.CubeGrid.GridSizeEnum == MyCubeSize.Large)
                        {
                            __instance.Enabled = false;

                            if (!__instance.IsQueueEmpty)
                                __instance.ClearQueue();
                        }
                    }
                }
            }
            catch
            {
            }

            /* this code is obsolete.
            if (DePatchPlugin.Instance.Config.CargoCleanup)
            {
                /// loop for 30 sec till next grid add / remove
                if (!CooldownManager.CheckCooldown(LoopRequestID, null, out var LoopremainingSeconds))
                {
                    var LoopTimer = LoopremainingSeconds;
                    return true;
                }
                CooldownManager.StartCooldown(LoopRequestID, null, LoopCooldown);
                Task.Run(delegate
                {
                    CargoCleanup.SearchAndDeleteItemStacks();
                });
            }
            */
        }
    }
}
