using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Groups;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace DePatch.VoxelProtection
{
    class ReloadShip
    {
        private static ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> FindGridGroups(long gridID)
        {
            var groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();

            _ = Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group =>
              {
                  foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes)
                  {
                      MyCubeGrid grid = groupNodes.NodeData;

                      if (grid.Physics is null)
                          continue;

                      /* Gridname is wrong ignore */
                      if (!grid.EntityId.Equals(gridID))
                          continue;

                      groups.Add(group);
                      break;
                  }
              });

            return groups;
        }

        private static bool CheckGroups(ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups, out MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group)
        {
            /* No group or too many groups found */
            if (groups.Count < 1)
            {
                group = null;
                return false;
            }

            /* too many groups found */
            if (groups.Count > 1)
            {
                group = null;
                return false;
            }

            if (!groups.TryPeek(out group))
                return false;

            return true;
        }

        private static bool FixGroup(MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group)
        {
            var objectBuilderList = new List<MyObjectBuilder_EntityBase>();
            var gridsList = new List<MyCubeGrid>();

            foreach (var groupNodes in group.Nodes)
            {
                var grid = groupNodes.NodeData;

                gridsList.Add(grid);

                grid.Physics.ClearSpeed();

                var ob = grid.GetObjectBuilder(true);

                if (!objectBuilderList.Contains(ob))
                {
                    if (ob is MyObjectBuilder_CubeGrid gridBuilder)
                    {
                        foreach (var cubeBlock in gridBuilder.CubeBlocks)
                        {
                            if (cubeBlock is MyObjectBuilder_OxygenTank o2Tank)
                                o2Tank.AutoRefill = false;
                            if (cubeBlock is MyObjectBuilder_MotorStator MotorStator)
                                MotorStator.RotorLock = true;
                            if (cubeBlock is MyObjectBuilder_MotorAdvancedStator MotorAdvancedStator)
                                MotorAdvancedStator.RotorLock = true;
                        }
                    }
                    ob.PositionAndOrientation.Value.Orientation.Normalize();
                    objectBuilderList.Add(ob);
                }
            }

            foreach (var grid in gridsList)
            {
                if (!(grid is IMyEntity entity) || entity.MarkedForClose || entity.Closed)
                    continue;

                entity.Close();
            }

            MyAPIGateway.Entities.RemapObjectBuilderCollection(objectBuilderList);

            var MainGridOrgPosition = objectBuilderList[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
            var NewPositionShift = (Vector3D.Forward * 6) + (Vector3D.Up * 9);
            var MainGridNewPos = MainGridOrgPosition + NewPositionShift;

            for (int i = 0; i < objectBuilderList.Count; i++)
            {
                var InspectedGrid = (MyObjectBuilder_CubeGrid)objectBuilderList[i];

                if (i == 0)
                {
                    var mainGridPos = InspectedGrid.PositionAndOrientation.GetValueOrDefault();
                    mainGridPos.Position = MainGridNewPos;
                    InspectedGrid.PositionAndOrientation = mainGridPos;
                }
                else
                {
                    var AttachedGridPos = InspectedGrid.PositionAndOrientation.GetValueOrDefault();
                    AttachedGridPos.Position = AttachedGridPos.Position + MainGridNewPos - MainGridOrgPosition;
                    InspectedGrid.PositionAndOrientation = AttachedGridPos;
                }

                _ = MyAPIGateway.Entities.CreateFromObjectBuilderParallel(InspectedGrid, addToScene: true);
            }

            return true;
        }

        public static bool FixShip(long gridID)
        {
            var result = CheckGroups(FindGridGroups(gridID), out var group);

            if (!result)
                return result;

            return FixGroup(group);
        }
    }
}
