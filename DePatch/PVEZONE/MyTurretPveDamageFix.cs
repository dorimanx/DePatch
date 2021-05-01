using System.Linq;
using DePatch.CoolDown;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Weapons;
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
        private static bool ServerBootLoopStart = true;

        private static bool Prefix(MySessionComponentSafeZones __instance, MyEntity entity, MySafeZoneAction action, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            if (entity is MyCubeGrid grid)
            {
                switch (action)
                {
                    case MySafeZoneAction.Building:
                        {
                            var myPlayerID = MySession.Static.Players.TryGetIdentityId(user);
                            if (myPlayerID == 0 || myPlayerID < 0)
                                return true;

                            var myPlayerBuilding = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == myPlayerID);

                            if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                            {
                                if (entity is MyWelder Welder) return true;

                                // if found will return false this why !PVE.CheckEntityInZone
                                if (myPlayerBuilding != null && !PVE.CheckEntityInZone(myPlayerBuilding, ref __result))
                                {
                                    if (grid.IsFriendlyPlayer(user)) return true;

                                    return false;
                                }
                            }
                            else if (myPlayerBuilding != null && PVE.PVESphere.Contains(myPlayerBuilding.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            {
                                if (entity is MyWelder Welder) return true;

                                if (grid.IsFriendlyPlayer(user)) return true;

                                return false;
                            }
                            return true;
                        }
                    case MySafeZoneAction.Shooting:
                        {
                            if (ServerBoot)
                            {
                                if (ServerBootLoopStart)
                                {
                                    CooldownManager.StartCooldown(LoopRequestID, null, LoopCooldown);
                                    ServerBootLoopStart = false;
                                }

                                // loop for 240 sec after boot to block weapons.
                                if (CooldownManager.CheckCooldown(LoopRequestID, null, out var remainingSecondsBoot))
                                {
                                }

                                if (remainingSecondsBoot < 2)
                                    ServerBoot = false;

                                // block weapons
                                return false;
                            }

                            // if found will return false
                            if (!PVE.CheckEntityInZone(entity, ref __result))
                            {
                                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                                    return true;

                                return false;
                            }
                            return true;
                        }
                }
            }

            if (action != MySafeZoneAction.Shooting)
            {
                __result = true;
                return true;
            }

            if (entity is MyCharacter character)
            {
                if (character == null)
                    return true;

                if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                    return true;

                var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find(b => b.Identity.IdentityId == character.GetPlayerIdentityId());

                // if found will return false
                if (myPlayer != null && DePatchPlugin.Instance.Config.PveZoneEnabled2)
                    return PVE.CheckEntityInZone(myPlayer, ref __result);

                if (myPlayer != null && PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    return false;
            }

            __result = true;
            return true;
        }
    }
}
