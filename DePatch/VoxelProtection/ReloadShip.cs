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

namespace DePatch.VoxelProtection
{
    class ReloadShip
    {
        private static ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> FindGridGroups(long gridID)
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();
            Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group =>
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    if (grid.Physics == null)
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

        private static bool CheckGroups(ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups,
                                        out MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group)
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
            List<MyObjectBuilder_EntityBase> objectBuilderList = new List<MyObjectBuilder_EntityBase>();
            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();

            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes)
            {
                MyCubeGrid grid = groupNodes.NodeData;
                gridsList.Add(grid);

                grid.Physics.ClearSpeed();

                MyObjectBuilder_EntityBase ob = grid.GetObjectBuilder(true);

                if (!objectBuilderList.Contains(ob))
                {
                    if (ob is MyObjectBuilder_CubeGrid gridBuilder)
                    {
                        foreach (MyObjectBuilder_CubeBlock cubeBlock in gridBuilder.CubeBlocks)
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

            foreach (MyCubeGrid grid in gridsList)
            {
                if (!(grid is IMyEntity entity) || entity.MarkedForClose || entity.Closed)
                    continue;

                entity.Close();
            }

            MyAPIGateway.Entities.RemapObjectBuilderCollection(objectBuilderList);

            for (int i = 0; i < objectBuilderList.Count; i++)
            {
                MyAPIGateway.Entities.CreateFromObjectBuilderParallel(objectBuilderList[i], true);
            }

            return true;
        }

        private static bool FixGroups(ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups)
        {
            var result = CheckGroups(groups, out MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group);

            if (!result)
                return result;

            return FixGroup(group);
        }

        public static bool FixShip(long gridID)
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups = FindGridGroups(gridID);

            return FixGroups(groups);
        }

    }
}
