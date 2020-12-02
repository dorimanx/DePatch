using System.Collections.Generic;
using NLog;
using VRageMath;

namespace DePatch
{
    internal static class PVE
    {
        public static readonly Logger Log = LogManager.GetLogger("PVE ZONE");

        public static List<long> EntitiesInZone = new List<long>();

        public static BoundingSphereD PVESphere;

        public static void Init(DePatchPlugin plugin)
        {
            Log.Info("Initing PVE ZONE...");
            PVESphere = new BoundingSphereD(new Vector3D(plugin.Config.PveX, plugin.Config.PveY, plugin.Config.PveZ), plugin.Config.PveZoneRadius);
            DamageHandler.Init();
            Log.Info("Complete!");
        }
    }
}
