using System;
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
        private static FieldInfo m_wantsToShake;
        private static FieldInfo m_otherGrid;
        internal readonly static MethodInfo SetBuildingMusic = typeof(MyShipToolBase).easyMethod("SetBuildingMusic");

        private static readonly List<MyPhysicalInventoryItem> m_tmpItemList = new List<MyPhysicalInventoryItem>();

        public static void Patch(PatchContext ctx)
        {
            m_wantsToShake = typeof(MyShipGrinder).easyField("m_wantsToShake");
            m_otherGrid = typeof(MyShipGrinder).easyField("m_otherGrid");

            ctx.Prefix(typeof(MyShipGrinder), typeof(ShipGrinderPatch), nameof(Activate));
        }

        public static bool Activate(MyShipGrinder __instance, ref bool __result, HashSet<MySlimBlock> targets)
        {
            __result = false;

            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null || __instance.MarkedForClose || __instance.Closed)
                return true;

            if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.CheckEntityInZone(__instance.CubeGrid))
                _ = targets.RemoveWhere(b => !__instance.GetUserRelationToOwner(b.BuiltBy).IsFriendly());

            if (!DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return true;

            var enumerable = ShipTool.shipTools.Where(t => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            var shipTools = enumerable.ToList();

            if (!shipTools.Any())
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                {
                    ShipTool.shipTools.Add(new ShipTool
                    {
                        Speed = ShipTool.DEFAULT_SPEED_G,
                        Subtype = __instance.DefinitionId.SubtypeId.String,
                        Type = ToolType.Grinder
                    });
                });
                return true;
            }

            var shipTool = shipTools.FirstOrDefault();
            if (shipTool == null)
                return true;

            int num = targets.Count;
            m_otherGrid.SetValue(__instance, null);

            if (targets.Count > 0)
                m_otherGrid.SetValue(__instance, targets.FirstElement().CubeGrid);

            float num2 = 0.25f / Math.Min(4, targets.Count);
            float amount = MySession.Static.GrinderSpeedMultiplier * 4f * num2;

            if (4f * num2 < shipTool.Speed)
                amount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;

            foreach (MySlimBlock mySlimBlock in targets)
            {
                if (!mySlimBlock.CubeGrid.Immune)
                {
                    m_otherGrid.SetValue(__instance, mySlimBlock.CubeGrid);

                    var OtherCubeGrid = (MyCubeGrid)m_otherGrid.GetValue(__instance);

                    if (OtherCubeGrid == null || OtherCubeGrid.Physics == null || !OtherCubeGrid.Physics.Enabled)
                    {
                        num--;
                    }
                    else
                    {
                        MyCubeBlockDefinition.PreloadConstructionModels(mySlimBlock.BlockDefinition);
                        if (Sync.IsServer)
                        {
                            MyDamageInformation myDamageInformation = new MyDamageInformation(false, amount, MyDamageType.Grind, __instance.EntityId);
                            if (mySlimBlock.UseDamageSystem)
                                MyDamageSystem.Static.RaiseBeforeDamageApplied(mySlimBlock, ref myDamageInformation);

                            if (mySlimBlock.CubeGrid.Editable)
                            {
                                mySlimBlock.DecreaseMountLevel(myDamageInformation.Amount, __instance.GetInventory(0), false, __instance.OwnerId, false);
                                mySlimBlock.MoveItemsFromConstructionStockpile(__instance.GetInventory(0), MyItemFlags.None);
                            }
                            if (mySlimBlock.UseDamageSystem)
                                MyDamageSystem.Static.RaiseAfterDamageApplied(mySlimBlock, myDamageInformation);

                            if (mySlimBlock.IsFullyDismounted)
                            {
                                if (mySlimBlock.FatBlock != null && mySlimBlock.FatBlock.HasInventory)
                                    EmptyBlockInventories(mySlimBlock.FatBlock, __instance);

                                if (mySlimBlock.UseDamageSystem)
                                    MyDamageSystem.Static.RaiseDestroyed(mySlimBlock, myDamageInformation);

                                mySlimBlock.SpawnConstructionStockpile();
                                mySlimBlock.CubeGrid.RazeBlock(mySlimBlock.Min, 0UL);
                            }
                        }
                        if (num > 0)
                            SetBuildingMusic.Invoke(__instance, new object[] { 200 });
                    }
                }
            }
            m_wantsToShake.SetValue(__instance, (num != 0));

            __result = num != 0;

            return false;
        }

        private static void EmptyBlockInventories(MyCubeBlock block, MyShipGrinder Grinder)
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
                        MyInventory.Transfer(inventory, Grinder.GetInventory(0), myPhysicalInventoryItem.ItemId, -1, null, false);
                    }
                }
            }
        }
    }
}