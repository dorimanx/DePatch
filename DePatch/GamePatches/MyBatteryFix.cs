using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;

namespace DePatch
{
	[HarmonyPatch(typeof(MyBatteryBlock))]
	[HarmonyPatch("ChargeMode", MethodType.Setter)]
	internal class MyBatteryFix
	{
		private static void Prefix(MyBatteryBlock __instance, ref ChargeMode value)
		{
      		if (Enum.IsDefined(typeof (ChargeMode), (object) value))
        		return;
			value = ChargeMode.Auto;
		}
	}
}
