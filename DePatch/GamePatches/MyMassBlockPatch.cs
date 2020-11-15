using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks;

namespace DePatch
{
	[HarmonyPatch(typeof(MyVirtualMass), "Init")]
	internal class MyMassBlockPatch
	{
		private static void Prefix(MyVirtualMass __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
				return;

      		((MyVirtualMassDefinition) ((MyCubeBlock) __instance).BlockDefinition).VirtualMass = 0.0f;
		}
	}
}
