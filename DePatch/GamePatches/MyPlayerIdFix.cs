using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    //[HarmonyPatch(typeof(MySession), "GetCheckpoint")]
    [PatchShim]

    internal static class MyPlayerIdFix
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MySession).GetMethod("GetCheckpoint", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(MyPlayerIdFix).GetMethod(nameof(GetCheckpoint), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void GetCheckpoint(MyObjectBuilder_Checkpoint __result)
        {
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
                }).ToDictionary((KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Key, (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> b) => b.Value);
                if (count > 0)
                    Log.Info($"Forced saving Client ids of {count} players");
            } catch
            {
            }
        }
    }
}
