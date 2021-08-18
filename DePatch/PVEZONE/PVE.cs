using System.Collections.Generic;
using System.Linq;
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

            PVESphere = new BoundingSphereD(new Vector3D(plugin.Config.PveX, plugin.Config.PveY, plugin.Config.PveZ), plugin.Config.PveZoneRadius);
            PVESphere2 = new BoundingSphereD(new Vector3D(plugin.Config.PveX2, plugin.Config.PveY2, plugin.Config.PveZ2), plugin.Config.PveZoneRadius2);
            DamageHandler.Init();
            Log.Info("Initing PVE ZONE... Complete!");
        }

        public static bool CheckEntityInZone(object obj, ref bool __result)
        {
            var zone1 = false;
            var zone2 = false;
            __result = false;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return __result;

            if (obj is MyPlayer myPlayer)
            {
                if (myPlayer == null)
                    return __result;

                if (myPlayer != default)
                {
                    try
                    {
                        if (PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone1 = true;
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVESphere2.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone2 = true;
                    }
                    catch
                    {
                        // FALSE. not in PVE zones.
                        return __result;
                    }

                    if (zone1 || zone2)
                    {
                        __result = true;
                        return __result;
                    }

                    // FALSE. not in PVE zones.
                    return __result;
                }
            }
            else if (obj is MyEntity entity)
            {
                if (EntitiesInZone.Contains(entity.EntityId))
                    zone1 = true;
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && EntitiesInZone2.Contains(entity.EntityId))
                    zone2 = true;

                if (zone1 || zone2)
                {
                    __result = true;
                    return __result;
                }

                // FALSE. not in PVE zones.
                return __result;
            }

            // FALSE. not in PVE zones.
            return __result;
        }

        public static bool CheckEntityInZone(MyCubeGrid grid)
        {
            bool res = false;
            return CheckEntityInZone(grid, ref res);
        }

        internal static MyPlayer FindOnlineOwner(MyCubeGrid grid)
        {
            var controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            if (controllingPlayer != null)
                return controllingPlayer;

            var dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);

            if (grid.BigOwners.Count() > 0)
            {
                var owner = grid.BigOwners.FirstOrDefault();
                if (dictionary.ContainsKey(owner))
                    return dictionary[owner];
            }
            return null;
        }
    }
}
