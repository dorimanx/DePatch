using System;
using System.Linq;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRageMath;

namespace DePatch
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
            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
            {
                return true;
            }
            if (entity is MyCubeGrid && action == MySafeZoneAction.Shooting)
            {
                if (PVE.EntitiesInZone.Contains(entity.EntityId))
                {
                    __result = false;
                    return false;
                }
            }
            else
            {
                if (entity as MyCharacter != null && action == MySafeZoneAction.Shooting)
                {
                    MyPlayer myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == (entity as MyCharacter).GetPlayerIdentityId());
                    if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
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
