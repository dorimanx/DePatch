using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace DePatch
{
	[HarmonyPatch(typeof(MyGridPhysics), "ApplyDeformation", new Type[]
	{
		typeof(float),
		typeof(float),
		typeof(float),
		typeof(Vector3),
		typeof(Vector3),
		typeof(MyStringHash),
		typeof(float),
		typeof(float),
		typeof(long)
	})]
	internal class MyGridDeformationPatch
	{
		private static bool Prefix(MyGridPhysics __instance, ref bool __result, long attackerId)
		{
			if (DePatchPlugin.Instance.Config.ProtectGrid)
			{
				MyCubeGrid myCubeGrid = __instance.Entity as MyCubeGrid;
				if (myCubeGrid != null && attackerId == 0L)
				{
					if (myCubeGrid.Physics != null)
					{
						MyGridPhysics physics = myCubeGrid.Physics;
						float? num = (physics != null) ? new float?(physics.LinearVelocity.Length()) : null;
						float minProtectSpeed = DePatchPlugin.Instance.Config.MinProtectSpeed;
						if (num.GetValueOrDefault() < minProtectSpeed & num != null)
						{
							__result = false;
							return false;
						}
					}
					if (myCubeGrid.GridSizeEnum == MyCubeSize.Small && (long)myCubeGrid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize)
					{
						__result = false;
						return false;
					}
					if (myCubeGrid.GridSizeEnum == MyCubeSize.Large && (long)myCubeGrid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize)
					{
						__result = false;
						return false;
					}
				}
			}
			return true;
		}
	}
}
