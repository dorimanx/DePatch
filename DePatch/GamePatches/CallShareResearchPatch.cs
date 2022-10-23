using Sandbox.Game.SessionComponents;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class CallShareResearchPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MySessionComponentResearch), "CallShareResearch", typeof(CallShareResearchPatch), nameof(CallShareResearch));
        }

        public static bool CallShareResearch(long toIdentity)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.BlockShareResearch)
                return true;

            return false;
        }
    }
}
