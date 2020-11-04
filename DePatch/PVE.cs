using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NLog;
using VRageMath;

namespace DePatch
{
	internal static class PVE
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002680 File Offset: 0x00000880
		public static void Init(DePatchPlugin plugin)
		{
			PVE.Log.Info("Initing PVE ZONE...");
			PVE.PVESphere = new BoundingSphereD(new Vector3D((double)plugin.Config.PveX, (double)plugin.Config.PveY, (double)plugin.Config.PveZ), (double)plugin.Config.PveZoneRadius);
			DamageHandler.Init();
			PVE.Log.Info("Complete!");
		}

		// Token: 0x04000007 RID: 7
		public static readonly Logger Log = LogManager.GetLogger("LTP PVE");

		// Token: 0x04000008 RID: 8
		public static List<long> EntitiesInZone = new List<long>();

		// Token: 0x04000009 RID: 9
		public static BoundingSphereD PVESphere;
	}
}
