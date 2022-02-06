using Sandbox.Game.Entities.Cube;
using SpaceEngineers.Game.Entities.Blocks;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyAssemblerPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyAssembler), typeof(MyAssemblerPatch), nameof(UpdateBeforeSimulation100));
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
                        if (!__instance.CubeGrid.IsStatic && !(__instance is MySurvivalKit))
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
        }
    }
}
