using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using System.Collections.Generic;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;

namespace DePatch.KEEN_BUG_FIXES
{
    /* // Should be Fixed NOW.
    [PatchShim]
    public static class KEEN_AntennaCrashFix
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyAntennaSystem).GetMethod("GetEntityReceivers", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEEN_AntennaCrashFix).GetMethod(nameof(GetEntityReceiversCheck), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool GetEntityReceiversCheck(MyEntity entity, ref HashSet<MyDataReceiver> output, long playerId)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                if (entity == null)
                {
                    output.Clear();
                    return false;
                }
            }
            return true;
        }
    }
    */
}
