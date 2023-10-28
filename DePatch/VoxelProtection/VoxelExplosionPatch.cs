using Sandbox.Game;
using Torch.Managers.PatchManager;

namespace DePatch.VoxelProtection
{
    [PatchShim]
    public static class VoxelExplosionPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyExplosionInfo), "get_AffectVoxels", typeof(VoxelExplosionPatch), nameof(AffectVoxelsPatch));
        }

        private static bool AffectVoxelsPatch(MyExplosionInfo __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.StopExplosion)
                return true;

            __result = !DePatchPlugin.Instance.Config.StopExplosion && (__instance.ExplosionFlags == MyExplosionFlags.AFFECT_VOXELS ||
                                                                        __instance.ExplosionFlags == MyExplosionFlags.APPLY_DEFORMATION ||
                                                                        __instance.ExplosionFlags == MyExplosionFlags.APPLY_FORCE_AND_DAMAGE);
            return false;
        }
    }
}
