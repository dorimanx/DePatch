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
using DePatch.CoolDown;

namespace DePatch.KEEN_BUG_FIXES
{
    public static class KEEN_UpdateComponentsFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_sessionComponentsForUpdate;
        private static Dictionary<int, SortedSet<MySessionComponentBase>> ThisSessionComponentsForUpdate;

        private static bool CounterIsActive = false;
        private static int CrashCounter = 0;
        private static bool CheckTimer = false; 

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

                if (CounterIsActive)
                {
                    // arm new timer.
                    int LoopCooldown = 10 * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopCrashComponents, null, LoopCooldown);
                    CounterIsActive = false;
                    CheckTimer = true;
                }

                if (CheckTimer)
                {
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopCrashComponents, null, out var remainingSecondsCrashCheck);

                    if (remainingSecondsCrashCheck < 1)
                    {
                        CheckTimer = false;
                        CrashCounter = 0;
                    }
                }

                // allow crash if we stuck in loop
                if (CrashCounter >= 25)
                {
                    CounterIsActive = false;
                    CheckTimer = false;
                    CrashCounter = 0;
                    return true;
                }

                // Optimization by RYO, cache the session once.
                if (ThisSessionComponentsForUpdate == null)
                    ThisSessionComponentsForUpdate = (Dictionary<int, SortedSet<MySessionComponentBase>>)m_sessionComponentsForUpdate.GetValue(__instance);

                if (ThisSessionComponentsForUpdate.TryGetValue(1, out SortedSet<MySessionComponentBase> sortedSet))
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

                if (ThisSessionComponentsForUpdate.TryGetValue(2, out SortedSet<MySessionComponentBase> sortedSet2))
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

                if (ThisSessionComponentsForUpdate.TryGetValue(4, out SortedSet<MySessionComponentBase> sortedSet3))
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
                CounterIsActive = true;
                CrashCounter++;
                Log.Error(ex, $"Error during UpdateComponents Function! Crash Avoided, crash loop number : {CrashCounter}");
            }
            return false;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static bool RemoveChild(MyHierarchyComponentBase __instance, IMyEntity child, bool preserveWorldPos = false)
#pragma warning restore IDE0060 // Remove unused parameter
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