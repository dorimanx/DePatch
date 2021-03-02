using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyPistonBase), "UpdateBeforeSimulation10")]
    public static class MyPistonShareInertiaTensor
    {
        public static void Prefix(MyPistonBase __instance)
        {
            if (__instance is IMyPistonBase piston && DePatchPlugin.Instance.Config.PistonInertiaTensor)
            {
                var tensor = piston.GetValueBool("ShareInertiaTensor");
                if (!tensor)
                {
                    piston.SetValueBool("ShareInertiaTensor", true);
                }
            }
        }
    }
}