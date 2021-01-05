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
    [HarmonyPatch(typeof(MySessionComponentSafeZones), "IsActionAllowed", new Type[]
    {
        typeof(MyEntity),
        typeof(MySafeZoneAction),
        typeof(long),
        typeof(ulong)
    })]
    internal class MyTurretPveDamageFix
    {
        private static bool Prefix(MySessionComponentSafeZones __instance, MyEntity entity, MySafeZoneAction action, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                return true;
            }
            if (entity is MyCubeGrid && action == MySafeZoneAction.Shooting)
            {
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                {
                    var zone1 = false;
                    var zone2 = false;
                    if (PVE.EntitiesInZone.Contains(entity.EntityId))
                        zone1 = true;
                    if (PVE.EntitiesInZone2.Contains(entity.EntityId))
                        zone2 = true;

                    if (zone1 || zone2)
                    {
                        __result = false;
                        return false;
                    }
                }
                else
                {
                    if (PVE.EntitiesInZone.Contains(entity.EntityId))
                    {
                        __result = false;
                        return false;
                    }
                }
            }
            else
            {
                if (entity as MyCharacter != null && action == MySafeZoneAction.Shooting)
                {
                    var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == (entity as MyCharacter).GetPlayerIdentityId());

                    if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    {
                        var zone1 = false;
                        var zone2 = false;
                        if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone1 = true;
                        if (PVE.PVESphere2.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone2 = true;

                        if (zone1 || zone2)
                        {
                            __result = false;
                            return false;
                        }
                    }
                    else
                    {
                        if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                        {
                            __result = false;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
