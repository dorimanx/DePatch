using HarmonyLib;
using Sandbox.Game.Entities;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    //[HarmonyPatch(typeof(MyCameraBlock), "Init")]
    [PatchShim]

    internal static class MyCameraBlockPatch
    {
        private static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyCameraBlock).GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[2]
            {
                typeof(MyObjectBuilder_CubeBlock),
                typeof(MyCubeGrid)
            }, new ParameterModifier[0])).
            Prefixes.Add(typeof(MyCameraBlockPatch).GetMethod(nameof(CameraInit), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

        private static void CameraInit(MyCameraBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            __instance.BlockDefinition.RaycastDistanceLimit = DePatchPlugin.Instance.Config.RaycastLimit;
        }
    }
}
