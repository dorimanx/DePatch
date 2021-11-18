using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]

    public static class MyPistonShareInertiaTensor
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Suffix(typeof(MyPistonBase), typeof(MyPistonShareInertiaTensor), "UpdateBeforeSimulation10");
        }

        public static void UpdateBeforeSimulation10(MyPistonBase __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PistonInertiaTensor)
                return;

            if (__instance is IMyPistonBase piston)
            {
                if (piston == null || !DePatchPlugin.GameIsReady || piston.Closed || !piston.IsFunctional)
                    return;

                try
                {
                    var tensor = piston.GetValueBool("ShareInertiaTensor");
                    if (tensor == false)
                        piston.SetValueBool("ShareInertiaTensor", true);
                }
                catch
                {
                    // just ignore.
                }
            }
        }
    }
}