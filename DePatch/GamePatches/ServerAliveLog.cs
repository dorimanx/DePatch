using NLog;
using Torch.Managers.PatchManager;
using Sandbox.Game.World;
using System.Reflection;
using DePatch.CoolDown;
using DePatch.PVEZONE;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class ServerAliveLog
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static bool LoopAliveLogStart = true;
        private static int TickLog = 1;
        private static bool ServerBootLoopStart = true;

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MySession).GetMethod("UpdateComponents", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
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

                    if (remainingSecondsBoot < 3)
                        MyPVESafeZoneAction.BootTickStarted = false;
                }
            }

            // send server alive log to torch log every 90sec +10 max.
            if (DePatchPlugin.Instance.Config.LogTracker)
            {
                if (++TickLog > 10)
                {
                    if (LoopAliveLogStart)
                    {
                        int LoopCooldown = 90 * 1000;
                        CooldownManager.StartCooldown(SteamIdCooldownKey.LoopAliveLogRequestID, null, LoopCooldown);
                        LoopAliveLogStart = false;
                    }

                    // loop for 90 sec and print new update to log.
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopAliveLogRequestID, null, out var remainingSecondsToNextLog);

                    if (remainingSecondsToNextLog < 2)
                    {
                        Log.Info("Server Status: ALIVE");
                        LoopAliveLogStart = true;
                    }
                    TickLog = 1;
                }
            }
        }
    }
}
