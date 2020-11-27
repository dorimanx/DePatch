using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCameraBlock), "Init")]
    internal class MyCameraBlockPatch
    {
        private static void Prefix(MyCameraBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            __instance.BlockDefinition.RaycastDistanceLimit = DePatchPlugin.Instance.Config.RaycastLimit;
        }
    }
}
