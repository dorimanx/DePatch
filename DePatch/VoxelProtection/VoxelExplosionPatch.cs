using Sandbox.Game;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.VoxelProtection
{
    [PatchShim]
    public static class VoxelExplosionPatch
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MyExplosionInfo).GetMethod("get_AffectVoxels", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(VoxelExplosionPatch).GetMethod("AffectVoxelsPatch", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool AffectVoxelsPatch(MyExplosionInfo __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.StopExplosion)
                return true;

            __result = (!DePatchPlugin.Instance.Config.StopExplosion && (__instance.ExplosionFlags & MyExplosionFlags.AFFECT_VOXELS) == MyExplosionFlags.AFFECT_VOXELS);
            return false;
        }
    }
}
