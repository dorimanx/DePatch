using DePatch.CoolDown;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Utils;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]

    public static class KEEN_ValidationFailedFix
    {
        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyMultiplayerServerBase).GetMethod("ValidationFailed", BindingFlags.Instance | BindingFlags.Public)).
                Prefixes.Add(typeof(KEEN_ValidationFailedFix).GetMethod(nameof(ValidationFailedLOG), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static bool ValidationFailedLOG(ulong clientId, bool kick = true, string additionalInfo = null, bool stackTrace = true)
        {
            if (DePatchPlugin.Instance.Config.ValidationFailedSuspend)
            {
                int LoopCooldown = 120 * 1000; // 120 sec
                SteamIdCooldownKey ValidationFailedRequestID = new SteamIdCooldownKey(clientId);

                _ = CooldownManager.CheckCooldown(ValidationFailedRequestID, null, out var remainingSecondsToNextLog);

                if (remainingSecondsToNextLog < 1)
                {
                    CooldownManager.StartCooldown(ValidationFailedRequestID, null, LoopCooldown);

                    string msg = MySession.Static.Players.TryGetIdentityNameFromSteamId(clientId) + (kick ? " was trying to cheat!" : "'s action was blocked.");
                    MyLog.Default.WriteLine(msg);

                    if (additionalInfo != null)
                        MyLog.Default.WriteLine(additionalInfo);

                    if (stackTrace)
                        MyLog.Default.WriteLine(Environment.StackTrace);
                }
                return false;
            }
            return true;
        }
    }
}
