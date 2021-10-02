using Sandbox.Definitions;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyMassBlockPatch
    {
        private static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyVirtualMass).GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[2]
            {
                typeof(MyObjectBuilder_CubeBlock),
                typeof(MyCubeGrid)
            }, new ParameterModifier[0])).
            Prefixes.Add(typeof(MyMassBlockPatch).GetMethod(nameof(MassInit), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

        private static void MassInit(MyVirtualMass __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MyVirtualMassDefinition)__instance.BlockDefinition).VirtualMass = 0f;
        }
    }
}
