using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch
{
	[PatchShim]
	public static class MyTimerBlockPatch
	{
		public static readonly Logger Log = LogManager.GetCurrentClassLogger();

		internal static readonly MethodInfo Update;

		internal static readonly MethodInfo UpdatePatch;

		internal static readonly MethodInfo TrigNow;

		internal static readonly MethodInfo TrigNowPatch;

		public static void Patch(PatchContext ctx)
		{
      		ctx.GetPattern((MethodBase) MyTimerBlockPatch.Update).Prefixes.Add(MyTimerBlockPatch.UpdatePatch);
      		ctx.GetPattern((MethodBase) MyTimerBlockPatch.TrigNow).Prefixes.Add(MyTimerBlockPatch.TrigNowPatch);
			MyTimerBlockPatch.Log.Info("Patching Successful!");
		}

		public static void PatchMethod(MyTimerBlock __instance)
		{
      		if (!DePatchPlugin.Instance.Config.Enabled || (double) __instance.TriggerDelay >= (double) DePatchPlugin.Instance.Config.TimerMinDelay)
				return;
				__instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;
			}

		public static bool TrigMethod(MyTimerBlock __instance)
		{
			return !DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.DisableTrigNow;
		}

		static MyTimerBlockPatch()
		{
			MethodInfo method1 = typeof(MyTimerBlock).GetMethod("UpdateAfterSimulation10", BindingFlags.Instance | BindingFlags.Public);
      		if ((object) method1 == null)
				throw new Exception("Failed to find patch method");
			MyTimerBlockPatch.Update = method1;

			MethodInfo method2 = typeof(MyTimerBlockPatch).GetMethod("PatchMethod", BindingFlags.Static | BindingFlags.Public);
      		if ((object) method2 == null)
				throw new Exception("Failed to find patch method");
			MyTimerBlockPatch.UpdatePatch = method2;

			MethodInfo method3 = typeof(MyTimerBlock).GetMethod("Trigger", BindingFlags.Instance | BindingFlags.NonPublic);
      		if ((object) method3 == null)
				throw new Exception("Failed to find patch method");
			MyTimerBlockPatch.TrigNow = method3;

			MethodInfo method4 = typeof(MyTimerBlockPatch).GetMethod("TrigMethod", BindingFlags.Static | BindingFlags.Public);
      		if ((object) method4 == null)
				throw new Exception("Failed to find patch method");
			MyTimerBlockPatch.TrigNowPatch = method4;
		}
	}
}
