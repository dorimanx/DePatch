using System;
using System.Reflection;
using NLog.Fluent;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;

namespace DePatch.TargettingFix
{
    [PatchShim]
    public static class TurretBaseRotateModelsPatch
    {
        // This code was been provided by @fin_mk Discord, great graditude for sharing the fix.

        static MethodInfo RotateModels;

        public static void Patch(PatchContext ctx)
        {
            ctx.Suffix(typeof(MyLargeTurretBase), "UpdateAfterSimulation", typeof(TurretBaseRotateModelsPatch), nameof(UpdateAfterSimulation));

            RotateModels = typeof(MyLargeTurretBase).EasyMethod("RotateModels");
        }

        static void UpdateAfterSimulation(MyLargeTurretBase __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.IsTargetSystemPatchEnabled)
                return;

            MyLargeTurretBase turret = __instance;
            if (turret == null)
                return;

            try
            {
                RotateModels?.Invoke(turret, new object[] { });
            }
            catch (Exception ex)
            {
                Log.Error($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
