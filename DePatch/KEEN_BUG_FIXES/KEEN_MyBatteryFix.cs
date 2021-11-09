using System;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;

namespace DePatch.KEEN_BUG_FIXES
{
    [HarmonyPatch(typeof(MyBatteryBlock))]
    [HarmonyPatch("ChargeMode", MethodType.Setter)]
    internal class KEEN_MyBatteryFix
    {
        private static bool Prefix(MyBatteryBlock __instance, ref ChargeMode value)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (Enum.IsDefined(typeof(ChargeMode), value))
                return true;

            value = ChargeMode.Auto;
            return true;
        }
    }
}
