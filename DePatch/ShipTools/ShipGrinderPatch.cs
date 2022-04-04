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
using Sandbox.Game.Weapons.Guns.Barrels;
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
        internal readonly static MethodInfo SetBuildingMusic = typeof(MyShipToolBase).EasyMethod("SetBuildingMusic");

        private static readonly List<MyPhysicalInventoryItem> m_tmpItemList = new List<MyPhysicalInventoryItem>();

        public static void Patch(PatchContext ctx)
        {
            m_wantsToShake = typeof(MyShipGrinder).EasyField("m_wantsToShake");
            m_otherGrid = typeof(MyShipGrinder).EasyField("m_otherGrid");

            ctx.Prefix(typeof(MyShipGrinder), typeof(ShipGrinderPatch), nameof(Activate));
        }

        public static bool Activate(MyShipGrinder __instance, HashSet<MySlimBlock> targets, ref bool __result)
        {
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

            int count = targets.Count;
            m_otherGrid.SetValue(__instance, null);

            if (targets.Count > 0)
                m_otherGrid.SetValue(__instance, targets.FirstElement().CubeGrid);

            float num = 0.25f / Math.Min(4, targets.Count);
            float amount = MySession.Static.GrinderSpeedMultiplier * 4f * num;

            if (4f * num < shipTool.Speed)
                amount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;

            foreach (MySlimBlock target in targets)
            {
                if (!target.CubeGrid.Immune)
                {
                    m_otherGrid.SetValue(__instance, target.CubeGrid);

                    var OtherCubeGrid = (MyCubeGrid)m_otherGrid.GetValue(__instance);

                    if ((OtherCubeGrid.Physics == null ? 1 : (!OtherCubeGrid.Physics.Enabled ? 1 : 0)) != 0)
                        count--;
                    else
                    {
                        MyCubeBlockDefinition.PreloadConstructionModels(target.BlockDefinition);
                        if (Sync.IsServer)
                        {
                            MyDamageInformation info = new MyDamageInformation(false, amount, MyDamageType.Grind, __instance.EntityId);
                            if (target.UseDamageSystem)
                                MyDamageSystem.Static.RaiseBeforeDamageApplied(target, ref info);

                            if (target.CubeGrid.Editable)
                            {
                                target.DecreaseMountLevel(info.Amount, MyEntityExtensions.GetInventory(__instance), identityId: __instance.OwnerId);
                                target.MoveItemsFromConstructionStockpile(MyEntityExtensions.GetInventory(__instance));
                            }

                            if (target.UseDamageSystem)
                                MyDamageSystem.Static.RaiseAfterDamageApplied(target, info);

                            if (target.IsFullyDismounted)
                            {
                                if (target.FatBlock != null && target.FatBlock.HasInventory)
                                    EmptyBlockInventories(target.FatBlock, __instance);

                                if (target.UseDamageSystem)
                                    MyDamageSystem.Static.RaiseDestroyed(target, info);

                                target.SpawnConstructionStockpile();
                                target.CubeGrid.RazeBlock(target.Min, 0UL);
                            }
                        }
                        if (count > 0)
                            SetBuildingMusic.Invoke(__instance, new object[] { 200 });
                    }
                }
            }
            m_wantsToShake.SetValue(__instance, (uint)count > 0U);

            __result = (uint)count > 0U;

            return false;
        }

        private static void EmptyBlockInventories(MyCubeBlock block, MyShipGrinder Grinder)
        {
            for (int index = 0; index < block.InventoryCount; index++)
            {
                MyInventory inventory = MyEntityExtensions.GetInventory(block, index);
                if (!inventory.Empty())
                {
                    m_tmpItemList.Clear();
                    m_tmpItemList.AddRange(inventory.GetItems());
                    foreach (MyPhysicalInventoryItem tmpItem in m_tmpItemList)
                    {
                        MyInventory.Transfer(inventory, MyEntityExtensions.GetInventory(Grinder), tmpItem.ItemId);
                    }
                }
            }
        }
    }
}