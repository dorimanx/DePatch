using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using System;
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
            private readonly List<IMyEntity> _entlist;
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
        private static readonly Random Random = new Random();

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

            // this code made by FOOGS! code ported from garage plugin!
            ChangePosition(ref cubeGrids);

            counter = new SpawnCounter.SpawnCallback(cubeGrids.Length);

            foreach (var grid in cubeGrids)
            {
                MyAPIGateway.Entities.CreateFromObjectBuilderParallel(grid, false, counter.Increment);
            }

            return true;
        }

        private static void ChangePosition(ref MyObjectBuilder_CubeGrid[] grids)
        {
            double randomdistance = Random.Next(1, 10);
            Vector3D First_SpawnPoint = Vector3D.Zero;
            var MainGridMatrix = grids[0].PositionAndOrientation.Value.GetMatrix();

            MainGridMatrix.Translation = RandomPositionFromPoint(MainGridMatrix.Translation, randomdistance);
            var sphere = new BoundingSphereD(Vector3D.Zero, 0);

            for (int i = 0; i < grids.Length; i++)
            {
                if (i == 0)
                    sphere = grids[i].CalculateBoundingSphere();
                else
                    sphere.Include(grids[i].CalculateBoundingSphere());
            }

            MainGridMatrix.Translation = MainGridMatrix.Translation + MainGridMatrix.Forward * (1 + sphere.Radius) + MainGridMatrix.Up * (1 + sphere.Radius);
            Vector3D GridPosition = MainGridMatrix.Translation;
            Vector3D? first_freepos = MyEntities.FindFreePlaceCustom(GridPosition, ((float)sphere.Radius * 0) + 1, 20, 5, 10, 2);

            if (first_freepos != null)
                First_SpawnPoint = (Vector3D)first_freepos;
            else
            {
                first_freepos = MyEntities.FindFreePlaceCustom(GridPosition, ((float)sphere.Radius * 0) + 50, 20, 15, 15, 10);
                if (first_freepos == null || first_freepos == Vector3D.Zero)
                    First_SpawnPoint = GridPosition;
            }

            if (First_SpawnPoint == Vector3D.Zero)
                return;

            sphere.Center = First_SpawnPoint;

            //First loop
            //set new pos for grids
            Vector3D first_oldpos = grids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
            for (int i = 0; i < grids.Length; i++) // set position
            {
                var ob = grids[i];

                if (i == 0)
                {
                    if (ob.PositionAndOrientation.HasValue)
                    {
                        var posiyto = ob.PositionAndOrientation.GetValueOrDefault();
                        posiyto.Position = First_SpawnPoint;
                        ob.PositionAndOrientation = posiyto;
                    }
                }
                else
                {
                    var o = ob.PositionAndOrientation.GetValueOrDefault();
                    o.Position = First_SpawnPoint + o.Position - first_oldpos;
                    ob.PositionAndOrientation = o;
                }
            }

            grids = AlightToGravity(grids);

            Vector3D Second_SpawnPoint = (Vector3D)MyAPIGateway.Entities.FindFreePlace(First_SpawnPoint, (float)sphere.Radius + 5, 24, 3, 1); // - basePos
            Vector3D second_oldpos = grids[0].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

            for (int i = 0; i < grids.Length; i++) // set position
            {
                var ob = grids[i];

                if (i == 0)
                {
                    if (ob.PositionAndOrientation.HasValue)
                    {
                        var posiyto = ob.PositionAndOrientation.GetValueOrDefault();
                        posiyto.Position = Second_SpawnPoint;
                        ob.PositionAndOrientation = posiyto;
                    }
                }
                else
                {
                    var o = ob.PositionAndOrientation.GetValueOrDefault();
                    o.Position = Second_SpawnPoint + o.Position - second_oldpos;
                    ob.PositionAndOrientation = o;
                }
            }
        }

        private static MyObjectBuilder_CubeGrid[] AlightToGravity(MyObjectBuilder_CubeGrid[] grids)
        {
            Vector3 m_pasteDirForward = new Vector3(0f, 1f, 0f);

            var main_grid = grids[0];
            var pos = main_grid.PositionAndOrientation.Value.Position;
            var hack = main_grid.PositionAndOrientation.Value.GetMatrix();

            Vector3 gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(hack.Translation); //find gravity
            gravity.Normalize();
            m_pasteDirForward = Vector3D.Reject(m_pasteDirForward, gravity);
            Vector3 m_pasteDirUp = -gravity;
            m_pasteDirForward = Vector3.Normalize(m_pasteDirForward);
            m_pasteDirUp = Vector3.Normalize(m_pasteDirUp);
            Vector3 crossed = Vector3.Cross(m_pasteDirForward, m_pasteDirUp);
            m_pasteDirForward -= crossed;

            int i = 0;
            var matrixx = Matrix.Multiply(Matrix.Invert(hack), Matrix.CreateWorld((Vector3D)pos, m_pasteDirForward, m_pasteDirUp));

            while (i < grids.Length && i <= grids.Length - 1)
            {
                if (grids[i].PositionAndOrientation != null)
                {
                    grids[i].CreatePhysics = true;
                    grids[i].EnableSmallToLargeConnections = true;
                    grids[i].PositionAndOrientation = new MyPositionAndOrientation?(new MyPositionAndOrientation(grids[i].PositionAndOrientation.Value.GetMatrix() * matrixx));
                    grids[i].PositionAndOrientation.Value.Orientation.Normalize();

                    i++;
                }
            }

            return grids;
        }

        /// <summary>
        ///     Randomizes a vector by the given amount
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3D RandomPositionFromPoint(Vector3D start, double distance)
        {
            double z = Random.NextDouble() * 2 - 1;
            double piVal = Random.NextDouble() * 2 * Math.PI;
            double zSqrt = Math.Sqrt(1 - z * z);
            var direction = new Vector3D(zSqrt * Math.Cos(piVal), zSqrt * Math.Sin(piVal), z);

            direction.Normalize();
            start += direction * -2;

            return start + direction * distance;
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
