using Torch.Managers.PatchManager;
using Sandbox.Game.World;
using System.Reflection;
using DePatch.CoolDown;
using DePatch.PVEZONE;
using VRage.Utils;
using System;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class ServerAliveLog
    {
        private static int TickLog = 1;
        private static bool ServerBootLoopStart = true;

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MySession).GetMethod("UpdateComponents", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).
                Suffixes.Add(typeof(ServerAliveLog).GetMethod(nameof(UpdateLOG), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateLOG()
        {
            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
            {
                if (ServerBootLoopStart)
                {
                    if (DePatchPlugin.Instance.Config.DelayShootingOnBootTime <= 0)
                        DePatchPlugin.Instance.Config.DelayShootingOnBootTime = 1;

                    int LoopCooldown = DePatchPlugin.Instance.Config.DelayShootingOnBootTime * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, LoopCooldown);
                    ServerBootLoopStart = false;
                    MyPVESafeZoneAction.BootTickStarted = true;
                }

                if (MyPVESafeZoneAction.BootTickStarted)
                {
                    // loop for X sec after boot to block weapons.
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, out var remainingSecondsBoot);

                    if (remainingSecondsBoot < 1)
                        MyPVESafeZoneAction.BootTickStarted = false;
                }
            }

            // send server alive log to torch log every 30sec.
            if (DePatchPlugin.Instance.Config.LogTracker)
            {
                if (++TickLog > 60)
                {
                    // loop for 30 sec and print new update to KEEN log and Torch console.
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopAliveLogRequestID, null, out var remainingSecondsToNextLog);

                    if (remainingSecondsToNextLog < 1)
                    {
                        // arm new timer.
                        int LoopCooldown = 30 * 1000;
                        CooldownManager.StartCooldown(SteamIdCooldownKey.LoopAliveLogRequestID, null, LoopCooldown);

                        // write to keen log.
                        MyLog.Default.Log(MyLogSeverity.Info, "Server Status: ALIVE", Array.Empty<object>());
                    }
                    TickLog = 1;
                }
            }
        }
    }
}
