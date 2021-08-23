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

            ctx.GetPattern(typeof(MyParallelEntityUpdateOrchestrator).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(KEEN_UpdateAfterSimulation100Fix).GetMethod(nameof(UpdateAfterSimulation100Dpatch), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
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
