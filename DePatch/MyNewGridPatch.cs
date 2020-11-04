using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
	[HarmonyPatch(typeof(MyCubeGrid), "Init")]
	internal class MyNewGridPatch
	{
		private static void Postfix(MyCubeGrid __instance)
		{
			if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
			{
				return;
			}
			if (!PVEGrid.Grids.ContainsKey(__instance))
			{
				PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
			}
			PVEGrid pvegrid = PVEGrid.Grids[__instance];
			if (pvegrid.InPVEZone())
			{
				pvegrid.OnGridEntered();
				PVE.EntitiesInZone.Add(__instance.EntityId);
			}
		}
	}
}
