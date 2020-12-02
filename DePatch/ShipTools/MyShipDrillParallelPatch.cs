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
        private static MethodInfo Receiver_IsPoweredChanged = typeof(MyShipDrill).GetMethod("Receiver_IsPoweredChanged", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingMethodException("missing Receiver_IsPoweredChanged");

        private static PropertyInfo ShakeAmount = typeof(MyShipDrill).GetProperty("ShakeAmount");

        private static FieldInfo m_drillFrameCountdown = typeof(MyShipDrill).GetField("m_drillFrameCountdown", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo m_drillBase = typeof(MyShipDrill).GetField("m_drillBase", BindingFlags.Instance | BindingFlags.NonPublic);

        public static FieldInfo m_wantsToCollect = typeof(MyShipDrill).GetField("m_wantsToCollect", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo InitSubBlocks = typeof(MyCubeBlock).GetMethod("InitSubBlocks", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Thread DrillThread = new Thread(DrillThreadLoop);

        public static List<MyShipDrill> pendingDrillers = new List<MyShipDrill>();

        private static void DrillThreadLoop()
        {
            while (true)
            {
                for (int index = 0; index < pendingDrillers.Count; ++index)
                {
                    AsyncUpdate(pendingDrillers[index]);
                }
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
                    Parallel.Start(delegate
                    {
                        AsyncUpdate(__instance);
                    });
                }
                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Keen)
                {
                    AsyncUpdate(__instance);
                }
                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Threading)
                {
                    pendingDrillers.Add(__instance);
                }
            }
            else
            {
                switch (DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == __instance.DefinitionId.SubtypeName).Mode)
                {
                    case DrillingMode.Parallel:
                        Parallel.Start(delegate
                        {
                            AsyncUpdate(__instance);
                        });
                        break;
                    case DrillingMode.Keen:
                        AsyncUpdate(__instance);
                        break;
                    case DrillingMode.Threading:
                        pendingDrillers.Add(__instance);
                        break;
                }
            }
            return false;
        }

        private static void AsyncUpdate(MyShipDrill drill)
        {
            Receiver_IsPoweredChanged.Invoke(drill, new object[0]);
            InitSubBlocks.Invoke(drill, new object[0]);

            if (drill.Parent == null || drill.Parent.Physics == null)
            {
                return;
            }

            m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) - 10);
            if ((int)m_drillFrameCountdown.GetValue(drill) > 0)
            {
                return;
            }

            DrillSettings drillSettings = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == drill.DefinitionId.SubtypeName);
            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
            {
                m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) + DePatchPlugin.Instance.Config.DrillUpdateRate);
            }
            else
            {
                m_drillFrameCountdown.SetValue(drill, (int)(float)((int)m_drillFrameCountdown.GetValue(drill) + drillSettings.TickRate));
            }

            if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out var _))
            {
                ShakeAmount.SetValue(drill, 0f);
                return;
            }

            bool collectOre;
            if (drill.Enabled)
            {
                collectOre = true;
            }
            else if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
            {
                collectOre = ((bool)m_wantsToCollect.GetValue(drill) || DePatchPlugin.Instance.Config.DrillDisableRightClick);
            }
            else if (drillSettings.DisableRightClick)
            {
                collectOre = ((bool)m_wantsToCollect.GetValue(drill) || drillSettings.DisableRightClick);
            }
            else
            {
                collectOre = (bool)m_wantsToCollect.GetValue(drill);
            }

            MyDrillBase myDrillBase = (MyDrillBase)m_drillBase.GetValue(drill);
            if (myDrillBase.Drill(collectOre, true, false, 0.1f))
            {
                ShakeAmount.SetValue(drill, 1f);
                return;
            }
            ShakeAmount.SetValue(drill, 0.5f);
        }
    }
}
