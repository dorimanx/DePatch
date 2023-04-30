using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;
using VRage.Game.Components;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]

    public static class KEEN_RenderUpdateFix
    {
        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyThrust), typeof(KEEN_RenderUpdateFix), nameof(RenderUpdate));

        private static bool RenderUpdate(MyThrust __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (__instance.Render == null || __instance.Render.GetType() == typeof(MyNullRenderComponent) || __instance.Render.GetType() != typeof(MyRenderComponentThrust))
                return false;
            else
                return true;
        }
    }
}