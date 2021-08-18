using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DePatch.PVEZONE;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace DePatch.ShipTools
{
    [PatchShim]

    internal static class ShipGrinderPatch
    {
        private static List<MyPhysicalInventoryItem> m_tmpItemList = new List<MyPhysicalInventoryItem>();
        private static MyCubeGrid m_otherGrid;

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyShipGrinder).GetMethod("Activate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(ShipGrinderPatch).GetMethod(nameof(Activate), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static void EmptyBlockInventories(MyCubeBlock block)
        {
            for (int i = 0; i < block.InventoryCount; i++)
            {
                MyInventory inventory = block.GetInventory(i);
                if (!inventory.Empty())
                {
                    m_tmpItemList.Clear();
                    m_tmpItemList.AddRange(inventory.GetItems());
                    foreach (MyPhysicalInventoryItem myPhysicalInventoryItem in m_tmpItemList)
                    {
                        MyInventory.Transfer(inventory, block.GetInventory(0), myPhysicalInventoryItem.ItemId, -1, null, false);
                    }
                }
            }
        }

        private static void Activate(MyShipGrinder __instance, HashSet<MySlimBlock> targets)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null) return;

            if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.CheckEntityInZone(__instance.CubeGrid))
                _ = targets.RemoveWhere(b => !__instance.GetUserRelationToOwner(b.BuiltBy).IsFriendly());

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

            m_otherGrid = null;
            if (targets.Count > 0)
            {
                m_otherGrid = targets.FirstElement().CubeGrid;
            }

            foreach (var mySlimBlock in targets)
            {
                if (!mySlimBlock.CubeGrid.Immune)
                {
                    m_otherGrid = mySlimBlock.CubeGrid;
                    if (m_otherGrid.Physics == null || !m_otherGrid.Physics.Enabled)
                    {
                    }
                    else
                    {
                        MyCubeBlockDefinition.PreloadConstructionModels(mySlimBlock.BlockDefinition);
                        if (Sync.IsServer)
                        {
                            MyDamageInformation myDamageInformation = new MyDamageInformation(false, grinderAmount, MyDamageType.Grind, __instance.EntityId);
                            if (mySlimBlock.UseDamageSystem)
                            {
                                MyDamageSystem.Static.RaiseBeforeDamageApplied(mySlimBlock, ref myDamageInformation);
                            }
                            if (mySlimBlock.CubeGrid.Editable && myDamageInformation.Amount != 0)
                            {
                                mySlimBlock.DecreaseMountLevel(grinderAmount, __instance.GetInventory(0), false, __instance.OwnerId, false);
                                mySlimBlock.MoveItemsFromConstructionStockpile(__instance.GetInventory(0), MyItemFlags.None);
                            }
                            if (mySlimBlock.UseDamageSystem && myDamageInformation.Amount != 0)
                            {
                                MyDamageSystem.Static.RaiseAfterDamageApplied(mySlimBlock, myDamageInformation);
                            }
                            if (mySlimBlock.IsFullyDismounted)
                            {
                                if (mySlimBlock.FatBlock != null && mySlimBlock.FatBlock.HasInventory)
                                {
                                    EmptyBlockInventories(mySlimBlock.FatBlock);
                                }
                                if (mySlimBlock.UseDamageSystem)
                                {
                                    MyDamageSystem.Static.RaiseDestroyed(mySlimBlock, myDamageInformation);
                                }
                                mySlimBlock.SpawnConstructionStockpile();
                                mySlimBlock.CubeGrid.RazeBlock(mySlimBlock.Min, 0UL);
                            }
                        }
                    }
                }
            }
        }
    }
}
