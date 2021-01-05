using HarmonyLib;
using Sandbox.Definitions;
using SpaceEngineers.Game.Entities.Blocks;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyVirtualMass), "Init")]
    internal class MyMassBlockPatch
    {
        private static bool Prefix(MyVirtualMass __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return true;

            ((MyVirtualMassDefinition)__instance.BlockDefinition).VirtualMass = 0f;
            return true;
        }
    }
}
