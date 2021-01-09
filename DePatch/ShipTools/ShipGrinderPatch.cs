using System.Collections.Generic;
using System.Linq;
using DePatch.PVEZONE;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Game.ModAPI;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipGrinder), "Activate")]
    internal class ShipGrinderPatch
    {
        private static void Prefix(MyShipGrinder __instance, HashSet<MySlimBlock> targets)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null) return;

            // if found will return false this why !PVE.CheckEntityInZone
            if (!PVE.CheckEntityInZone(__instance.CubeGrid))
            {
                _ = targets.RemoveWhere(b => !__instance.GetUserRelationToOwner(b.BuiltBy).IsFriendly());
            }

            if (!DePatchPlugin.Instance.Config.ShipToolsEnabled) return;

            var enumerable = ShipTool.shipTools.Where(t => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            var shipTools = enumerable.ToList();
            if (!shipTools.Any())
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                {
                    ShipTool.shipTools.Add(new ShipTool
                    {
                        Speed = ShipTool.DEFAULT_SPEED,
                        Subtype = __instance.DefinitionId.SubtypeId.String,
                        Type = ToolType.Grinder
                    });
                });
                return;
            }

            var shipTool = shipTools.FirstOrDefault();
            if (shipTool == null) return;
            var grinderAmount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;
            foreach (var mySlimBlock in targets)
            {
                if (!mySlimBlock.CubeGrid.Immune)
                {
                    MyDamageInformation myDamageInformation = new MyDamageInformation(false, grinderAmount, MyDamageType.Grind, __instance.EntityId);
                    if (mySlimBlock.UseDamageSystem)
                    {
                        MyDamageSystem.Static.RaiseBeforeDamageApplied(mySlimBlock, ref myDamageInformation);
                    }

                    if (mySlimBlock.CubeGrid.Editable && myDamageInformation.Amount != 0)
                    {
                        mySlimBlock.DecreaseMountLevel(grinderAmount, __instance.GetInventoryBase(), false, __instance.OwnerId, false);
                        mySlimBlock.MoveItemsFromConstructionStockpile(__instance.GetInventory(0), MyItemFlags.None);
                    }
                    if (mySlimBlock.UseDamageSystem && myDamageInformation.Amount != 0)
                    {
                        MyDamageSystem.Static.RaiseAfterDamageApplied(mySlimBlock, myDamageInformation);
                    }
                }
            }
        }
    }
}
