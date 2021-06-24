using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;
using VRage.Game.Entity;

namespace DePatch.ShipTools
{
    //[HarmonyPatch(typeof(MyShipDrill), "OnInventoryComponentAdded")]
    //[PatchShim] this is not working and not needed! function is protected override void OnInventoryComponentAdded(MyInventoryBase inventory)

    internal static class MyShipDrillStonePatch
    {
        private static Dictionary<MyDefinitionId, MyFixedPoint> items = new Dictionary<MyDefinitionId, MyFixedPoint>();

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyShipDrill).GetMethod("OnInventoryComponentAdded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(MyShipDrillStonePatch).GetMethod(nameof(OnInventoryComponentAdded), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static bool OnInventoryComponentAdded(MyShipDrill __instance, MyInventoryBase inventory)
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
