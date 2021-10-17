using NLog;
using Sandbox.Game.Entities;
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
            MethodInfo _target = typeof(MyParallelEntityUpdateOrchestrator).GetMethod("ParallelUpdateHandlerAfterSimulation", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEEN_ParallelCrashFix).GetMethod(nameof(ParallelUpdateHandlerAfterSimulationFIX), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool ParallelUpdateHandlerAfterSimulationFIX(IMyParallelUpdateable entity)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                // checking for null here saves us from crash.
                if (entity == null || entity.MarkedForClose || (entity.UpdateFlags & MyParallelUpdateFlags.EACH_FRAME_PARALLEL) == MyParallelUpdateFlags.NONE || !entity.InScene)
                    return false;

                entity.UpdateAfterSimulationParallel();
                return false;
            }
            return true;
        }
    }
}
