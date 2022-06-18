using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
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
        private static bool FixGroup(List<MyCubeGrid> GridGroup)
        {
            var gridsList = new List<MyCubeGrid>();
            var ObList = new List<MyObjectBuilder_EntityBase>();
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

            Vector3D GridCockpit = Vector3D.Zero;

            foreach (var Block in gridsList[0].GetFatBlocks())
            {
                if (Block is MyCockpit Cockpit)
                {
                    if ((Cockpit.IsMainCockpit && Cockpit.IsFunctional) || Cockpit.IsFunctional)
                    {
                        GridCockpit = Cockpit.PositionComp.GetPosition();
                        break;
                    }
                }
            }

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

            // this code made by FOOGS! code ported from garage plugin!
            ChangePosition(ref cubeGrids, GridCockpit);

            var NewMyEntityList = new List<MyEntity>();
            var GridsCount = cubeGrids.Count();
            var GridsCreated = 0;

            foreach (var ObGrid in cubeGrids)
            {
                MyEntities.CreateFromObjectBuilderParallel(ObGrid, false, delegate (MyEntity grid)
                {
                    var NewGrid = (MyCubeGrid)grid;

                    if (grid.Physics != null)
                    {
                        grid.Physics.Gravity = Vector3.Zero;
                        grid.Physics.ClearSpeed();
                        grid.Physics.Deactivate();
                    }

                    NewGrid.DetectDisconnectsAfterFrame();
                    NewMyEntityList.Add(grid);
                    ++GridsCreated;

                    if (GridsCount == GridsCreated)
                    {
                        NewMyEntityList.Reverse();

                        foreach (var ReadyGrid in NewMyEntityList)
                        {
                            MyEntities.Add(ReadyGrid, true);

                            if (ReadyGrid.Physics != null)
                            {
                                var GridGavity = (MyCubeGrid)ReadyGrid;
                                ReadyGrid.Physics.Activate();
                                ReadyGrid.Physics.Gravity = Vector3.Zero;
                                GridGavity.Physics.DisableGravity = 2;
                            }
                        }
                    }
                });
            }

            return true;
        }

        private static void ChangePosition(ref MyObjectBuilder_CubeGrid[] grids, Vector3D GridCockpit)
        {
            Vector3D First_SpawnPoint = Vector3D.Zero;
            var MainGridMatrix = grids[0].PositionAndOrientation.Value.GetMatrix();
            var sphere = new BoundingSphereD(Vector3D.Zero, 0);
            double SphereRadius;

            for (int i = 0; i < grids.Length; i++)
            {
                if (i == 0)
                    sphere = grids[i].CalculateBoundingSphere();
                else
                    sphere.Include(grids[i].CalculateBoundingSphere());
            }

            Vector3D GridPosition = MainGridMatrix.Translation;
            Vector3D? first_freepos = (Vector3D)MyAPIGateway.Entities.FindFreePlace(GridPosition, radius: 5, maxTestCount: 25, testsPerDistance: 5, stepSize: 5);

            if (first_freepos != null)
                First_SpawnPoint = (Vector3D)first_freepos;
            else
            {
                first_freepos = (Vector3D)MyAPIGateway.Entities.FindFreePlace(GridPosition, radius: 25, maxTestCount: 25, testsPerDistance: 10, stepSize: 10);
                if (first_freepos == null || first_freepos == Vector3D.Zero)
                    First_SpawnPoint = GridPosition;
            }

            if (First_SpawnPoint == Vector3D.Zero)
                return;

            //First loop
            sphere.Center = First_SpawnPoint;
            Vector3D first_oldpos = grids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

            //set new pos for grids
            SetGridspositions(grids, First_SpawnPoint, first_oldpos);

            grids = SetAlignedToGravity(grids, GridCockpit);

            // Second loop after gravity alignment.
            if (sphere.Radius > 30 || sphere.Radius < 15)
                SphereRadius = 30;
            else
                SphereRadius = sphere.Radius;

            Vector3D GravityAligned_SpawnPoint = (Vector3D)MyAPIGateway.Entities.FindFreePlace(First_SpawnPoint, radius: (float)SphereRadius, maxTestCount: 30, testsPerDistance: 5, stepSize: 5); // - basePos
            Vector3D second_oldpos = grids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

            // correct position for all grids.
            SetGridspositions(grids, GravityAligned_SpawnPoint, second_oldpos);
        }

        private static void SetGridspositions(MyObjectBuilder_CubeGrid[] grids, Vector3D SpawnPoint, Vector3D oldpos)
        {
            for (int i = 0; i < grids.Length; i++)
            {
                var ob = grids[i];

                if (i == 0)
                {
                    if (ob.PositionAndOrientation.HasValue)
                    {
                        var posiyto = ob.PositionAndOrientation.GetValueOrDefault();
                        posiyto.Position = SpawnPoint;
                        ob.PositionAndOrientation = posiyto;
                    }
                }
                else
                {
                    var o = ob.PositionAndOrientation.GetValueOrDefault();
                    o.Position = SpawnPoint + o.Position - oldpos;
                    ob.PositionAndOrientation = o;
                }
            }
        }

        private static MyObjectBuilder_CubeGrid[] SetAlignedToGravity(MyObjectBuilder_CubeGrid[] grids, Vector3D GridCockpit)
        {
            Vector3D position = grids[0].PositionAndOrientation.Value.Position;
            Vector3D direction;
            float gravityOffset = 15f;
            float gravityRotation = 0f;
            Vector3D vector3D;
            int i = 0;

            if (GridCockpit != Vector3D.Zero)
                position = GridCockpit;

            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            if (vector == Vector3.Zero)
                vector = MyGravityProviderSystem.CalculateArtificialGravityInPoint(position, 1f);

            if (vector != Vector3.Zero)
            {
                vector.Normalize();
                //vector3D = -vector;
                vector3D = Vector3D.Down;
                position += vector * gravityOffset;

                direction = Vector3D.CalculatePerpendicularVector(vector);
                if (gravityRotation != 0f)
                {
                    MatrixD matrix = MatrixD.CreateFromAxisAngle(vector3D, gravityRotation);
                    direction = Vector3D.Transform(direction, matrix);
                }
            }
            else
            {
                direction = Vector3D.Right;
                vector3D = Vector3D.Up;
            }

            while (i < grids.Length && i <= grids.Length - 1)
            {
                if (grids[i].PositionAndOrientation != null)
                {
                    grids[i].CreatePhysics = true;
                    grids[i].EnableSmallToLargeConnections = true;
                    i++;
                }
            }

            MatrixD worldMatrix = MatrixD.CreateWorld(position, direction, vector3D);
            RelocateGrids(grids, worldMatrix);

            return grids;
        }

        private static void RelocateGrids(MyObjectBuilder_CubeGrid[] cubegrids, MatrixD worldMatrix0)
        {
            MatrixD matrix = cubegrids[0].PositionAndOrientation.Value.GetMatrix();
            MatrixD matrixD = Matrix.Invert(matrix) * worldMatrix0.GetOrientation();
            Matrix matrix2 = matrixD;

            foreach (MyObjectBuilder_CubeGrid Grid in cubegrids)
            {
                if (Grid.PositionAndOrientation != null)
                {
                    MatrixD matrixD2 = Grid.PositionAndOrientation.Value.GetMatrix();
                    Vector3 value = Vector3.TransformNormal(matrixD2.Translation - matrix.Translation, matrix2);
                    matrixD2 *= matrix2;
                    Vector3D translation = worldMatrix0.Translation + value;
                    matrixD2.Translation = Vector3D.Zero;
                    matrixD2 = MatrixD.Orthogonalize(matrixD2);
                    matrixD2.Translation = translation;
                    Grid.PositionAndOrientation = new MyPositionAndOrientation?(new MyPositionAndOrientation(ref matrixD2));
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
