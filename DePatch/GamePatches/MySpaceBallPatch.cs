using System;
using System.Reflection;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MySpaceBallPatch
    {
        internal static readonly MethodInfo Update = typeof(MySpaceBall).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
        {
            typeof(MyObjectBuilder_CubeBlock),
            typeof(MyCubeGrid)
        }, new ParameterModifier[0]);

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(Update).Prefixes.Add(typeof(MySpaceBallPatch).GetMethod(nameof(PatchInitMethod), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method"));
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
