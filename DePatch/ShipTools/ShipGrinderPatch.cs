using System;
using System.Collections.Generic;
using System.Linq;
using DePatch.PVEZONE;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Game.ModAPI;

namespace DePatch.ShipTools
{
    internal static class ShipGrinderPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyDamageSystem), typeof(ShipGrinderPatch), nameof(RaiseBeforeDamageApplied));
            ctx.Prefix(typeof(MyShipGrinder), typeof(ShipGrinderPatch), nameof(Activate));
            ctx.Suffix(typeof(MyShipGrinder), "Activate", typeof(ShipGrinderPatch), nameof(ActivateAfter));
            ctx.Prefix(typeof(MySlimBlock), typeof(ShipGrinderPatch), nameof(SpawnConstructionStockpile));
        }

        static Dictionary<long, HashSet<MySlimBlock>> TargetsInternal = new Dictionary<long, HashSet<MySlimBlock>>();

        public static bool RaiseBeforeDamageApplied(MyDamageSystem __instance, object target, ref MyDamageInformation info)
        { // keep it BOOL here.
            if (!DePatchPlugin.Instance.Config.Enabled || __instance == null)
                return true;

            if (!DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return true;

            if (info.Type != MyDamageType.Grind)
                return true;

            _ = MyEntities.TryGetEntityById(info.AttackerId, out var AttackerEntity, allowClosed: false);

            if (AttackerEntity is MyShipGrinder Grinder)
            {
                var enumerable = ShipTool.shipTools.Where(t => t.Subtype == Grinder.DefinitionId.SubtypeId.String);
                var shipTools = enumerable.ToList();

                if (!shipTools.Any())
                {
                    DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                    {
                        ShipTool.shipTools.Add(new ShipTool
                        {
                            Speed = ShipTool.DEFAULT_SPEED_G,
                            Subtype = Grinder.DefinitionId.SubtypeId.String,
                            Type = ToolType.Grinder
                        });
                    });
                    return true;
                }

                var shipTool = shipTools.FirstOrDefault();
                if (shipTool == null)
                    return true;

                float num = 0.25f / Math.Min(4, 1); // if 1 target then here 0.25 
                float DefaultAmount = MySession.Static.GrinderSpeedMultiplier * 4f * num; // if 1 target then here 3 if GrinderSpeedMultiplier is 3 in game confi
                float RequestedDamageAmount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;

                if (info.Amount < shipTool.Speed || DefaultAmount < RequestedDamageAmount)
                    info.Amount = RequestedDamageAmount;
            }

            return true;
        }

        public static bool Activate(MyShipGrinder __instance, ref HashSet<MySlimBlock> targets, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            __result = false;

            if (__instance == null || __instance.MarkedForClose || __instance.Closed)
                return false;

            if (targets.Count == 0)
                return false;

            _ = targets.RemoveWhere(b => b == null || (b.FatBlock != null && (b.FatBlock.Closed || b.FatBlock.MarkedForClose)));

            if (DePatchPlugin.Instance.Config.ShipToolsEnabled && DePatchPlugin.Instance.Config.NoStockPileSpawnFromGrinders)
            {
                if (TargetsInternal.ContainsKey(__instance.EntityId))
                    TargetsInternal.Remove(__instance.EntityId);

                TargetsInternal.Add(__instance.EntityId, targets);
            }

            if (DePatchPlugin.Instance.Config.PveZoneEnabled && PVE.CheckEntityInZone(__instance.CubeGrid))
            {
                _ = targets.RemoveWhere(b => !__instance.GetUserRelationToOwner(b.OwnerId).IsFriendly());

                if (targets.Count == 0)
                    return false;

                if (DePatchPlugin.Instance.Config.ShipToolsEnabled && DePatchPlugin.Instance.Config.NoStockPileSpawnFromGrinders)
                {
                    if (TargetsInternal.ContainsKey(__instance.EntityId))
                        TargetsInternal.Remove(__instance.EntityId);

                    TargetsInternal.Add(__instance.EntityId, targets);
                }
            }

            __result = true;
            return true;
        }

        public static void ActivateAfter(MyShipGrinder __instance, HashSet<MySlimBlock> targets, bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ShipToolsEnabled || __instance == null)
                return;

            if (DePatchPlugin.Instance.Config.NoStockPileSpawnFromGrinders)
            {
                if (TargetsInternal.ContainsKey(__instance.EntityId))
                    TargetsInternal.Remove(__instance.EntityId);
            }
        }

        public static bool SpawnConstructionStockpile(MySlimBlock __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ShipToolsEnabled || __instance == null)
                return true;

            if (DePatchPlugin.Instance.Config.NoStockPileSpawnFromGrinders)
            {
                if (TargetsInternal.Count > 0)
                {
                    foreach (var target in TargetsInternal)
                    {
                        if (target.Value.Contains(__instance))
                            return false;
                    }
                }
            }
            return true;
        }
    }
}