using DePatch.CoolDown;
using HarmonyLib;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System;

namespace DePatch.PVEZONE
{
    [HarmonyPatch(typeof(MyCubeGrid), "UpdateAfterSimulation100")]

    internal class MyCubeGridPatch
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly SteamIdCooldownKey BootRequestID = new SteamIdCooldownKey(76000000000000003);
        private static int LoopCooldown = 3;
        private static readonly int BootCooldown = 180 * 1000;
        private static bool ServerBoot = true;
        private static bool ServerBootLoopStart = true;

        private static void Prefix(MyCubeGrid __instance)
        {
            if (DePatchPlugin.Instance.Config.PveZoneEnabled && DePatchPlugin.Instance.Config.Enabled)
            {
                try
                {
                    if (__instance != null)
                    {
                        var IsItNPC = (__instance.BigOwners.Count > 0) ? __instance.BigOwners[0] : 0L;
                        var NPC_Grid = false;

                        if (IsItNPC != 0L && MySession.Static.Players.IdentityIsNpc(IsItNPC))
                            NPC_Grid = true;

                        if (NPC_Grid && PVEGrid.Grids.ContainsKey(__instance))
                            PVEGrid.Grids.Remove(__instance);

                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && NPC_Grid && PVEGrid2.Grids2.ContainsKey(__instance))
                            PVEGrid2.Grids2.Remove(__instance);

                        if (NPC_Grid)
                            return;
                    }

                    if (__instance != null && !PVEGrid.Grids.ContainsKey(__instance))
                        PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));

                    if (__instance != null && DePatchPlugin.Instance.Config.PveZoneEnabled2 && !PVEGrid2.Grids2.ContainsKey(__instance))
                        PVEGrid2.Grids2.Add(__instance, new PVEGrid2(__instance));

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
                            /// loop for 3 ticks till next grid add / remove
                            if (++LoopCooldown <= 3)
                                return;
                            LoopCooldown = 0;
                        }
                        else if (ServerBoot)
                        {
                            if (ServerBootLoopStart)
                            {
                                CooldownManager.StartCooldown(BootRequestID, null, BootCooldown);
                                ServerBootLoopStart = false;
                            }

                            // Allow fast grid add to dictonary on boot. for 180sec
                            if (CooldownManager.CheckCooldown(BootRequestID, null, out var remainingSecondsBoot))
                            {
                            }

                            if (remainingSecondsBoot < 2)
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
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
