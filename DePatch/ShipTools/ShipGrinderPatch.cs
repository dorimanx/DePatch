using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch.ShipTools
{
    [HarmonyPatch(typeof(MyShipGrinder), "Activate")]
    internal class ShipGrinderPatch
    {
        private static readonly FieldInfo m_detectorSphere = typeof(MyShipToolBase).GetField("m_detectorSphere", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly HashSet<MySlimBlock> SlimBlocks = new HashSet<MySlimBlock>();

        private static bool Prefix(MyShipGrinder __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled && !DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return true;

            IEnumerable<ShipTool> enumerable = ShipTool.shipTools.Where((ShipTool t) => t.Subtype == __instance.DefinitionId.SubtypeId.String);
            if (enumerable.Count() == 0)
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
                enumerable.AddItem(new ShipTool
                {
                    Speed = ShipTool.DEFAULT_SPEED,
                    Subtype = __instance.DefinitionId.SubtypeId.String,
                    Type = ToolType.Grinder
                });
            }
            var list = new List<MyEntity>();
            var boundingSphere = (BoundingSphere)m_detectorSphere.GetValue(__instance);
            var boundingSphereD = new BoundingSphereD(Vector3D.Transform(boundingSphere.Center, __instance.CubeGrid.WorldMatrix), boundingSphere.Radius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphereD, list);

            if (list.Contains(__instance.CubeGrid))
            {
                list.Remove(__instance.CubeGrid);
            }
            foreach (var myEntity in list)
            {
                if (!(myEntity is MyCubeGrid myCubeGrid) || myEntity.Physics == null) continue;

                var inventoryBase = __instance.GetInventoryBase();
                var grinderAmount = MySession.Static.GrinderSpeedMultiplier * enumerable.First().Speed;
                SlimBlocks.Clear();
                myCubeGrid.GetBlocksInsideSphere(ref boundingSphereD, SlimBlocks, false);

                foreach (var mySlimBlock in SlimBlocks)
                {
                    mySlimBlock.DecreaseMountLevel(grinderAmount, inventoryBase, false, 0L, false);
                }
            }
            return true;
        }
    }
}
