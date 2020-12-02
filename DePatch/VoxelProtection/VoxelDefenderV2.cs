using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "PerformCutouts")]
    internal class VoxelDefenderV2
    {
        private static bool Prefix(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;
            if (!DePatchPlugin.Instance.Config.ProtectVoxels)
                return true;

            return false;
        }
    }
}
