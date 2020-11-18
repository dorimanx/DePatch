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

namespace DePatch
{
    [HarmonyPatch(typeof(MyShipGrinder), "Activate")]
    internal class ShipGrinderPatch
    {
        private static readonly FieldInfo m_detectorSphere = typeof(MyShipToolBase).GetField(nameof(m_detectorSphere), BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly HashSet<MySlimBlock> slimBlocks = new HashSet<MySlimBlock>();

        private static void Prefix(MyShipGrinder __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled && !DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return;

            IEnumerable<ShipTool> enumerable = from t in ShipTool.shipTools
                                               where t.Subtype == __instance.DefinitionId.SubtypeId.String
                                               select t;
            if (enumerable.Count() == 0)
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate ()
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
            List<MyEntity> list = new List<MyEntity>();
            BoundingSphere boundingSphere = (BoundingSphere)m_detectorSphere.GetValue(__instance);
            BoundingSphereD boundingSphereD = new BoundingSphereD(Vector3D.Transform(boundingSphere.Center, __instance.CubeGrid.WorldMatrix), boundingSphere.Radius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphereD, list, MyEntityQueryType.Both);
            if (list.Contains(__instance.CubeGrid))
            {
                list.Remove(__instance.CubeGrid);
            }

            foreach (var (myCubeGrid, inventoryBase, grinderAmount) in from MyEntity myEntity in list
                                          let myCubeGrid = myEntity as MyCubeGrid
                                          where myCubeGrid  != null && myEntity.Physics != null
                                          let inventoryBase = __instance.GetInventoryBase()
                                          let grinderAmount = MySession.Static.GrinderSpeedMultiplier * enumerable.First().Speed
                                          select (myCubeGrid, inventoryBase, grinderAmount))
            {
                slimBlocks.Clear();
                myCubeGrid.GetBlocksInsideSphere(ref boundingSphereD, slimBlocks, true);
                foreach (MySlimBlock mySlimBlock in slimBlocks)
                    mySlimBlock.DecreaseMountLevel(grinderAmount, inventoryBase, false, 0L);
            }
        }
    }
}
