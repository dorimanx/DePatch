using System.Collections.Generic;
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
            if (!DePatchPlugin.Instance.Config.DrillStoneDumpRightClick || (bool)MyShipDrillParallelPatch.m_wantsToCollect.GetValue(__instance))
                return;

            items.Clear();
            inventory.CountItems(items);
            if (!items.ContainsKey(stoneDefinition))
                return;

            inventory.RemoveItemsOfType(items[stoneDefinition], stoneDefinition, MyItemFlags.None, false);
        }
    }
}
