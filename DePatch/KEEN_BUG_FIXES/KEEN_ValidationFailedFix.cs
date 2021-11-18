﻿using DePatch.CoolDown;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System;
using Torch.Managers.PatchManager;
using VRage.Utils;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]

    public static class KEEN_ValidationFailedFix
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyMultiplayerServerBase), typeof(KEEN_ValidationFailedFix), "ValidationFailed");
        }

        private static bool ValidationFailed(ulong clientId, bool kick = true, string additionalInfo = null, bool stackTrace = true)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

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