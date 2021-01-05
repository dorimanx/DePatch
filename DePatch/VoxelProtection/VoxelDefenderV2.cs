using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch.VoxelProtection
{
    [HarmonyPatch(typeof(MyCubeGrid), "PerformCutouts")]
    internal class VoxelDefenderV2
    {
        private static bool Prefix()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;
            return !DePatchPlugin.Instance.Config.ProtectVoxels;
        }
    }
}
