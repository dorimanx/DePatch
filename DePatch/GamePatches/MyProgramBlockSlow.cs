using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Utils;
using HarmonyLib;
using Sandbox.Game.World;

namespace DePatch
{
    [HarmonyPatch(typeof(MyProgrammableBlock), "Run")]
    public static class MyProgramBlockSlow
    {
        private static Dictionary<long, int> timers1 = new Dictionary<long, int>();
        private static Dictionary<long, int> timers10 = new Dictionary<long, int>();
        private static Dictionary<long, int> timers100 = new Dictionary<long, int>();

        static HashSet<MyStringHash> ignoredTimers = new HashSet<MyStringHash>();

        public static void Init() {
            var subs = DePatchPlugin.Instance.Config.SlowPBIgnored.Split(new[] {",", " "}, StringSplitOptions.None);
            foreach (var x in subs) {
                if (x.Length > 0) {
                    DePatchPlugin.Log.Error("Added " + x + " to ignored PB's");
                    ignoredTimers.Add(MyStringHash.GetOrCompute(x));
                }
            }
        }
        private static bool Slow(long id, Dictionary<long, int> timers, int howSlow)
        {

            if (timers.TryGetValue(id, out int timer))
            {
                if (timer > 0)
                {
                    timers[id] = timer - 1;
                    return false;
                }
                else
                {
                    timers[id] = howSlow - 1;
                    return true;
                }
            }
            else
            {
                timers.Add(id, howSlow - 1);
                return true;
            }
        }

        public static bool Prefix(MyProgrammableBlock __instance, UpdateType updateSource)
        {
            if (DePatchPlugin.Instance.Config.Enabled && __instance.Enabled)
            {
                if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
                {
                    if (__instance.IsFunctional && !MySession.Static.Players.IsPlayerOnline(__instance.OwnerId))
                    {
                        if (PlayersUtility.KeepBlockOffProgramBlocks(__instance))
                            __instance.Enabled = false;
                    }
                }

                if (!DePatchPlugin.Instance.Config.SlowPBEnabled || __instance.Enabled == false)
                    return true;

                if (ignoredTimers.Contains(__instance.BlockDefinition.Id.SubtypeId))
                    return true;

                if (updateSource == UpdateType.Update1)
                {
                    var sl = DePatchPlugin.Instance.Config.SlowPBUpdate1;
                    if (sl == 1) { return true; } else { return Slow(__instance.EntityId, timers1, sl); }
                }
                else if (updateSource == UpdateType.Update10)
                {
                    var sl = DePatchPlugin.Instance.Config.SlowPBUpdate10;
                    if (sl == 1) { return true; } else { return Slow(__instance.EntityId, timers10, sl); }
                }
                else if (updateSource == UpdateType.Update100)
                {
                    var sl = DePatchPlugin.Instance.Config.SlowPBUpdate100;
                    if (sl == 1) { return true; } else { return Slow(__instance.EntityId, timers100, sl); }
                }
            }
            return true;
        }
    }
}