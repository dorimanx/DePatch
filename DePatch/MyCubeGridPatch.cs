using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace DePatch
{
	[HarmonyPatch(typeof(MyCubeGrid), "UpdateAfterSimulation100")]
	internal class MyCubeGridPatch
	{
		private static void Prefix(MyCubeGrid __instance)
		{
			if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
			{
				return;
			}
			try
			{
				if (!PVEGrid.Grids.ContainsKey(__instance))
				{
					PVEGrid.Grids.Add(__instance, new PVEGrid(__instance));
				}
				if (__instance == null)
				{
					PVEGrid.Grids.Remove(__instance);
				}
				else
				{
					PVEGrid pvegrid = PVEGrid.Grids[__instance];
					PVEGrid pvegrid2 = pvegrid;
					int num = pvegrid2.Tick + 1;
					pvegrid2.Tick = num;
					if (num > 10)
					{
						pvegrid.Tick = 0;
						bool flag = pvegrid.InPVEZone();
						bool flag2 = PVE.EntitiesInZone.Contains(__instance.EntityId);
						if (!flag || !flag2)
						{
							if (!flag && flag2)
							{
								pvegrid.OnGridLeft();
								PVE.EntitiesInZone.Remove(__instance.EntityId);
							}
							if (flag && !flag2)
							{
								pvegrid.OnGridEntered();
								PVE.EntitiesInZone.Add(__instance.EntityId);
							}
						}
					}
				}
			}
			catch
			{
			}
		}
	}
}
