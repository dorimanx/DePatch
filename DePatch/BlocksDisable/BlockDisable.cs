using Sandbox.Game.Entities.Cube;
using System;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;

namespace DePatch.BlocksDisable
{
    [PatchShim]
    public static class BlockDisable
    {
        private static int Cooldown = 1;

        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyFunctionalBlock), typeof(BlockDisable), nameof(UpdateAfterSimulation100));
        }

        public static void UpdateAfterSimulation100(MyFunctionalBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if (DePatchPlugin.Instance.Config.EnableBlockDisabler && __instance != null && __instance.IsFunctional && __instance.Enabled)
                {
                    if (++Cooldown < 30)
                        return;

                    Cooldown = 1;

                    if (string.Compare("ShipWelder", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        string.Compare("MyProgrammableBlock", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        // weldertypes are off in ShipWelderPatch.cs
                        // ProgramBlocksTypes are off in MyProgramBlockSlow.cs
                    }
                    else
                    {
                        if (!MySession.Static.Players.IsPlayerOnline(__instance.OwnerId) && PlayersUtility.KeepBlockOff(__instance))
                            __instance.Enabled = false;
                    }
                }
            }
        }
    }
}
