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
            MyShipDrill __instance2 = __instance;
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
                        AsyncUpdate(__instance2);
                    });
                }
                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Keen)
                {
                    AsyncUpdate(__instance2);
                }
                else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Threading)
                {
                    pendingDrillers.Add(__instance2);
                }
            }
            else
            {
                switch (DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == __instance2.DefinitionId.SubtypeName).Mode)
                {
                    case DrillingMode.Parallel:
                        Parallel.Start(delegate
                        {
                            AsyncUpdate(__instance2);
                        });
                        break;
                    case DrillingMode.Keen:
                        AsyncUpdate(__instance2);
                        break;
                    case DrillingMode.Threading:
                        pendingDrillers.Add(__instance2);
                        break;
                }
            }
            return false;
        }

        private static void AsyncUpdate(MyShipDrill drill)
        {
            MyShipDrill drill2 = drill;
            Receiver_IsPoweredChanged.Invoke(drill2, new object[0]);
            InitSubBlocks.Invoke(drill2, new object[0]);
            if (drill2.Parent == null || drill2.Parent.Physics == null)
            {
                return;
            }
            m_drillFrameCountdown.SetValue(drill2, (int)m_drillFrameCountdown.GetValue(drill2) - 10);
            if ((int)m_drillFrameCountdown.GetValue(drill2) > 0)
            {
                return;
            }
            DrillSettings drillSettings = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == drill2.DefinitionId.SubtypeName);
            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
            {
                m_drillFrameCountdown.SetValue(drill2, (int)m_drillFrameCountdown.GetValue(drill2) + DePatchPlugin.Instance.Config.DrillUpdateRate);
            }
            else
            {
                m_drillFrameCountdown.SetValue(drill2, (int)((float)(int)m_drillFrameCountdown.GetValue(drill2) + drillSettings.TickRate));
            }
            if (!drill2.CanShoot(MyShootActionEnum.PrimaryAction, drill2.OwnerId, out var _))
            {
                ShakeAmount.SetValue(drill2, 0f);
                return;
            }
            bool flag = (bool)m_wantsToCollect.GetValue(drill2);
            bool drillIgnoreSubtypes = DePatchPlugin.Instance.Config.DrillIgnoreSubtypes;
            bool flag2 = false;
            MyDrillBase obj = (MyDrillBase)m_drillBase.GetValue(drill2);
            flag2 = drill2.Enabled || (drillIgnoreSubtypes ? (flag || !DePatchPlugin.Instance.Config.DrillDisableRightClick) : ((!drillSettings.RightClick) ? flag : (flag || drillSettings.RightClick)));
            if (obj.Drill(flag2))
            {
                ShakeAmount.SetValue(drill2, 1f);
            }
            else
            {
                ShakeAmount.SetValue(drill2, 0.5f);
            }
        }
    }
}
