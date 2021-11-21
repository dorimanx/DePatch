using Sandbox.Definitions;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyMassBlockPatch
    {
        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyVirtualMass), "Init", typeof(MyMassBlockPatch), nameof(MassInit), new[] { "objectBuilder", "cubeGrid" });

        private static void MassInit(MyVirtualMass __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MyVirtualMassDefinition)__instance.BlockDefinition).VirtualMass = 0f;
        }
    }
}
