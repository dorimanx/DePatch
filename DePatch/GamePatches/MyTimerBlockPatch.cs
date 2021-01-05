using System;
using System.Reflection;
using NLog;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyTimerBlockPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static readonly MethodInfo Update = typeof(MyTimerBlock).GetMethod("UpdateAfterSimulation10", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo UpdatePatch = typeof(MyTimerBlockPatch).GetMethod("PatchMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo TrigNow = typeof(MyTimerBlock).GetMethod("Trigger", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo TrigNowPatch = typeof(MyTimerBlockPatch).GetMethod("TrigMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(Update).Prefixes.Add(UpdatePatch);
            ctx.GetPattern(TrigNow).Prefixes.Add(TrigNowPatch);
            Log.Info("Patching Successful!");
        }

        private static bool PatchMethod(MyTimerBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance.TriggerDelay >= DePatchPlugin.Instance.Config.TimerMinDelay)
                return true;
            __instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;
            return true;
        }

        private static bool TrigMethod(MyTimerBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                return !DePatchPlugin.Instance.Config.DisableTrigNow;
            }
            return true;
        }
    }
}
