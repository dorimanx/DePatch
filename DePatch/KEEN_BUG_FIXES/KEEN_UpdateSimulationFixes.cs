using NLog;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Collections;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace DePatch.KEEN_BUG_FIXES
{
    public static class KEEN_UpdateSimulationFixes
    {
        private static FieldInfo m_entitiesForUpdate100;
        private static FieldInfo m_entitiesForUpdate10;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation10FIX || DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX
                                                                         || DePatchPlugin.Instance.Config.UpdateBeforeSimulation100FIX)
            {
                m_entitiesForUpdate10 = typeof(MyParallelEntityUpdateOrchestrator).easyField("m_entitiesForUpdate10");
                m_entitiesForUpdate100 = typeof(MyParallelEntityUpdateOrchestrator).easyField("m_entitiesForUpdate100");

                if (DePatchPlugin.Instance.Config.UpdateAfterSimulation10FIX)
                    ctx.Prefix(typeof(MyParallelEntityUpdateOrchestrator), typeof(KEEN_UpdateSimulationFixes), nameof(UpdateAfterSimulation10));

                if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
                    ctx.Prefix(typeof(MyParallelEntityUpdateOrchestrator), typeof(KEEN_UpdateSimulationFixes), nameof(UpdateAfterSimulation100));

                if (DePatchPlugin.Instance.Config.UpdateBeforeSimulation100FIX)
                    ctx.Prefix(typeof(MyParallelEntityUpdateOrchestrator), typeof(KEEN_UpdateSimulationFixes), nameof(UpdateBeforeSimulation100));
            }

            if (DePatchPlugin.Instance.Config.ParallelUpdateHandlerAfterSimulationFIX)
                ctx.Prefix(typeof(MyParallelEntityUpdateOrchestrator), typeof(KEEN_UpdateSimulationFixes), nameof(ParallelUpdateHandlerAfterSimulation));
        }

        private static bool UpdateAfterSimulation10(MyParallelEntityUpdateOrchestrator __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                if (__instance == null)
                    return false;

                try
                {
                    var My_entitiesForUpdate10 = (MyDistributedUpdater<List<MyEntity>, MyEntity>)m_entitiesForUpdate10.GetValue(__instance);
                    // checking for Null here saving us from crash,
                    if (My_entitiesForUpdate10 == null)
                        return false;

                    foreach (MyEntity myEntity in My_entitiesForUpdate10)
                    {
                        // checking for Null here saving us from crash,
                        if (myEntity != null && !myEntity.MarkedForClose && (myEntity.Flags & EntityFlags.NeedsUpdate10) != (EntityFlags)0 && myEntity.InScene)
                            myEntity.UpdateAfterSimulation10();
                    }
                }
                catch (Exception ex)
                {
                    // We have few hits on this! and crash was avoided!.
                    Log.Error(ex, "Error during UpdateAfterSimulation10 entity update! Crash Avoided");
                }
                return false;
            }
            return true;
        }

        private static bool UpdateAfterSimulation100(MyParallelEntityUpdateOrchestrator __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation10FIX)
            {
                if (__instance == null)
                    return false;

                try
                {
                    var My_entitiesForUpdate100 = (MyDistributedUpdater<List<MyEntity>, MyEntity>)m_entitiesForUpdate100.GetValue(__instance);
                    // checking for Null here saving us from crash,
                    if (My_entitiesForUpdate100 == null)
                        return false;

                    foreach (MyEntity myEntity in My_entitiesForUpdate100)
                    {
                        // checking for Null here saving us from crash,
                        if (myEntity != null && !myEntity.MarkedForClose && (myEntity.Flags & EntityFlags.NeedsUpdate100) != (EntityFlags)0 && myEntity.InScene)
                            myEntity.UpdateAfterSimulation100();
                    }
                }
                catch (Exception ex)
                {
                    // We have few hits on this! and crash was avoided!.
                    Log.Error(ex, "Error during UpdateAfterSimulation100 entity update! Crash Avoided");
                }
                return false;
            }
            return true;
        }

        private static bool UpdateBeforeSimulation100(MyParallelEntityUpdateOrchestrator __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateBeforeSimulation100FIX)
            {
                if (__instance == null)
                    return false;

                try
                {
                    var My_entitiesForUpdate100 = (MyDistributedUpdater<List<MyEntity>, MyEntity>)m_entitiesForUpdate100.GetValue(__instance);
                    // checking for Null here saving us from crash,
                    if (My_entitiesForUpdate100 == null)
                        return false;

                    My_entitiesForUpdate100.Update();

                    foreach (MyEntity myEntity in My_entitiesForUpdate100)
                    {
                        // checking for Null here saving us from crash,
                        if (myEntity != null && !myEntity.MarkedForClose && (myEntity.Flags & EntityFlags.NeedsUpdate100) != (EntityFlags)0 && myEntity.InScene)
                            myEntity.UpdateBeforeSimulation100();
                    }
                }
                catch (Exception ex)
                {
                    // We have few hits on this! and crash was avoided!.
                    Log.Error(ex, "Error during UpdateBeforeSimulation100 entity update! Crash Avoided");
                }
                return false;
            }
            return true;
        }

        private static bool ParallelUpdateHandlerAfterSimulation(IMyParallelUpdateable entity)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.ParallelUpdateHandlerAfterSimulationFIX)
            {
                // checking for null here saves us from crash.
                if (entity != null && !entity.MarkedForClose && (entity.UpdateFlags & MyParallelUpdateFlags.EACH_FRAME_PARALLEL) != MyParallelUpdateFlags.NONE && entity.InScene)
                    entity.UpdateAfterSimulationParallel();

                return false;
            }
            return true;
        }
    }
}