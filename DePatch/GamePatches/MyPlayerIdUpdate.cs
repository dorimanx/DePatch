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
            ctx.GetPattern(typeof(MySession).GetMethod("GetCheckpoint", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(MyPlayerIdUpdate).GetMethod(nameof(GetCheckpointPostfix), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void GetCheckpointPostfix(MyObjectBuilder_Checkpoint __result)
        {
            // check if request is from client connect and not from worldsave.
            if (__result.Clients == null)
                return;

            var originalresult = __result;

            try
            {
                int count = 0;
                __result.AllPlayersData.Dictionary = __result.AllPlayersData.Dictionary.Select(delegate (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b)
                {
                    MyObjectBuilder_Checkpoint.PlayerId key = b.Key;
                    if (key.ClientId != 0)
                        return b;
                    key.ClientId = b.Key.GetClientId();
                    count++;
                    return new KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>(key, b.Value);

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

                    Log.Info($"Forced saving Client ids of {count} players");
                }
            }
            catch
            {
                __result = originalresult;
                return;
            }
        }
    }
}
