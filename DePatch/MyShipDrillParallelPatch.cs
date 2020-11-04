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
		// Token: 0x0600008C RID: 140 RVA: 0x00003B1C File Offset: 0x00001D1C
		private static void DrillThreadLoop()
		{
			for (;;)
			{
				for (int i = 0; i < MyShipDrillParallelPatch.pendingDrillers.Count; i++)
				{
					MyShipDrillParallelPatch.AsyncUpdate(MyShipDrillParallelPatch.pendingDrillers[i]);
				}
				MyShipDrillParallelPatch.pendingDrillers.Clear();
				Thread.Sleep(166);
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00003B64 File Offset: 0x00001D64
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
				{
					MyShipDrillParallelPatch.AsyncUpdate(__instance);
				}
				else if (DePatchPlugin.Instance.Config.ParallelDrill == DrillingMode.Threading)
				{
					MyShipDrillParallelPatch.pendingDrillers.Add(__instance);
				}
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
				{
					MyShipDrillParallelPatch.AsyncUpdate(__instance);
				}
				else if (mode == DrillingMode.Threading)
				{
					MyShipDrillParallelPatch.pendingDrillers.Add(__instance);
				}
			}
			return false;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003C74 File Offset: 0x00001E74
		private static void AsyncUpdate(MyShipDrill drill)
		{
			MyShipDrillParallelPatch.Receiver_IsPoweredChanged.Invoke(drill, new object[0]);
			MyShipDrillParallelPatch.InitSubBlocks.Invoke(drill, new object[0]);
			if (drill.Parent == null || drill.Parent.Physics == null)
			{
				return;
			}
			MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue(drill, (int)MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue(drill) - 10);
			if ((int)MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue(drill) > 0)
			{
				return;
			}
			DrillSettings drillSettings = DrillSettings.drills.ToList<DrillSettings>().Find((DrillSettings b) => b.Subtype == drill.DefinitionId.SubtypeName);
			if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
			{
				MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue(drill, (int)MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue(drill) + DePatchPlugin.Instance.Config.DrillUpdateRate);
			}
			else
			{
				MyShipDrillParallelPatch.m_drillFrameCountdown.SetValue(drill, (int)((float)((int)MyShipDrillParallelPatch.m_drillFrameCountdown.GetValue(drill)) + drillSettings.TickRate));
			}
			MyGunStatusEnum myGunStatusEnum;
			if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out myGunStatusEnum))
			{
				MyShipDrillParallelPatch.ShakeAmount.SetValue(drill, 0f);
				return;
			}
			bool flag = (bool)MyShipDrillParallelPatch.m_wantsToCollect.GetValue(drill);
			bool drillIgnoreSubtypes = DePatchPlugin.Instance.Config.DrillIgnoreSubtypes;
			MyDrillBase myDrillBase = (MyDrillBase)MyShipDrillParallelPatch.m_drillBase.GetValue(drill);
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
				MyShipDrillParallelPatch.ShakeAmount.SetValue(drill, 1f);
				return;
			}
			MyShipDrillParallelPatch.ShakeAmount.SetValue(drill, 0.5f);
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003ED0 File Offset: 0x000020D0
		// Note: this type is marked as 'beforefieldinit'.
		static MyShipDrillParallelPatch()
		{
			MethodInfo method = typeof(MyShipDrill).GetMethod("Receiver_IsPoweredChanged", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method == null)
			{
				throw new MissingMethodException("missing Receiver_IsPoweredChanged");
			}
			MyShipDrillParallelPatch.Receiver_IsPoweredChanged = method;
			MyShipDrillParallelPatch.ShakeAmount = typeof(MyShipDrill).GetProperty("ShakeAmount");
			MyShipDrillParallelPatch.m_drillFrameCountdown = typeof(MyShipDrill).GetField("m_drillFrameCountdown", BindingFlags.Instance | BindingFlags.NonPublic);
			MyShipDrillParallelPatch.m_drillBase = typeof(MyShipDrill).GetField("m_drillBase", BindingFlags.Instance | BindingFlags.NonPublic);
			MyShipDrillParallelPatch.m_wantsToCollect = typeof(MyShipDrill).GetField("m_wantsToCollect", BindingFlags.Instance | BindingFlags.NonPublic);
			MyShipDrillParallelPatch.InitSubBlocks = typeof(MyCubeBlock).GetMethod("InitSubBlocks", BindingFlags.Instance | BindingFlags.NonPublic);
			MyShipDrillParallelPatch.DrillThread = new Thread(new ThreadStart(MyShipDrillParallelPatch.DrillThreadLoop));
			MyShipDrillParallelPatch.pendingDrillers = new List<MyShipDrill>();
		}

		// Token: 0x04000047 RID: 71
		private static MethodInfo Receiver_IsPoweredChanged;

		// Token: 0x04000048 RID: 72
		private static PropertyInfo ShakeAmount;

		// Token: 0x04000049 RID: 73
		private static FieldInfo m_drillFrameCountdown;

		// Token: 0x0400004A RID: 74
		private static FieldInfo m_drillBase;

		// Token: 0x0400004B RID: 75
		public static FieldInfo m_wantsToCollect;

		// Token: 0x0400004C RID: 76
		private static MethodInfo InitSubBlocks;

		// Token: 0x0400004D RID: 77
		public static Thread DrillThread;

		// Token: 0x0400004E RID: 78
		public static List<MyShipDrill> pendingDrillers;
	}
}
