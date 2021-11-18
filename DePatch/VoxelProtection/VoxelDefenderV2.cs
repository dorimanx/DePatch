using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;

namespace DePatch.VoxelProtection
{
    [PatchShim]

    internal static class VoxelDefenderV2
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyCubeGrid), typeof(VoxelDefenderV2), "PerformCutouts");
        }

        private static bool PerformCutouts()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (DePatchPlugin.Instance.Config.ProtectVoxels)
                return false;

            return true;
        }
    }
}
