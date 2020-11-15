using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NLog;
using VRageMath;

namespace DePatch
{
	internal static class PVE
	{
		public static readonly Logger Log = LogManager.GetLogger("LTP PVE");

		public static List<long> EntitiesInZone = new List<long>();

		public static BoundingSphereD PVESphere;

		public static void Init(DePatchPlugin plugin)
		{
			PVE.Log.Info("Initing PVE ZONE...");
			PVE.PVESphere = new BoundingSphereD(new Vector3D((double)plugin.Config.PveX, (double)plugin.Config.PveY, (double)plugin.Config.PveZ), (double)plugin.Config.PveZoneRadius);
			DamageHandler.Init();
			PVE.Log.Info("Complete!");
		}
	}
}
