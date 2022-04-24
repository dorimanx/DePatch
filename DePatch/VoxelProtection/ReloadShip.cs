using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace DePatch.VoxelProtection
{
    public static class SpawnCounter
    {
        public class SpawnCallback
        {
            private int _counter;
            private List<IMyEntity> _entlist;
            private readonly int _maxCount;

            public SpawnCallback(int count)
            {
                _counter = 0;
                _entlist = new List<IMyEntity>();
                _maxCount = count;
            }

            public void Increment(IMyEntity ent)
            {
                _counter++;
                _entlist.Add(ent);

                if (_counter < _maxCount)
                    return;

                FinalSpawnCallback(_entlist);
            }

            private static void FinalSpawnCallback(List<IMyEntity> grids)
            {
                foreach (MyCubeGrid ent in grids)
                {
                    ent.DetectDisconnectsAfterFrame();
                    MyAPIGateway.Entities.AddEntity(ent, true);
                }
            }
        }
    }

    class ReloadShip
    {
        private static bool FixGroup(List<MyCubeGrid> GridGroup)
        {
            var gridsList = new List<MyCubeGrid>();
            var ObList = new List<MyObjectBuilder_EntityBase>();
            SpawnCounter.SpawnCallback counter = null;
            var index = 0;

            foreach (var Grid in GridGroup)
            {
                if (!(Grid is IMyEntity entity) || entity.MarkedForClose || entity.Closed)
                    continue;

                Grid.Physics.ClearSpeed();
                var ob = Grid.GetObjectBuilder(true);

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
                ObList.Add(ob);
                gridsList.Add(Grid);
            }

            if (ObList.Count == 0)
                return false;

            foreach (var grid in gridsList)
            {
                grid.Close();
            }

            MyObjectBuilder_CubeGrid[] cubeGrids = new MyObjectBuilder_CubeGrid[ObList.Count];

            foreach (var ObGrid in ObList)
            {
                cubeGrids[index] = (MyObjectBuilder_CubeGrid)ObGrid;
                index++;
            }

            MyAPIGateway.Entities.RemapObjectBuilderCollection(cubeGrids);
            MatrixD projection_matrix = cubeGrids[0].PositionAndOrientation.Value.GetMatrix();
            SetNewRotationGroup(ref cubeGrids, projection_matrix.Forward, projection_matrix.Up);
            counter = new SpawnCounter.SpawnCallback(cubeGrids.Length);

            foreach (var grid in cubeGrids)
            {
                MyAPIGateway.Entities.CreateFromObjectBuilderParallel(grid, false, counter.Increment);
            }

            return true;
        }

        private static void SetNewRotationGroup(ref MyObjectBuilder_CubeGrid[] grids, Vector3D forward, Vector3D up)
        {
            Vector3 m_pasteDirUp = up * (Vector3.Up * 25);
            Vector3 m_pasteDirForward = forward;
            var main_grid = grids[0];
            var main_grid_pos = main_grid.PositionAndOrientation.Value.Position;
            var main_grid_matrix = main_grid.PositionAndOrientation.Value.GetMatrix();

            int i = 0;
            var rotation_matrix = Matrix.Multiply(Matrix.Invert(main_grid_matrix), Matrix.CreateWorld((Vector3D)main_grid_pos, m_pasteDirForward, m_pasteDirUp));

            while (i < grids.Length && i <= grids.Length - 1)
            {
                //copied from UpdateGridTransformations
                if (grids[i].PositionAndOrientation != null)
                {
                    grids[i].CreatePhysics = true;
                    grids[i].DestructibleBlocks = true;
                    grids[i].EnableSmallToLargeConnections = true;
                    grids[i].PositionAndOrientation = new MyPositionAndOrientation?(new MyPositionAndOrientation(grids[i].PositionAndOrientation.Value.GetMatrix() * rotation_matrix));
                    grids[i].PositionAndOrientation.Value.Orientation.Normalize();
                    i++;
                }
            }
        }

        public static bool FixShip(MyCubeGrid grid)
        {
            // only here we can see attached by landing gear grids to main grid!
            var IMygrids = new List<IMyCubeGrid>();
            MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical, IMygrids);

            // convert back to MyCubeGrid
            var grids = new List<MyCubeGrid>();
            foreach (var Mygrid in IMygrids)
            {
                grids.Add((MyCubeGrid)Mygrid);
            }

            // sort the list. largest to smallest
            grids.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
            grids.Reverse();
            grids.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

            return FixGroup(grids);
        }
    }
}
