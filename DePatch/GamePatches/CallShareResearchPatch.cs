using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.SessionComponents;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class CallShareResearchPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MySessionComponentResearch), "CallShareResearch", typeof(CallShareResearchPatch), nameof(CallShareResearch));
            ctx.Prefix(typeof(MySessionComponentResearch), "OnBlockBuilt", typeof(CallShareResearchPatch), nameof(OnBlockBuilt));
        }

        public static bool CallShareResearch(long toIdentity)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.BlockShareResearch)
                return true;

            return false;
        }

        private static bool OnBlockBuilt(MySessionComponentResearch __instance, MyCubeGrid grid, MySlimBlock block, ref bool handWelded)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.BlockShareResearch)
                return true;

            if (!handWelded)
                return true;

            long builtBy = block.BuiltBy;
            MyDefinitionId id = block.BlockDefinition.Id;

            __instance.UnlockResearch(builtBy, id, builtBy);

            return false;
        }
    }
}
