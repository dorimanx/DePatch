using System;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyBatteryBlock))]
    [HarmonyPatch("ChargeMode", MethodType.Setter)]
    internal class KEENMyBatteryFix
    {
        private static bool Prefix(MyBatteryBlock __instance, ref ChargeMode value)
        {
            if (Enum.IsDefined(typeof(ChargeMode), value))
                return true;

            value = ChargeMode.Auto;
            return true;
        }
    }
}
