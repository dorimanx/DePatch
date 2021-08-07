using NLog;
using Torch.Managers.PatchManager;
using Sandbox.Game.World;
using System.Reflection;
using DePatch.CoolDown;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class ServerAliveLog
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly SteamIdCooldownKey LoopAliveLogRequestID = new SteamIdCooldownKey(76000000000000010);
        private static bool LoopAliveLogStart = true;
        private static int TickLog = 1;

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MySession).GetMethod("UpdateComponents", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(ServerAliveLog).GetMethod(nameof(UpdateLOG), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void UpdateLOG()
        {
            // send server alive log to torch log every 90sec +10 max.
            if (DePatchPlugin.Instance.Config.LogTracker)
            {
                if (++TickLog > 10)
                {
                    if (LoopAliveLogStart)
                    {
                        int LoopCooldown = 90 * 1000;
                        CooldownManager.StartCooldown(LoopAliveLogRequestID, null, LoopCooldown);
                        LoopAliveLogStart = false;
                    }

                    // loop for 90 sec and print new update to log.
                    _ = CooldownManager.CheckCooldown(LoopAliveLogRequestID, null, out var remainingSecondsToNextLog);

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
