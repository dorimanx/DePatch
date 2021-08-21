using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using System;
using System.Collections.Generic;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class KEENAntennaCrashFix
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyAntennaSystem).GetMethod("GetEntityReceivers", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEENAntennaCrashFix).GetMethod(nameof(GetEntityReceiversCheck), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool GetEntityReceiversCheck(MyEntity entity, ref HashSet<MyDataReceiver> output, long playerId)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                if (entity == null)
                {
                    output.Clear();
                    return false;
                }
            }
            return true;
        }
    }

    public static class KEENParallelCrashFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyParallelEntityUpdateOrchestrator).GetMethod("ParallelUpdateHandlerAfterSimulation", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEENParallelCrashFix).GetMethod(nameof(ParallelUpdateHandlerAfterSimulationFIX), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
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

    public static class KEENRenderLocalFix
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyRenderComponentBase).GetMethod("UpdateRenderObjectLocal");
            MethodInfo _patch = typeof(KEENRenderLocalFix).GetMethod(nameof(RenderLocalIgnore));
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        // Server should not care about render updates.
        public static bool RenderLocalIgnore()
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
                return false;

            return true;
        }
    }

    public static class KEENUpdateAfterSimulation100Fix
    {
        private static FieldInfo m_entitiesForUpdate100;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Patch(PatchContext ctx)
        {
            m_entitiesForUpdate100 = typeof(MyParallelEntityUpdateOrchestrator).GetField("m_entitiesForUpdate100", BindingFlags.NonPublic | BindingFlags.Instance);

            ctx.GetPattern(typeof(MyParallelEntityUpdateOrchestrator).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(KEENUpdateAfterSimulation100Fix).GetMethod(nameof(UpdateAfterSimulation100Dpatch), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static bool UpdateAfterSimulation100Dpatch(MyParallelEntityUpdateOrchestrator __instance)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                var My_entitiesForUpdate100 = (MyDistributedUpdater<List<MyEntity>, MyEntity>)m_entitiesForUpdate100.GetValue(__instance);

                foreach (MyEntity myEntity in My_entitiesForUpdate100)
                {
                    if (myEntity == null)
                        continue;

                    if (!myEntity.MarkedForClose && (myEntity.Flags & EntityFlags.NeedsUpdate100) != (EntityFlags)0 && myEntity.InScene)
                    {
                        try
                        {
                            myEntity.UpdateAfterSimulation100();
                        } 
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error during UpdateAfterSimulation100 entity update! Crash Avoided");
                            continue;
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}
