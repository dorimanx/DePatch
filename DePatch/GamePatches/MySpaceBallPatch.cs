using Sandbox.Definitions;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MySpaceBallPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MySpaceBall), "Init", typeof(MySpaceBallPatch), "PatchInitMethod", new[] { "objectBuilder", "cubeGrid" });
            ctx.Prefix(typeof(MySpaceBall), "RefreshPhysicsBody", typeof(MySpaceBallPatch), "PatchRefreshPhysicsBodyMethod");
        }

        private static void PatchInitMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MySpaceBallDefinition)__instance.BlockDefinition).MaxVirtualMass = 0f;
        }

        private static void PatchRefreshPhysicsBodyMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            __instance.VirtualMass = 0f;
        }
    }
}
