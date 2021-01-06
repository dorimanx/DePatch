using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DePatch.PVEZONE;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipGrinder), "Activate")]
    internal class ShipGrinderPatch
    {
        private static void Prefix(MyShipGrinder __instance, HashSet<MySlimBlock> targets)
        {
            if (!DePatchPlugin.Instance.Config.Enabled && !DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return;

            if (PVE.CheckEntityInZone(__instance.CubeGrid))
                targets.RemoveWhere(b => !__instance.GetUserRelationToOwner(b.BuiltBy).IsFriendly());
            
            var enumerable = ShipTool.shipTools.Where(t => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            if (!enumerable.Any())
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

            var shipTool = enumerable.FirstOrDefault();
            if (shipTool == null) return;
            var grinderAmount = MySession.Static.GrinderSpeedMultiplier * shipTool.Speed;
            foreach (var mySlimBlock in targets)
            {
                mySlimBlock.DecreaseMountLevel(grinderAmount, __instance.GetInventoryBase());
            }
        }
    }
}
