using System.Collections.Generic;
using HarmonyLib;
using Sandbox.Game.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Entity;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipDrill), "OnInventoryComponentAdded")]
    internal class MyShipDrillStonePatch
    {
        private static Dictionary<MyDefinitionId, MyFixedPoint> items = new Dictionary<MyDefinitionId, MyFixedPoint>();

        private static bool Prefix(MyShipDrill __instance, MyInventoryBase inventory)
        {
            if (!DePatchPlugin.Instance.Config.DrillStoneDumpRightClick || (bool)MyShipDrillParallelPatch.m_wantsToCollect.GetValue(__instance))
                return true;

            items.Clear();
            inventory.CountItems(items);
            if (!items.ContainsKey(MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone")))
                return true;

            inventory.RemoveItemsOfType(items[MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone")], MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone"));

            return true;
        }
    }
}
