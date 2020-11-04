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
		// Token: 0x0600009D RID: 157 RVA: 0x000047B4 File Offset: 0x000029B4
		public static void Patch(PatchContext ctx)
		{
			ctx.GetPattern(MyTimerBlockPatch.Update).Prefixes.Add(MyTimerBlockPatch.UpdatePatch);
			ctx.GetPattern(MyTimerBlockPatch.TrigNow).Prefixes.Add(MyTimerBlockPatch.TrigNowPatch);
			MyTimerBlockPatch.Log.Info("Patching Successful!");
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00004806 File Offset: 0x00002A06
		public static void PatchMethod(MyTimerBlock __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled)
			{
				return;
			}
			if (__instance.TriggerDelay < DePatchPlugin.Instance.Config.TimerMinDelay)
			{
				__instance.TriggerDelay = DePatchPlugin.Instance.Config.TimerMinDelay;
			}
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00004846 File Offset: 0x00002A46
		public static bool TrigMethod(MyTimerBlock __instance)
		{
			return !DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.DisableTrigNow;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004870 File Offset: 0x00002A70
		// Note: this type is marked as 'beforefieldinit'.
		static MyTimerBlockPatch()
		{
			MethodInfo method = typeof(MyTimerBlock).GetMethod("UpdateAfterSimulation10", BindingFlags.Instance | BindingFlags.Public);
			if (method == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MyTimerBlockPatch.Update = method;
			MethodInfo method2 = typeof(MyTimerBlockPatch).GetMethod("PatchMethod", BindingFlags.Static | BindingFlags.Public);
			if (method2 == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MyTimerBlockPatch.UpdatePatch = method2;
			MethodInfo method3 = typeof(MyTimerBlock).GetMethod("Trigger", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method3 == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MyTimerBlockPatch.TrigNow = method3;
			MethodInfo method4 = typeof(MyTimerBlockPatch).GetMethod("TrigMethod", BindingFlags.Static | BindingFlags.Public);
			if (method4 == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MyTimerBlockPatch.TrigNowPatch = method4;
		}

		// Token: 0x04000058 RID: 88
		public static readonly Logger Log = LogManager.GetCurrentClassLogger();

		// Token: 0x04000059 RID: 89
		internal static readonly MethodInfo Update;

		// Token: 0x0400005A RID: 90
		internal static readonly MethodInfo UpdatePatch;

		// Token: 0x0400005B RID: 91
		internal static readonly MethodInfo TrigNow;

		// Token: 0x0400005C RID: 92
		internal static readonly MethodInfo TrigNowPatch;
	}
}
