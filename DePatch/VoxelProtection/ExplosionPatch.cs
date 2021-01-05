using HarmonyLib;
using Sandbox.Game;

namespace DePatch.VoxelProtection
{
    [HarmonyPatch(typeof(MyExplosionInfo), "AffectVoxels", MethodType.Getter)]
    internal class VoxelExplosionPatch
    {
        private static bool Prefix(MyExplosionInfo __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.StopExplosion)
                return true;

            __result = (__instance.ExplosionFlags & MyExplosionFlags.AFFECT_VOXELS) == MyExplosionFlags.AFFECT_VOXELS;
            return false;
        }
    }
}
