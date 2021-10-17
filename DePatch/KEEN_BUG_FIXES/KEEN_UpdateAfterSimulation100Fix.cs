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
    [PatchShim]
    public static class KEEN_UpdateAfterSimulation100Fix
    {
        private static FieldInfo m_entitiesForUpdate100;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Patch(PatchContext ctx)
        {
            m_entitiesForUpdate100 = typeof(MyParallelEntityUpdateOrchestrator).GetField("m_entitiesForUpdate100", BindingFlags.NonPublic | BindingFlags.Instance);

            ctx.GetPattern(typeof(MyParallelEntityUpdateOrchestrator).GetMethod("UpdateAfterSimulation100", BindingFlags.Instance | BindingFlags.NonPublic)).
                Prefixes.Add(typeof(KEEN_UpdateAfterSimulation100Fix).GetMethod(nameof(UpdateAfterSimulation100Dpatch), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static bool UpdateAfterSimulation100Dpatch(MyParallelEntityUpdateOrchestrator __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
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
    }
}
