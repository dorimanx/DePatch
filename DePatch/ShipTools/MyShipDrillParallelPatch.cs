using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        		for (int index = 0; index < MyShipDrillParallelPatch.pendingDrillers.Count; ++index)
					MyShipDrillParallelPatch.AsyncUpdate(MyShipDrillParallelPatch.pendingDrillers[index]);

				MyShipDrillParallelPatch.pendingDrillers.Clear();
				Thread.Sleep(166);
			}
		}

		private static bool Prefix(MyShipDrill __instance)
		{
			if (!MyShipDrillParallelPatch.DrillThread.IsAlive)
			{
				MyShipDrillParallelPatch.DrillThread.Start();
			}
			if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
			{
				if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Parallel)
				{
					Parallel.Start(delegate()
					{
						MyShipDrillParallelPatch.AsyncUpdate(__instance);
					}, WorkPriority.Normal);
				}
				else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Keen)
					MyShipDrillParallelPatch.AsyncUpdate(__instance);

				else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Threading)
					MyShipDrillParallelPatch.pendingDrillers.Add(__instance);
			}
			else
			{
				DrillingMode mode = DrillSettings.drills.ToList<DrillSettings>().Find((DrillSettings b) => b.Subtype == __instance.DefinitionId.SubtypeName).Mode;
				if (mode == DrillingMode.Parallel)
				{
					Parallel.Start(delegate()
					{
						MyShipDrillParallelPatch.AsyncUpdate(__instance);
					}, WorkPriority.Normal);
				}
				else if (mode == DrillingMode.Keen)
					MyShipDrillParallelPatch.AsyncUpdate(__instance);

				else if (mode == DrillingMode.Threading)
					MyShipDrillParallelPatch.pendingDrillers.Add(__instance);
			}
			return false;
		}

		private static void AsyncUpdate(MyShipDrill drill)
		{
			MyShipDrillParallelPatch.Receiver_IsPoweredChanged.Invoke((object) drill, new object[0]);
			MyShipDrillParallelPatch.InitSubBlocks.Invoke((object) drill, new object[0]);
			if (drill.Parent == null || drill.Parent.Physics == null)
				return;

			MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue((object) drill, (object) ((int) MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue((object) drill) - 10));
			if ((int)MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue((object) drill) > 0)
				return;

			DrillSettings drillSettings = DrillSettings.drills.ToList<DrillSettings>().Find((DrillSettings b) => b.Subtype == drill.DefinitionId.SubtypeName);
			if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
			{
				MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue((object) drill, (object) ((int) MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue((object) drill) + DePatchPlugin.Instance.Config.DrillUpdateRate));
			}
			else
			{
				MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue((object) drill, (object) (int) ((double) (int) MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue((object) drill) + (double) drillSettings.TickRate));
			}
			MyGunStatusEnum myGunStatusEnum;
			if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out myGunStatusEnum))
			{
        		MyShipDrillParallelPatch.ShakeAmount.SetValue((object) drill, (object) 0.0f);
				return;
			}
			bool flag = (bool)MyShipDrillParallelPatch.m_wantsToCollect.GetValue((object) drill);
			bool drillIgnoreSubtypes = DePatchPlugin.Instance.Config.DrillIgnoreSubtypes;
			MyDrillBase myDrillBase = (MyDrillBase)MyShipDrillParallelPatch.m_drillBase.GetValue((object) drill);
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
				MyShipDrillParallelPatch.ShakeAmount.SetValue((object) drill, (object) 1f);
				return;
			}
			MyShipDrillParallelPatch.ShakeAmount.SetValue((object) drill, (object) 0.5f);
		}

		static MyShipDrillParallelPatch()
		{
			MethodInfo method = typeof(MyShipDrill).GetMethod(nameof (Receiver_IsPoweredChanged), BindingFlags.Instance | BindingFlags.NonPublic);
			if ((object) method == null)
				throw new MissingMethodException("missing Receiver_IsPoweredChanged");

      		MyShipDrillParallelPatch.Receiver_IsPoweredChanged = method;
      		MyShipDrillParallelPatch.ShakeAmount = typeof (MyShipDrill).GetProperty(nameof (ShakeAmount));
      		MyShipDrillParallelPatch.m_drillFrameCountdown = typeof (MyShipDrill).GetField(nameof (m_drillFrameCountdown), BindingFlags.Instance | BindingFlags.NonPublic);
      		MyShipDrillParallelPatch.m_drillBase = typeof (MyShipDrill).GetField(nameof (m_drillBase), BindingFlags.Instance | BindingFlags.NonPublic);
      		MyShipDrillParallelPatch.m_wantsToCollect = typeof (MyShipDrill).GetField(nameof (m_wantsToCollect), BindingFlags.Instance | BindingFlags.NonPublic);
      		MyShipDrillParallelPatch.InitSubBlocks = typeof (MyCubeBlock).GetMethod(nameof (InitSubBlocks), BindingFlags.Instance | BindingFlags.NonPublic);
      		MyShipDrillParallelPatch.DrillThread = new Thread(new ThreadStart(MyShipDrillParallelPatch.DrillThreadLoop));
      		MyShipDrillParallelPatch.pendingDrillers = new List<MyShipDrill>();
		}
	}
}
