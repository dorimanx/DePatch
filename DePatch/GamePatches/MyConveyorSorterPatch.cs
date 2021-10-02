using System;
using System.Reflection;
using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyConveyorSorterPatch
    {
        internal static readonly MethodInfo ConveyorSorterUpdateAfterSimulation10 = typeof(MyConveyorSorter).GetMethod("UpdateAfterSimulation10", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo UpdatePatch = typeof(MyConveyorSorterPatch).GetMethod(nameof(PatchMethod), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");
        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(ConveyorSorterUpdateAfterSimulation10).Prefixes.Add(UpdatePatch);
        }

        private static bool PatchMethod(MyConveyorSorter __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (__instance != null && __instance.OwnerId == 0L && __instance.DrainAll)
                return false;

            return true;
        }
    }
}
