using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
	// Token: 0x02000019 RID: 25
	[HarmonyPatch(typeof(MyCameraBlock), "Init")]
	internal class MyCameraBlockPatch
	{
		private static void Prefix(MyCameraBlock __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled)
			{
				return;
			}
			__instance.BlockDefinition.RaycastDistanceLimit = (double)DePatchPlugin.Instance.Config.RaycastLimit;
		}
	}
}
