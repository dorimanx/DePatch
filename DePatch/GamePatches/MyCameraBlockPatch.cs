using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyCameraBlock), "Init")]
    internal class MyCameraBlockPatch
    {
        private static bool Prefix(MyCameraBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            __instance.BlockDefinition.RaycastDistanceLimit = DePatchPlugin.Instance.Config.RaycastLimit;
            return true;
        }
    }
}
