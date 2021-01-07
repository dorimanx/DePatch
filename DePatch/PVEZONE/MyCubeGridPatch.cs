using DePatch.CoolDown;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch.PVEZONE
{
    [HarmonyPatch(typeof(MyCubeGrid), "UpdateAfterSimulation100")]

    internal class MyCubeGridPatch
    {
        private static readonly SteamIdCooldownKey LoopRequestID = new SteamIdCooldownKey(76000000000000003);
        private static readonly SteamIdCooldownKey BootRequestID = new SteamIdCooldownKey(76000000000000004);
        private static int LoopCooldown = 5;
        private static readonly int BootCooldown = 90 * 1000;
        public static bool ServerBoot = true;
        public static bool ServerBootLoopStart = true;

        private static bool Prefix(MyCubeGrid __instance)
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled && DePatchPlugin.Instance.Config.Enabled)
            {
                try
                {
                    if (__instance != null && !PVEGrid.Grids.ContainsKey(__instance))
                    {
                        PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
                    }

                    if (__instance != null && DePatchPlugin.Instance.Config.PveZoneEnabled2 && !PVEGrid2.Grids2.ContainsKey(__instance))
                    {
                        PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));
                    }

                    if (__instance == null)
                    {
                        PVEGrid.Grids.Remove(__instance);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                            PVEGrid2.Grids2.Remove(__instance);
                    }
                    else
                    {
                        if (!ServerBoot)
                        {
                            /// loop for 5 ticks till next grid add / remove
                            if (++LoopCooldown <= 6)
                                return true;
                            LoopCooldown = 0;
                        }
                        else if (ServerBoot)
                        {
                            // Allow fast grid add to dictonary on boot. for 90sec
                            if (CooldownManager.CheckCooldown(BootRequestID, null, out long remainingSecondsBoot))
                            {
                                if (ServerBootLoopStart)
                                {
                                    CooldownManager.StartCooldown(BootRequestID, null, BootCooldown);
                                    ServerBootLoopStart = false;
                                }
                            }
                            else
                                ServerBoot = false;
                        }

                        var pVEGrid = PVEGrid.Grids[__instance];
                        var flag = pVEGrid.InPVEZone();
                        var flag2 = PVE.EntitiesInZone.Contains(__instance.EntityId);
                        if (!(flag && flag2))
                        {
                            if (__instance != null && !flag && flag2)
                            {
                                PVE.EntitiesInZone.Remove(__instance.EntityId);
                                pVEGrid?.OnGridLeft();
                            }
                            if (__instance != null && flag && !flag2)
                            {
                                PVE.EntitiesInZone.Add(__instance.EntityId);
                                pVEGrid?.OnGridEntered();
                            }
                        }

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                        {
                            var pVEGrid2 = PVEGrid2.Grids2[__instance];
                            var flag3 = pVEGrid2.InPVEZone2();
                            var flag4 = PVE.EntitiesInZone2.Contains(__instance.EntityId);
                            if (!(flag3 && flag4))
                            {
                                if (__instance != null && !flag3 && flag4)
                                {
                                    PVE.EntitiesInZone2.Remove(__instance.EntityId);
                                    pVEGrid2?.OnGridLeft2();
                                }
                                if (__instance != null && flag3 && !flag4)
                                {
                                    PVE.EntitiesInZone2.Add(__instance.EntityId);
                                    pVEGrid2?.OnGridEntered2();
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return true;
        }
    }
}
