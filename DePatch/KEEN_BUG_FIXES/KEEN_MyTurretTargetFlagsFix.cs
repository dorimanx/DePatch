using HarmonyLib;
using Sandbox.Game.Weapons;
using System.Reflection;
using VRage.Sync;

namespace DePatch.KEEN_BUG_FIXES
{
    public class KEEN_MyTurretTargetFlagsFix
    {
        // Code by Buddhist#3825 (Discord)
        // This fixing the "Keen: Validation of sync value ID 8 failed"
        // when switching targets in turrets interface.

        private static readonly Harmony _harmony = new Harmony("Dori.KEEN_MyTurretTargetFlagsFix");

        private static readonly MethodInfo original = typeof(Sync<MyTurretTargetFlags, SyncDirection.BothWays>)
            .GetMethod("IsValid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        private static readonly MethodInfo prefix = typeof(KEEN_MyTurretTargetFlagsFix)
            .GetMethod(nameof(MethodIsValid), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);

        public static void PatchMyTurretTarget()
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.TurretsAimFix)
                return;

            _ = _harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static bool MethodIsValid(MyTurretTargetFlags value, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.TurretsAimFix)
                return true;

            __result = true;
            return false;
        }
    }
}