using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyCameraBlockPatch
    {
        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyCameraBlock), "Init", typeof(MyCameraBlockPatch), nameof(CameraInit), new[] { "objectBuilder", "cubeGrid" });

        private static void CameraInit(MyCameraBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            __instance.BlockDefinition.RaycastDistanceLimit = DePatchPlugin.Instance.Config.RaycastLimit;
        }
    }
}
