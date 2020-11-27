using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch
{
    [HarmonyPatch(typeof(MyShipWelder), "Activate")]
    internal class ShipWelderPatch
    {
        private static readonly FieldInfo m_detectorSphere = typeof(MyShipToolBase).GetField("m_detectorSphere", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly HashSet<MySlimBlock> slimBlocks = new HashSet<MySlimBlock>();

        private static void Prefix(MyShipWelder __instance)
        {
            MyShipWelder __instance2 = __instance;
            if (!DePatchPlugin.Instance.Config.Enabled && !DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return;

            IEnumerable<ShipTool> enumerable = ShipTool.shipTools.Where((ShipTool t) => t.Subtype == __instance2.DefinitionId.SubtypeId.String);
            if (enumerable.Count() == 0)
            {
                DePatchPlugin.Instance.Control.Dispatcher.Invoke(delegate
                {
                    ShipTool.shipTools.Add(new ShipTool
                    {
                        Speed = ShipTool.DEFAULT_SPEED,
                        Subtype = __instance2.DefinitionId.SubtypeId.String,
                        Type = ToolType.Welder
                    });
                });
                enumerable.AddItem(new ShipTool
                {
                    Speed = ShipTool.DEFAULT_SPEED,
                    Subtype = __instance2.DefinitionId.SubtypeId.String,
                    Type = ToolType.Welder
                });
            }
            List<MyEntity> list = new List<MyEntity>();
            BoundingSphere boundingSphere = (BoundingSphere)m_detectorSphere.GetValue(__instance2);
            BoundingSphereD boundingSphereD = new BoundingSphereD(Vector3D.Transform(boundingSphere.Center, __instance2.CubeGrid.WorldMatrix), boundingSphere.Radius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref boundingSphereD, list);
            if (list.Contains(__instance2.CubeGrid))
            {
                list.Remove(__instance2.CubeGrid);
            }
            foreach (MyEntity myEntity in list)
            {
                MyCubeGrid myCubeGrid = myEntity as MyCubeGrid;
                if (myCubeGrid != null && myEntity.Physics != null)
                {
                    MyInventoryBase inventoryBase = __instance2.GetInventoryBase();
                    bool helpOthers = __instance2.HelpOthers;
                    float welderMountAmount = MySession.Static.WelderSpeedMultiplier * enumerable.First().Speed;
                    slimBlocks.Clear();
                    myCubeGrid.GetBlocksInsideSphere(ref boundingSphereD, slimBlocks, checkTriangles: true);
                    foreach (MySlimBlock mySlimBlock in slimBlocks)
                    {
                        mySlimBlock.IncreaseMountLevel(welderMountAmount, __instance.OwnerId, inventoryBase, 0.6f, helpOthers);
                    }
                }
            }
        }
    }
}
