using System.Reflection;
using Sandbox.Game;
using Torch.Managers.PatchManager;

namespace DePatch
{
    [PatchShim]
    public static class VoxelExplosionPatch
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo p = typeof(MyExplosionInfo).GetMethod("get_AffectVoxels", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo q = typeof(VoxelExplosionPatch).GetMethod("PatchGetter");
            ctx.GetPattern(p).Prefixes.Add(q);
        }

        public static bool PatchGetter(MyExplosionInfo __instance, ref bool __result)
        {
            __result = (!DePatchPlugin.Instance.Config.StopExplosion && (__instance.ExplosionFlags & MyExplosionFlags.AFFECT_VOXELS) == MyExplosionFlags.AFFECT_VOXELS);
            return false;
        }
    }
}
