using System;
using System.Linq;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRageMath;

namespace DePatch.PVEZONE
{
    [HarmonyPatch(typeof(MySessionComponentSafeZones), "IsActionAllowed", typeof(MyEntity), typeof(MySafeZoneAction), typeof(long), typeof(ulong))]
    internal class MyTurretPveDamageFix
    {
        private static bool Prefix(MySessionComponentSafeZones __instance, MyEntity entity, MySafeZoneAction action, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                return true;
            }

            switch (entity)
            {
                case MyCubeGrid grid when action == MySafeZoneAction.Building:
                {
                    if (grid.IsFriendlyPlayer(user)) return true;
                    var zone1 = false;
                    var zone2 = false;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.EntitiesInZone.Contains(entity.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(entity.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2) return true;
                    __result = false;
                    return false;
                }
                case MyCubeGrid _ when action == MySafeZoneAction.Shooting:
                {
                    var zone1 = false;
                    var zone2 = false;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.EntitiesInZone.Contains(entity.EntityId))
                        zone1 = true;
                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVE.EntitiesInZone2.Contains(entity.EntityId))
                        zone2 = true;

                    if (!zone1 && !zone2) return true;
                    __result = false;
                    return false;
                }
            }

            if (!(entity is MyCharacter) || action != MySafeZoneAction.Shooting) return true;
            var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == ((MyCharacter) entity).GetPlayerIdentityId());

            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                var zone1 = false;
                var zone2 = false;
                if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    zone1 = true;
                if (PVE.PVESphere2.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    zone2 = true;

                if (!zone1 && !zone2) return true;
                __result = false;
                return false;
            }

            if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) !=
                ContainmentType.Contains) return true;
            __result = false;
            return false;
        }
    }
}
