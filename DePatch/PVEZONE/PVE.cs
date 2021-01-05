using System.Collections.Generic;
using NLog;
using VRageMath;

namespace DePatch.PVEZONE
{
    internal static class PVE
    {
        public static readonly Logger Log = LogManager.GetLogger("PVE ZONE");

        public static List<long> EntitiesInZone = new List<long>();
        public static List<long> EntitiesInZone2 = new List<long>();

        public static BoundingSphereD PVESphere;
        public static BoundingSphereD PVESphere2;

        public static void Init(DePatchPlugin plugin)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            Log.Info("Initing PVE ZONE...");
            PVESphere = new BoundingSphereD(new Vector3D(plugin.Config.PveX, plugin.Config.PveY, plugin.Config.PveZ), plugin.Config.PveZoneRadius);
            PVESphere2 = new BoundingSphereD(new Vector3D(plugin.Config.PveX2, plugin.Config.PveY2, plugin.Config.PveZ2), plugin.Config.PveZoneRadius2);
            DamageHandler.Init();
            Log.Info("Complete!");
        }
    }
}
