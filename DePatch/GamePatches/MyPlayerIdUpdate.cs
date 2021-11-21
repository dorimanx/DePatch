using System;
using System.Collections.Generic;
using System.Linq;
using DePatch.CoolDown;
using NLog;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    internal static class MyPlayerIdUpdate
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            ctx.Suffix(typeof(MySession), "GetCheckpoint", typeof(MyPlayerIdUpdate), nameof(GetCheckpointPostfix));
        }

        private static void GetCheckpointPostfix(ref MyObjectBuilder_Checkpoint __result, ref bool isClientRequest)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PlayersIdUpdate)
                return;

            if (__result is null || __result.AllPlayersData is null || __result.AllPlayersData.Dictionary is null || __result.AllPlayersData.Dictionary.Count == 0)
                return;

            // if function called on player join, skip this work. it's needed only for world save.
            if (isClientRequest)
                return;

            int count = 0;
            var NewDictionary = new Dictionary<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>();
            var idents = MySession.Static.Players.GetAllIdentities().ToList();
            var AllIdentities = new HashSet<long>();
            var cutoff = DateTime.Now - TimeSpan.FromDays(7);

            foreach (var identity in idents)
            {
                if (identity == null || MySession.Static.Players.IdentityIsNpc(identity.IdentityId))
                    continue;

                if (identity.LastLoginTime < cutoff)
                    AllIdentities.Add(identity.IdentityId);
            }

            foreach (var Dictionary in __result.AllPlayersData.Dictionary)
            {
                if (Dictionary.Value == null)
                    continue;

                if (AllIdentities.Contains(Dictionary.Value.IdentityId))
                    NewDictionary.Add(Dictionary.Key, Dictionary.Value);
            }

            foreach (var DictionaryItem in NewDictionary)
            {
                var PlayerSteamID = MySession.Static?.Players.TryGetSteamId(DictionaryItem.Value.IdentityId);

                if (PlayerSteamID != null && PlayerSteamID > 0)
                {
                    var key = DictionaryItem.Key;
                    key.ClientId = (ulong)PlayerSteamID;

                    __result.AllPlayersData.Dictionary.Remove(DictionaryItem.Key);
                    __result.AllPlayersData.Dictionary.Add(key, DictionaryItem.Value);

                    count++;
                }
            }

            NewDictionary.Clear();

            if (count > 0)
            {
                _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopPlayerIdsSaveRequestID, null, out var remainingSecondsToNextLog);

                if (remainingSecondsToNextLog < 1)
                {
                    // arm new timer.
                    int LoopCooldown = 10 * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopPlayerIdsSaveRequestID, null, LoopCooldown);

                    Log.Info($"Forced saving Client ids of {count} players to World Save");
                }
            }
        }
    }
}
