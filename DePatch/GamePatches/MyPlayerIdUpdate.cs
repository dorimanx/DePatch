using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyPlayerIdUpdate
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public static readonly Dictionary<long, DateTime> cooldowns = new Dictionary<long, DateTime>();

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MySession).GetMethod("GetCheckpoint", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).
                Suffixes.Add(typeof(MyPlayerIdUpdate).GetMethod(nameof(GetCheckpointPostfix), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void GetCheckpointPostfix(ref MyObjectBuilder_Checkpoint __result)
        {
            if (__result is null || __result.AllPlayersData is null || __result.AllPlayersData.Dictionary is null || __result.AllPlayersData.Dictionary.Count is 0)
                return;

            var originalresult = __result;

            try
            {
                lock (__result)
                {
                    int count = 0;

                    __result.AllPlayersData.Dictionary = __result.AllPlayersData.Dictionary.Select(delegate (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b)
                    {
                        MyObjectBuilder_Checkpoint.PlayerId key = b.Key;
                        KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> result;
                        if (key.ClientId != 0UL)
                            result = b;
                        else
                        {
                            var PlayerSteamID = key.GetClientId();
                            if (PlayerSteamID > 0)
                            {
                                key.ClientId = PlayerSteamID;
                                count++;
                            }
                            result = new KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>(key, b.Value);
                        }
                        return result;
                    }).ToDictionary((KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Key,
                                    (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Value);
                    if (count > 0)
                    {
                        var TimerId = 123456789;

                        if (cooldowns.ContainsKey(TimerId))
                        {
                            var time = cooldowns[TimerId];
                            var neededTime = time.AddSeconds(5);
                            if (neededTime > DateTime.Now)
                                return; // dont spam log for many new clients.

                            cooldowns.Remove(TimerId);
                        }
                        if (!cooldowns.ContainsKey(TimerId))
                            cooldowns.Add(TimerId, DateTime.Now);

                        if (__result.Clients != null)
                            Log.Info($"Forced saving Client ids of {count} players on player join");
                        else
                            Log.Info($"Forced saving Client ids of {count} players to World Save");
                    }
                    return;
                }
            }
            catch
            {
                Log.Info($"There was error in MyPlayerIdUpdate code, report to Dorimanx");
            }

            // in case something went horribly wrong, restore original data.
            __result = originalresult;
        }
    }
}
