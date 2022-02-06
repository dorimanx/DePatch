using Sandbox;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using Sandbox.Game.World;
using System.Collections.Generic;
using VRage.Game.Components;
using Sandbox.Engine.Multiplayer;
using VRage.ModAPI;

namespace DePatch.KEEN_BUG_FIXES
{
    public static class KEEN_UpdateComponentsFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_sessionComponentsForUpdate;

        public static void Patch(PatchContext ctx)
        {
            m_sessionComponentsForUpdate = typeof(MySession).EasyField("m_sessionComponentsForUpdate");
            ctx.Prefix(typeof(MySession), typeof(KEEN_UpdateComponentsFix), nameof(UpdateComponents));

            ctx.Prefix(typeof(MyHierarchyComponentBase), typeof(KEEN_UpdateComponentsFix), nameof(RemoveChild));
        }

        public static bool UpdateComponents(MySession __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.UpdateComponentsFix)
                return true;

            try
            {
                if (__instance is null)
                    return false;

                var m_sessionComponentsForUpdateList = (Dictionary<int, SortedSet<MySessionComponentBase>>)m_sessionComponentsForUpdate.GetValue(__instance);

                if (m_sessionComponentsForUpdateList.TryGetValue(1, out SortedSet<MySessionComponentBase> sortedSet))
                {
                    if (sortedSet != null && sortedSet.Count > 0)
                    {
                        foreach (MySessionComponentBase mySessionComponentBase in sortedSet)
                        {
                            if (mySessionComponentBase is null)
                                continue;

                            if (mySessionComponentBase.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                                mySessionComponentBase.UpdateBeforeSimulation();
                        }
                    }
                }

                if (MyMultiplayer.Static != null)
                    MyMultiplayer.Static.ReplicationLayer.Simulate();

                if (m_sessionComponentsForUpdateList.TryGetValue(2, out SortedSet<MySessionComponentBase> sortedSet2))
                {
                    if (sortedSet2 != null && sortedSet2.Count > 0)
                    {
                        foreach (MySessionComponentBase mySessionComponentBase2 in sortedSet2)
                        {
                            if (mySessionComponentBase2 is null)
                                continue;

                            if (mySessionComponentBase2.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                                mySessionComponentBase2.Simulate();
                        }
                    }
                }

                if (m_sessionComponentsForUpdateList.TryGetValue(4, out SortedSet<MySessionComponentBase> sortedSet3))
                {
                    if (sortedSet3 != null && sortedSet3.Count > 0)
                    {
                        foreach (MySessionComponentBase mySessionComponentBase3 in sortedSet3)
                        {
                            if (mySessionComponentBase3 is null)
                                continue;

                            if (mySessionComponentBase3.UpdatedBeforeInit() || MySandboxGame.IsGameReady)
                                mySessionComponentBase3.UpdateAfterSimulation();
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during UpdateComponents Function! Crash Avoided");
            }
            return false;
        }

        public static bool RemoveChild(MyHierarchyComponentBase __instance, IMyEntity child, bool preserveWorldPos = false)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.UpdateComponentsFix)
                return true;

            if (__instance == null || child == null)
                return false;

            MyHierarchyComponentBase myHierarchyComponentBase = child.Components?.Get<MyHierarchyComponentBase>();
            if (myHierarchyComponentBase == null)
                return false;

            return true;
        }
    }
}