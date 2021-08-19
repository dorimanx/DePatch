using Sandbox.Game.Entities;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.VoxelProtection
{
    [PatchShim]

    internal static class VoxelDefenderV2
    {
        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyCubeGrid).GetMethod("PerformCutouts", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(VoxelDefenderV2).GetMethod(nameof(PerformCutouts), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
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
