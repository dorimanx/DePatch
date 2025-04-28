using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using SpaceEngineers.Game.ModAPI;

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

        private static void PatchMethod(IMyTimerBlock __instance)
        {
            if (__instance == null || !DePatchPlugin.Instance.Config.Enabled)
                return;

            if (__instance.TriggerDelay < DePatchPlugin.Instance.Config.TimerMinDelay)
                __instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;
        }

        private static bool TrigMethod(IMyTimerBlock __instance)
        {
            if (__instance == null || !DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (__instance.TriggerDelay < DePatchPlugin.Instance.Config.TimerMinDelay)
                __instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;

            if (__instance.OwnerId == 0)
                return false;

            return !DePatchPlugin.Instance.Config.DisableTrigNow;
        }
    }
}