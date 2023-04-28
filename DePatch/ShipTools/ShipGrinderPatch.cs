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

            if (targets.Count == 0)
            {
                __result = false;
                return false;
            }

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

            HashSet<MySlimBlock> targetsReduced = new HashSet<MySlimBlock>();

            if (targets.Count > 0)
            {
                // prevent self grinding
                if (targets.Remove(__instance.SlimBlock))
                    count = targets.Count;

                if (targets.Count > 20)
                {
                    int targetsCount = 0;
                    foreach (var NewTraget in targets)
                    {
                        targetsReduced.Add(NewTraget);
                        targetsCount++;

                        if (targetsCount == 20)
                            break;
                    }

                    targets.Clear();
                    targets = targetsReduced;
                    count = targets.Count;
                }

                m_otherGrid.SetValue(__instance, targets.FirstElement().CubeGrid);
            }

            float num = 0.25f / Math.Min(4, targets.Count); // if 1 target then here 0.25
            float amount = MySession.Static.GrinderSpeedMultiplier * 4f * num; // if 1 target then here 3 if GrinderSpeedMultiplier is 3 in game config

            if (amount < shipTool.Speed || targets.Count < 3)
                amount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed * 2;
            else
                return true;

            foreach (MySlimBlock target in targets)
            {
                if (!target.CubeGrid.Immune)
                {
                    m_otherGrid.SetValue(__instance, target.CubeGrid);

                    var OtherCubeGrid = (MyCubeGrid)m_otherGrid.GetValue(__instance);

                    if (OtherCubeGrid?.Physics == null || !OtherCubeGrid.Physics.Enabled)
                        count--;
                    else
                    {
                        MyCubeBlockDefinition.PreloadConstructionModels(target.BlockDefinition);
                        if (Sync.IsServer)
                        {
                            MyDamageInformation info = new MyDamageInformation(false, amount, MyDamageType.Grind, __instance.GetBaseEntity().EntityId);
                            if (target.UseDamageSystem)
                                MyDamageSystem.Static.RaiseBeforeDamageApplied(target, ref info);

                            if (info.Amount <= 0)
                            {
                                count--;
                                continue;
                            }

                            if (target.CubeGrid.Editable)
                            {
                                var CubeBlockOwner = __instance as MyCubeBlock; // base.OwnerId
                                target.DecreaseMountLevel(info.Amount, __instance.GetInventory(0), false, CubeBlockOwner.OwnerId, false);
                                target.MoveItemsFromConstructionStockpile(__instance.GetInventory(0), MyItemFlags.None);
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
                        {
                            var MyshipTooBase = __instance as MyShipToolBase;
                            SetBuildingMusic.Invoke(MyshipTooBase, new object[] { 200 });
                        }
                    }
                }
            }
            m_wantsToShake.SetValue(__instance, count != 0);

            __result = count != 0;

            return false;
        }

        private static void EmptyBlockInventories(MyCubeBlock block, MyShipGrinder Grinder)
        {
            for (int index = 0; index < block.InventoryCount; index++)
            {
                MyInventory inventory = block.GetInventory(index);
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