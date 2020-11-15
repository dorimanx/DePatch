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
		private static Dictionary<MyDefinitionId, MyFixedPoint> items = new Dictionary<MyDefinitionId, MyFixedPoint>();

		private static MyDefinitionId stoneDefinition = MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone");

		private static void Prefix(MyShipDrill __instance, MyInventoryBase inventory)
		{
      		if (!DePatchPlugin.Instance.Config.DrillStoneDumpRightClick || (bool) MyShipDrillParallelPatch.m_wantsToCollect.GetValue((object) __instance))
        		return;

			MyShipDrillStonePatch.items.Clear();
			inventory.CountItems(MyShipDrillStonePatch.items);
      		if (!MyShipDrillStonePatch.items.ContainsKey(MyShipDrillStonePatch.stoneDefinition))
        		return;

			inventory.RemoveItemsOfType(MyShipDrillStonePatch.items[MyShipDrillStonePatch.stoneDefinition], MyShipDrillStonePatch.stoneDefinition, MyItemFlags.None, false);
		}
	}
}
