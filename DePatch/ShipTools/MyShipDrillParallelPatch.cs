using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using ParallelTasks;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;

namespace DePatch
{
    [HarmonyPatch(typeof(MyShipDrill), "UpdateBeforeSimulation10")]
    internal class MyShipDrillParallelPatch
    {
        private static MethodInfo Receiver_IsPoweredChanged;

        private static PropertyInfo ShakeAmount;

        private static FieldInfo m_drillFrameCountdown;

        private static FieldInfo m_drillBase;

        public static FieldInfo m_wantsToCollect;

        private static MethodInfo InitSubBlocks;

        public static Thread DrillThread;

        public static List<MyShipDrill> pendingDrillers;

        private static void DrillThreadLoop()
        {
            while (true)
            {
                for (int index = 0; index < pendingDrillers.Count; ++index)
                    AsyncUpdate(pendingDrillers[index]);

                pendingDrillers.Clear();
                Thread.Sleep(166);
            }
        }

        private static bool Prefix(MyShipDrill __instance)
        {
            if (!DrillThread.IsAlive)
            {
                DrillThread.Start();
            }
            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
            {
                if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Parallel)
                {
                    Parallel.Start(delegate ()
                    {
                        AsyncUpdate(__instance);
                    }, WorkPriority.Normal);
                }
                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Keen)
                    AsyncUpdate(__instance);

                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Threading)
                    pendingDrillers.Add(__instance);
            }
            else
            {
                DrillingMode mode = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == __instance.DefinitionId.SubtypeName).Mode;
                if (mode == DrillingMode.Parallel)
                {
                    Parallel.Start(delegate ()
                    {
                        AsyncUpdate(__instance);
                    }, WorkPriority.Normal);
                }
                else if (mode == DrillingMode.Keen)
                    AsyncUpdate(__instance);

                else if (mode == DrillingMode.Threading)
                    pendingDrillers.Add(__instance);
            }
            return false;
        }

        private static void AsyncUpdate(MyShipDrill drill)
        {
            Receiver_IsPoweredChanged.Invoke(drill, new object[0]);
            InitSubBlocks.Invoke(drill, new object[0]);
            if (drill.Parent == null || drill.Parent.Physics == null)
                return;

            m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) - 10);
            if ((int)m_drillFrameCountdown.GetValue(drill) > 0)
                return;

            DrillSettings drillSettings = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == drill.DefinitionId.SubtypeName);
            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
            {
                m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) + DePatchPlugin.Instance.Config.DrillUpdateRate);
            }
            else
            {
                m_drillFrameCountdown.SetValue(drill, (int)((int)m_drillFrameCountdown.GetValue(drill) + (double)drillSettings.TickRate));
            }
            if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out MyGunStatusEnum myGunStatusEnum))
            {
                ShakeAmount.SetValue(drill, 0.0f);
                return;
            }
            bool flag = (bool)m_wantsToCollect.GetValue(drill);
            bool drillIgnoreSubtypes = DePatchPlugin.Instance.Config.DrillIgnoreSubtypes;
            MyDrillBase myDrillBase = (MyDrillBase)m_drillBase.GetValue(drill);
            bool collectOre;
            if (drill.Enabled)
            {
                collectOre = true;
            }
            else if (drillIgnoreSubtypes)
            {
                collectOre = (flag || !DePatchPlugin.Instance.Config.DrillDisableRightClick);
            }
            else if (drillSettings.RightClick)
            {
                collectOre = (flag || drillSettings.RightClick);
            }
            else
            {
                collectOre = flag;
            }
            if (myDrillBase.Drill(collectOre, true, false, 1f))
            {
                ShakeAmount.SetValue(drill, 1f);
                return;
            }
            ShakeAmount.SetValue(drill, 0.5f);
        }

        static MyShipDrillParallelPatch()
        {
            MethodInfo method = typeof(MyShipDrill).GetMethod(nameof(Receiver_IsPoweredChanged), BindingFlags.Instance | BindingFlags.NonPublic);
            if (method is null)
                throw new MissingMethodException("missing Receiver_IsPoweredChanged");

            Receiver_IsPoweredChanged = method;
            ShakeAmount = typeof(MyShipDrill).GetProperty(nameof(ShakeAmount));
            m_drillFrameCountdown = typeof(MyShipDrill).GetField(nameof(m_drillFrameCountdown), BindingFlags.Instance | BindingFlags.NonPublic);
            m_drillBase = typeof(MyShipDrill).GetField(nameof(m_drillBase), BindingFlags.Instance | BindingFlags.NonPublic);
            m_wantsToCollect = typeof(MyShipDrill).GetField(nameof(m_wantsToCollect), BindingFlags.Instance | BindingFlags.NonPublic);
            InitSubBlocks = typeof(MyCubeBlock).GetMethod(nameof(InitSubBlocks), BindingFlags.Instance | BindingFlags.NonPublic);
            DrillThread = new Thread(new ThreadStart(DrillThreadLoop));
            pendingDrillers = new List<MyShipDrill>();
        }
    }
}
