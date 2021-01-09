using System.Linq;
using DePatch.CoolDown;
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
        private static readonly SteamIdCooldownKey LoopRequestID = new SteamIdCooldownKey(76000000000000001);
        private static readonly int LoopCooldown = 240 * 1000;
        private static bool ServerBoot = true;
        public static bool ServerBootLoopStart = true;

        private static bool Prefix(MySessionComponentSafeZones __instance, MyEntity entity, MySafeZoneAction action, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            switch (entity)
            {
                case MyCubeGrid grid when action == MySafeZoneAction.Building:
                    {
                        var myPlayerID = MySession.Static.Players.TryGetIdentityId(user);
                        var myPlayerBuilding = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == myPlayerID);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                        {
                            // if found will return false this why !PVE.CheckEntityInZone
                            if (myPlayerBuilding != null && !PVE.CheckEntityInZone(myPlayerBuilding, ref __result))
                            {
                                if (grid.IsFriendlyPlayer(user))
                                    return true;
                                else
                                    return false;
                            }
                        }
                        else
                        {                          
                            if (myPlayerBuilding != null && PVE.PVESphere.Contains(myPlayerBuilding.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            {
                                if (grid.IsFriendlyPlayer(user))
                                    return true;
                                else
                                    return false;
                            }
                        }
                        return true;
                    }
                case MyCubeGrid _ when action == MySafeZoneAction.Shooting:
                    {
                        if (ServerBoot)
                        {
                            // loop for 240 sec after boot to block weapons.
                            if (CooldownManager.CheckCooldown(LoopRequestID, null, out long remainingSecondsBoot))
                            {
                                if (ServerBootLoopStart)
                                {
                                    CooldownManager.StartCooldown(LoopRequestID, null, LoopCooldown);
                                    ServerBootLoopStart = false;
                                }
                                var LoopTimer = remainingSecondsBoot;
                                // block weapons
                                return false;
                            }
                            else
                                ServerBoot = false;
                        }

                        return PVE.CheckEntityInZone(entity, ref __result);
                    }
            }

            if (action != MySafeZoneAction.Shooting) return true;

            if (entity is MyCharacter character)
            {
                var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == character.GetPlayerIdentityId());
                if (myPlayer != null && DePatchPlugin.Instance.Config.PveZoneEnabled2)
                {
                    // if found will return false
                    return PVE.CheckEntityInZone(myPlayer, ref __result);
                }

                if (myPlayer != null && PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    return false;
            }

            __result = false;
            return false;
        }
    }
}
