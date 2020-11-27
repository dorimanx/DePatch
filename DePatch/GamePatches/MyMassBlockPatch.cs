using HarmonyLib;
using Sandbox.Definitions;
using SpaceEngineers.Game.Entities.Blocks;

namespace DePatch
{
    [HarmonyPatch(typeof(MyVirtualMass), "Init")]
    internal class MyMassBlockPatch
    {
        private static void Prefix(MyVirtualMass __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
                return;

            ((MyVirtualMassDefinition)__instance.BlockDefinition).VirtualMass = 0f;
        }
    }
}
