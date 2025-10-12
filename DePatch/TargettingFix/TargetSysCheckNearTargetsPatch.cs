using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;

namespace DePatch.TargettingFix
{
    [PatchShim]
    public static class TargetSysCheckNearTargetsPatch
    {
        // This code was been provided by @fin_mk Discord, great graditude for sharing the fix.

        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyLargeTurretTargetingSystem),
                                                        "CheckNearTargets",
                                                        typeof(TargetSysCheckNearTargetsPatch),
                                                        nameof(CheckNearTargets));

        static void CheckNearTargets(MyLargeTurretTargetingSystem __instance, ref MyEntity suggestedTarget, ref bool suggestedTargetIsOnlyPotential)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.IsTargetSystemPatchEnabled)
                return;

            /* 
             * неизвестно почему турель оказывается в состоянии когда у неё нет цели и
             * сброшен флаг разрешающий чекать другие цели CheckOtherTargets. 
             * из-за этого метод CheckNearTargets хоть и вызвается постоянно, но ничего 
             * не делает. и находиться в таком состоянии турель может сколь угодно долго,
             * пока не будет установлен флаг CheckOtherTargets. это происходит например при 
             * выстреле через терминал (выстрелить один раз). 
             * для исправления детектируется указанная ситуация и CheckOtherTargets 
             * устанавливается в true, что позволяет методу нормально отработать
             */

            var targetSys = __instance;
            if (targetSys != null && targetSys.Target == null && targetSys.CheckOtherTargets == false)
            {
                targetSys.CheckOtherTargets = true;
                //Log.Warn($"CheckNearTargets, turret: {targetSys.TerminalControlReciever}");
            }
        }
    }
}
