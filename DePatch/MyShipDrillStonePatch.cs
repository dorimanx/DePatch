using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Entity;

namespace DePatch
{
	[HarmonyPatch(typeof(MyShipDrill), "OnInventoryComponentAdded")]
	internal class MyShipDrillStonePatch
	{
		// Token: 0x06000094 RID: 148 RVA: 0x00004580 File Offset: 0x00002780
		private static void Prefix(MyShipDrill __instance, MyInventoryBase inventory)
		{
			if (DePatchPlugin.Instance.Config.DrillStoneDumpRightClick && !(bool)MyShipDrillParallelPatch.m_wantsToCollect.GetValue(__instance))
			{
				MyShipDrillStonePatch.items.Clear();
				inventory.CountItems(MyShipDrillStonePatch.items);
				if (MyShipDrillStonePatch.items.ContainsKey(MyShipDrillStonePatch.stoneDefinition))
				{
					inventory.RemoveItemsOfType(MyShipDrillStonePatch.items[MyShipDrillStonePatch.stoneDefinition], MyShipDrillStonePatch.stoneDefinition, MyItemFlags.None, false);
				}
			}
		}

		// Token: 0x04000052 RID: 82
		private static Dictionary<MyDefinitionId, MyFixedPoint> items = new Dictionary<MyDefinitionId, MyFixedPoint>();

		// Token: 0x04000053 RID: 83
		private static MyDefinitionId stoneDefinition = MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone");
	}
}
