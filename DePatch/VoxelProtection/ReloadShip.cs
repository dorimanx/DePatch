using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace DePatch.VoxelProtection
{
    class ReloadShip
    {
        private static void FixGroup(List<MyCubeGrid> GridGroup)
        {
            var gridsList = new List<MyCubeGrid>();
            var ObList = new List<MyObjectBuilder_EntityBase>();
            var index = 0;
            var GridSizeForParallel = false;

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
                        if (cubeBlock is MyObjectBuilder_Projector Projector)
                            Projector.Enabled = false;
                    }
                }

                ob.PositionAndOrientation.Value.Orientation.Normalize();
                ObList.Add(ob);
                gridsList.Add(Grid);
            }

            if (ObList.Count == 0)
                return;

            foreach (var grid in gridsList)
            {
                if (grid.BlocksCount >= 200)
                    GridSizeForParallel = true;

                grid.Close();
            }

            MyObjectBuilder_CubeGrid[] cubeGrids = new MyObjectBuilder_CubeGrid[ObList.Count];

            foreach (var ObGrid in ObList)
            {
                cubeGrids[index] = (MyObjectBuilder_CubeGrid)ObGrid;
                index++;
            }

            MyAPIGateway.Entities.RemapObjectBuilderCollection(cubeGrids);

            ChangePosition(ref cubeGrids);

            var NewMyEntityList = new List<MyEntity>();
            var GridsCount = cubeGrids.Count();
            var GridsCreated = 0;

            foreach (var ObGrid in cubeGrids)
            {
                if (ObGrid.CubeBlocks.Count() <= 200)
                {
                    if (GridsCount > 1 && GridSizeForParallel)
                        GridsCount--;
                }
                else
                {
                    MyEntities.CreateFromObjectBuilderParallel(ObGrid, false, delegate (MyEntity grid)
                    {
                        var NewGrid = (MyCubeGrid)grid;

                        NewGrid.DetectDisconnectsAfterFrame();
                        NewMyEntityList.Add(grid);
                        ++GridsCreated;

                        if (GridsCount == GridsCreated)
                        {
                            NewMyEntityList.Reverse();

                            foreach (var ReadyGrid in NewMyEntityList)
                            {
                                MyEntities.Add(ReadyGrid, true);
                            }
                        }
                    });
                }
            }
        }

        private static void ChangePosition(ref MyObjectBuilder_CubeGrid[] grids)
        {
            var MainGridMatrix = grids[0].PositionAndOrientation.Value.GetMatrix();
            Vector3D oldpos = grids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

            //set new pos for grids
            for (int i = 0; i < grids.Length; i++)
            {
                var ob = grids[i];

                if (i == 0)
                {
                    if (ob.PositionAndOrientation.HasValue)
                    {
                        var posiyto = ob.PositionAndOrientation.GetValueOrDefault();
                        posiyto.Position = MainGridMatrix.Translation;
                        ob.PositionAndOrientation = posiyto;
                    }
                }
                else
                {
                    var o = ob.PositionAndOrientation.GetValueOrDefault();
                    o.Position = MainGridMatrix.Translation + o.Position - oldpos;
                    ob.PositionAndOrientation = o;
                }
            }
        }

        public static void FixShip(MyCubeGrid grid)
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

            FixGroup(grids);
        }
    }
}
