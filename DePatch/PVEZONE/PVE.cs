using System.Collections.Generic;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRage.Game.Entity;
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

        public static bool CheckEntityInZone(object obj, ref bool __result)
        {
            var zone1 = false;
            var zone2 = false;

            if (obj is MyPlayer myPlayer)
            {
                if (myPlayer == null)
                {
                    __result = false;
                    return false;
                }

                try
                {
                    if (myPlayer != default)
                    {
                        if (PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone1 = true;
                        if (PVESphere2.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone2 = true;
                    }
                }
                catch
                {
                }
            }
            else if (obj is MyEntity entity)
            {
                if (DePatchPlugin.Instance.Config.PveZoneEnabled && EntitiesInZone.Contains(entity.EntityId))
                    zone1 = true;
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && EntitiesInZone2.Contains(entity.EntityId))
                    zone2 = true;

                if (!zone1 && !zone2) return true;
                __result = false;
                return false;
            }

            if (!zone1 && !zone2) return true;
            __result = false;
            return false;
        }

        public static bool CheckEntityInZone(MyCubeGrid grid)
        {
            var res = false;
            return CheckEntityInZone(grid, ref res);
        }
    }
}
