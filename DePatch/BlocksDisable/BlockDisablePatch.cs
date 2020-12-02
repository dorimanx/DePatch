using System.Reflection;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using System;

namespace DePatch
{
    [PatchShim]
    public static class BlockUpdatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyFunctionalBlock).GetMethod("UpdateBeforeSimulation100", BindingFlags.Instance | BindingFlags.Public)).Prefixes.Add(typeof(BlockUpdatePatch).GetMethod("CheckBlock", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));

        private static void CheckBlock(MyFunctionalBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled && DePatchPlugin.Instance.Config.EnableBlockDisabler)
            {
                if (__instance != null && (string.Compare("ShipWelder", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                }
                else
                {
                    if (__instance != null && PlayersUtility.KeepBlockOff(__instance))
                    {
                        if (__instance.Enabled)
                        {
                            __instance.Enabled = false;
                        }
                    }
                }
            }
        }
    }
}
