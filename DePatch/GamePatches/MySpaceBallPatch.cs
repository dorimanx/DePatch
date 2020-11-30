using System;
using System.Reflection;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch
{
    [PatchShim]
    public static class MySpaceBallPatch
    {
        internal static readonly MethodInfo Update = typeof(MySpaceBall).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
        {
            typeof(MyObjectBuilder_CubeBlock),
            typeof(MyCubeGrid)
        }, new ParameterModifier[0]);

        internal static readonly MethodInfo UpdatePatch = typeof(MySpaceBallPatch).GetMethod("PatchMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        internal static readonly MethodInfo UpdateMass = typeof(MySpaceBall).GetMethod("RefreshPhysicsBody", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static readonly MethodInfo UpdateMassPatch = typeof(MySpaceBallPatch).GetMethod("PatchMassMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(Update).Prefixes.Add(UpdatePatch);
            ctx.GetPattern(UpdateMass).Prefixes.Add(UpdateMassPatch);
        }

        private static void PatchMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MySpaceBallDefinition)__instance.BlockDefinition).MaxVirtualMass = 0f;
        }

        private static void PatchMassMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            __instance.VirtualMass = 0f;
        }
    }
}
