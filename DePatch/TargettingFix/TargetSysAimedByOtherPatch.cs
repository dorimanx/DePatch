using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;

namespace DePatch.TargettingFix
{
    [PatchShim]
    public static class TargetSysAimedByOtherPatch
    {
        // This code was been provided by @fin_mk Discord, great graditude for sharing the fix.

        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyLargeTurretTargetingSystem),
                                                                "IsTargetAimedByOtherTurret",
                                                                typeof(TargetSysAimedByOtherPatch),
                                                                nameof(IsTargetAimedByOtherTurret));

        static bool IsTargetAimedByOtherTurret(ref bool __result, MyEntity target)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.IsTargetSystemPatchEnabled)
                return true;

            __result = false;
            return false;
        }
    }
}
