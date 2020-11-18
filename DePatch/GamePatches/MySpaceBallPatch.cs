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
        internal static readonly MethodInfo Update = typeof(MySpaceBall).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public, (Binder)null, new Type[2]
        {
            typeof(MyObjectBuilder_CubeBlock),
            typeof(MyCubeGrid)
        }, new ParameterModifier[0]);

        internal static readonly MethodInfo UpdatePatch;

        internal static readonly MethodInfo UpdateMass;

        internal static readonly MethodInfo UpdateMassPatch;

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern((MethodBase)MySpaceBallPatch.Update).Prefixes.Add(MySpaceBallPatch.UpdatePatch);
            ctx.GetPattern((MethodBase)MySpaceBallPatch.UpdateMass).Prefixes.Add(MySpaceBallPatch.UpdateMassPatch);
        }

        public static void PatchMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MySpaceBallDefinition)((MyCubeBlock)__instance).BlockDefinition).MaxVirtualMass = 0.0f;
        }

        public static void PatchMassMethod(MySpaceBall __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            __instance.VirtualMass = 0.0f;
        }

        static MySpaceBallPatch()
        {
            MethodInfo method1 = typeof(MySpaceBallPatch).GetMethod("PatchMethod", BindingFlags.Static | BindingFlags.Public);
            if ((object)method1 == null)
                throw new Exception("Failed to find patch method");

            MySpaceBallPatch.UpdatePatch = method1;
            MySpaceBallPatch.UpdateMass = typeof(MySpaceBall).GetMethod("RefreshPhysicsBody", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo method2 = typeof(MySpaceBallPatch).GetMethod("PatchMassMethod", BindingFlags.Static | BindingFlags.Public);
            if ((object)method2 == null)
                throw new Exception("Failed to find patch method");

            MySpaceBallPatch.UpdateMassPatch = method2;
        }
    }
}
