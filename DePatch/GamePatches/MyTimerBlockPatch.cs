using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyTimerBlockPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyTimerBlock), "UpdateAfterSimulation10", typeof(MyTimerBlockPatch), nameof(PatchMethod));
            ctx.Prefix(typeof(MyTimerBlock), "Trigger", typeof(MyTimerBlockPatch), nameof(TrigMethod));
        }

        private static void PatchMethod(MyTimerBlock __instance)
        {
            if (__instance == null || !DePatchPlugin.Instance.Config.Enabled || __instance.TriggerDelay >= DePatchPlugin.Instance.Config.TimerMinDelay)
                return;

            __instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;
        }

        private static bool TrigMethod(MyTimerBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            return !DePatchPlugin.Instance.Config.DisableTrigNow;
        }
    }
}