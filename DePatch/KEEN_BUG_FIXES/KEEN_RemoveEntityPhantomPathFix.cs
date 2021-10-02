using Havok;
using Sandbox.Game.Entities;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI.Ingame;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_RemoveEntityPhantomPathFix
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MySafeZone).GetMethod("RemoveEntityPhantom", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEEN_RemoveEntityPhantomPathFix).GetMethod(nameof(RemoveEntityPhantomPath), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool RemoveEntityPhantomPath(HkRigidBody body, IMyEntity entity)
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                // checking for null here saves us from crash.
                if (entity is null || body is null)
                    return false;
            }
            return true;
        }
    }
}