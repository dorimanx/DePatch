using NLog;
using Sandbox.Game.Entities;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_ParallelCrashFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyParallelEntityUpdateOrchestrator).GetMethod("ParallelUpdateHandlerAfterSimulation", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEEN_ParallelCrashFix).GetMethod(nameof(ParallelUpdateHandlerAfterSimulationFIX), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool ParallelUpdateHandlerAfterSimulationFIX(IMyParallelUpdateable entity)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                if (entity == null)
                    return false;

                if (entity.MarkedForClose || (entity.UpdateFlags & MyParallelUpdateFlags.EACH_FRAME_PARALLEL) == MyParallelUpdateFlags.NONE || !entity.InScene)
                    return false;

                try
                {
                    entity.UpdateAfterSimulationParallel();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during parallel entity update! Crash Avoided");
                }
                return false;
            }
            return true;
        }
    }
}
