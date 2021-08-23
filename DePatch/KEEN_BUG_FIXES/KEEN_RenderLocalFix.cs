using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game.Components;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_RenderLocalFix
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyRenderComponentBase).GetMethod("UpdateRenderObjectLocal");
            MethodInfo _patch = typeof(KEEN_RenderLocalFix).GetMethod(nameof(RenderLocalIgnore));
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        // Server should not care about render updates.
        public static bool RenderLocalIgnore()
        {
            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
                return false;

            return true;
        }
    }
}
