using DePatch.CoolDown;
using VRage.Utils;
using System;

namespace DePatch.GamePatches
{
    public static class ServerAliveLog
    {
        private static int TickLog = 1;

        public static void UpdateLOG()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

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
