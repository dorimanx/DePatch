using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NLog;
using Sandbox.Game.World;
using VRage.Game;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MySession), "GetCheckpoint")]
    internal class MyPlayerIdFix
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static void Postfix(MyObjectBuilder_Checkpoint __result)
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
            }).ToDictionary((KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Key, (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Value);
            if (count > 0)
                Log.Info($"Forced saving Client ids of {count} players");
        }
    }
}
