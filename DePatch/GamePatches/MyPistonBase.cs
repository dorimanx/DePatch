using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    //[HarmonyPatch(typeof(MyPistonBase), "UpdateBeforeSimulation10")]
    [PatchShim]

    public static class MyPistonShareInertiaTensor
    {
        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyPistonBase).GetMethod("UpdateBeforeSimulation10", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Suffixes.Add(typeof(MyPistonShareInertiaTensor).GetMethod(nameof(UpdateBeforeSimulation10), BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        }

        public static void UpdateBeforeSimulation10(MyPistonBase __instance)
        {
            if (__instance is IMyPistonBase piston && DePatchPlugin.Instance.Config.PistonInertiaTensor)
            {
                var tensor = piston.GetValueBool("ShareInertiaTensor");
                if (!tensor)
                    piston.SetValueBool("ShareInertiaTensor", true);
            }
        }
    }
}