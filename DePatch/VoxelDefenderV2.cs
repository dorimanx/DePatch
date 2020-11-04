using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;
using VRageMath;

namespace DePatch
{
	[HarmonyPatch(typeof(MyCubeGrid), "PerformCutouts")]
	internal class VoxelDefenderV2
	{
		private static bool Prefix(MyCubeGrid __instance)
		{
			if (!DePatchPlugin.Instance.Config.ProtectVoxels)
			{
				return true;
			}
			if (__instance == null || __instance.Physics == null)
			{
				return false;
			}
			Vector3D position = __instance.PositionComp.GetPosition();
			if (__instance.Physics.LinearVelocity.Length() > DePatchPlugin.Instance.Config.MinProtectSpeed)
			{
				__instance.Physics.ApplyImpulse(position - __instance.Physics.LinearVelocity * (float)__instance.BlocksCount, position + __instance.Physics.LinearVelocity);
			}
			return false;
		}
	}
}
