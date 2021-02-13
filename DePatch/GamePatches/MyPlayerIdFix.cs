using NLog;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace DePatch.GamePatches
{
    [HarmonyLib.HarmonyPatch(typeof(MySession), nameof(MySession.GetCheckpoint))]
    class MyPlayerIdFix
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        static void Postfix(MyObjectBuilder_Checkpoint __result)
        {
            var count = 0;
            __result.AllPlayersData.Dictionary = __result.AllPlayersData.Dictionary.Select(b => {
                var item = b.Key;
                if (item.ClientId != 0) return b;
                item.ClientId = b.Key.GetClientId();
                count++;
                return new KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player>(item, b.Value); 
            }).ToDictionary(b => b.Key, b => b.Value);
            if (count > 0)
                Log.Info($"Forced saving Client ids of {count} players");
        }
    }
}
