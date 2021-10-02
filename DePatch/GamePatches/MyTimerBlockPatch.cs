using System;
using System.Reflection;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyTimerBlockPatch
    {
        internal static readonly MethodInfo TimerUpdateAfterSimulation10 = typeof(MyTimerBlock).GetMethod("UpdateAfterSimulation10", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo UpdatePatch = typeof(MyTimerBlockPatch).GetMethod(nameof(PatchMethod), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo TimerTrigNow = typeof(MyTimerBlock).GetMethod("Trigger", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo TrigNowPatch = typeof(MyTimerBlockPatch).GetMethod(nameof(TrigMethod), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(TimerUpdateAfterSimulation10).Prefixes.Add(UpdatePatch);
            ctx.GetPattern(TimerTrigNow).Prefixes.Add(TrigNowPatch);
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