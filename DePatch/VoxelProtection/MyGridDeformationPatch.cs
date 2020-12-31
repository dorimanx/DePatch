using System;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Game.Entity;
using VRage.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Common.ObjectBuilders;
using System.Collections.Generic;
using VRageMath;
using System.Linq;
using VRage.Groups;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Sandbox.Game.Entities.Cube;
using VRage.Network;
using VRage;
using Sandbox.Game.Weapons;

namespace DePatch
{
    public class MyGridDeformationPatch
    {
        private static bool _init;

        public static void Init()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (!_init)
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, HandleGridDamage);
                _init = true;
            }
        }

        public static ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> FindGridGroup(string gridname)
        {
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();
            Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group =>
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;

                    if (grid.Physics == null)
                        continue;

                    if (!grid.DisplayName.Equals(gridname) && grid.EntityId + "" != gridname)
                        continue;

                    groups.Add(group);
                }
            });

            return groups;
        }

        private static void HandleGridDamage(object target, ref MyDamageInformation damage)
        {
            if (DePatchPlugin.Instance.Config.ProtectGrid && DePatchPlugin.Instance.Config.Enabled)
            {
                if (damage.Type == MyDamageType.Deformation || damage.Type == MyDamageType.Fall)
                {
                    MyEntities.TryGetEntityById(damage.AttackerId, out MyEntity AttackerEntity, allowClosed: true);

                    IMySlimBlock GridBlock = target as IMySlimBlock;
                    MyPhysicsComponentBase GridPhysics = GridBlock?.CubeGrid.Physics;
                    IMyCubeGrid GridCube = GridBlock?.CubeGrid;
                    MyCubeGrid Grid = (MyCubeGrid)GridCube;

                    if (GridBlock != null && GridPhysics != null &&
                        (GridCube.GridSizeEnum == MyCubeSize.Large && Grid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize ||
                         GridCube.GridSizeEnum == MyCubeSize.Small && Grid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize))
                    {
                        float speed = DePatchPlugin.Instance.Config.MinProtectSpeed;
                        var LinearVelocity = GridPhysics.LinearVelocity;
                        var AngularVelocity = GridPhysics.AngularVelocity;

                        if (LinearVelocity.Length() < speed && AngularVelocity.Length() < speed)
                        { //by voxel or grid on low speed
                            if (AttackerEntity is MyVoxelBase)
                            {
                                if (damage.IsDeformation) damage.IsDeformation = false;
                                if (damage.Amount != 0f) damage.Amount = 0f;

                                return;
                            }
                            else
                            {
                                if (Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                                {
                                    if (damage.IsDeformation) damage.IsDeformation = false;
                                    if (damage.Amount != 0f) damage.Amount = 0f;
                                }
                            }
                        }
                        else
                        {
                            if (AttackerEntity is MyVoxelBase)
                            { // by voxel on high speed
                                if (damage.IsDeformation) damage.IsDeformation = false;

                                if (damage.Amount != 0f)
                                {
                                    if (damage.Amount >= DePatchPlugin.Instance.Config.DamgeToBlocksVoxel)
                                        damage.Amount = DePatchPlugin.Instance.Config.DamgeToBlocksVoxel;
                                }

                                _ = MyGravityProviderSystem.CalculateNaturalGravityInPoint(Grid.PositionComp.GetPosition(), out float ingravitynow);
                                if (ingravitynow <= 20 && ingravitynow >= 0.2)
                                {
                                    if (DePatchPlugin.Instance.Config.ConvertToStatic &&
                                            Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxGridSizeToConvert &&
                                            (LinearVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed || AngularVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed))
                                    {
                                        BoundingBoxD worldAABB = Grid.PositionComp.WorldAABB;
                                        MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
                                        double elevation = double.PositiveInfinity;
                                        if (closestPlanet != null)
                                        {
                                            Vector3D centerOfMassWorld = GridPhysics.CenterOfMassWorld;
                                            Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref centerOfMassWorld);
                                            elevation = Vector3D.Distance(closestSurfacePointGlobal, centerOfMassWorld);
                                        }
                                        else
                                        {
                                            elevation = double.PositiveInfinity;
                                        }

                                        if (elevation < 150 && elevation != double.PositiveInfinity &&
                                            Grid.GetFatBlockCount<MyMotorSuspension>() < 4 &&
                                            Grid.GetFatBlockCount<MyThrust>() >= 6)
                                        {
                                            if (damage.Amount != 0f)
                                                damage.Amount = 0f;

                                            /*
                                            if (GridCube is MyObjectBuilder_LandingGear)
                                            {
                                                (GridCube as MyObjectBuilder_LandingGear).AttachedEntityId = null;
                                                (GridCube as MyObjectBuilder_LandingGear).IsLocked = false;
                                                (GridCube as MyObjectBuilder_LandingGear).LockMode = SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Unlocked;
                                                (GridCube as MyObjectBuilder_LandingGear).Enabled = true;
                                            }
                                            */

                                            GridPhysics?.ClearSpeed();

                                            foreach (var a in Grid.GetFatBlocks<MyCockpit>())
                                            {
                                                if (a != null)
                                                    a.RemovePilot();
                                            }
                                            foreach (var b in Grid.GetFatBlocks<MyCryoChamber>())
                                            {
                                                if (b != null)
                                                    b.RemovePilot();
                                            }

                                            foreach (var projector in Grid.GetFatBlocks<MyProjectorBase>())
                                            {
                                                if (projector.ProjectedGrid == null) continue;

                                                projector.Enabled = false;
                                            }

                                            foreach (var drills in Grid.GetFatBlocks<MyShipDrill>())
                                            {
                                                if (drills != null && drills.Enabled)
                                                    drills.Enabled = false;
                                            }

                                            /* This part of code belong to Foogs great plugin dev! */
                                            List<MyCubeGrid> grids = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(Grid);
                                            grids.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
                                            grids.Reverse();
                                            grids.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                                            MyMultiplayer.RaiseEvent(grids.First(), (MyCubeGrid x) => new Action(x.ConvertToStatic), default(EndpointId));

                                            /* This part of code belong to LordTylus great plugin dev! FixShip after converting to static */
                                            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> gridWithSubGrids = FindGridGroup(Grid.DisplayName);
                                            List<MyObjectBuilder_EntityBase> objectBuilderList = new List<MyObjectBuilder_EntityBase>();
                                            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();

                                            foreach (var item in gridWithSubGrids)
                                            {
                                                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in item.Nodes)
                                                {
                                                    MyCubeGrid grid = groupNodes.NodeData;
                                                    gridsList.Add(grid);

                                                    MyObjectBuilder_EntityBase ob = grid.GetObjectBuilder(true);

                                                    if (!objectBuilderList.Contains(ob))
                                                    {
                                                        if (ob is MyObjectBuilder_CubeGrid gridBuilder)
                                                        {
                                                            foreach (MyObjectBuilder_CubeBlock cubeBlock in gridBuilder.CubeBlocks)
                                                            {
                                                                if (cubeBlock is MyObjectBuilder_OxygenTank o2Tank)
                                                                    o2Tank.AutoRefill = false;
                                                            }
                                                        }
                                                        objectBuilderList.Add(ob);
                                                    }
                                                }
                                            }

                                            foreach (MyCubeGrid grid in gridsList)
                                            {
                                                IMyEntity entity = grid;
                                                if (entity == null || entity.MarkedForClose || entity.Closed)
                                                    continue;

                                                entity.Close();
                                            }

                                            var DisableThisForNow = false;
                                            if (DisableThisForNow)
                                            {
                                                /* This insane code belong to Dorimanx */
                                                Vector3D PastePositionUpDown = (Vector3D.Up + Vector3D.Up + Vector3D.Up) * 30;
                                                Vector3D PastePositioBackForward = (Vector3D.Backward + Vector3D.Backward + Vector3D.Backward) * 30;
                                                Vector3D PastePositioRightLeft = (Vector3D.Right + Vector3D.Right + Vector3D.Right) * 30;
                                                var MinElevation = 35;

                                                for (int i = 0; i < objectBuilderList.Count; i++)
                                                {
                                                    MyObjectBuilder_EntityBase ob = objectBuilderList[i];

                                                    if (ob.PositionAndOrientation.HasValue)
                                                    {
                                                        Vector3D OriginalPosition = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;
                                                        MyPositionAndOrientation PasteTo = ob.PositionAndOrientation.GetValueOrDefault();

                                                        if (elevation < MinElevation)
                                                        {
                                                            PasteTo.Position = OriginalPosition - PastePositionUpDown;
                                                            ob.PositionAndOrientation = PasteTo;
                                                            Vector3D NewPosition = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                            Vector3D closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition);
                                                            double? elevation2 = Vector3D.Distance(closestSurfacePointGlobal, NewPosition);

                                                            if (elevation2 < MinElevation)
                                                            {
                                                                PasteTo.Position = OriginalPosition;
                                                                ob.PositionAndOrientation = PasteTo;

                                                                PasteTo.Position = OriginalPosition + PastePositionUpDown;
                                                                ob.PositionAndOrientation = PasteTo;
                                                                Vector3D NewPosition2 = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                                Vector3D closestSurfacePointGlobal2 = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition2);
                                                                double? elevation3 = Vector3D.Distance(closestSurfacePointGlobal2, NewPosition2);

                                                                if (elevation3 < MinElevation)
                                                                {
                                                                    PasteTo.Position = OriginalPosition;
                                                                    ob.PositionAndOrientation = PasteTo;

                                                                    PasteTo.Position = OriginalPosition - PastePositioBackForward;
                                                                    ob.PositionAndOrientation = PasteTo;
                                                                    Vector3D NewPosition3 = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                                    Vector3D closestSurfacePointGlobal3 = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition3);
                                                                    double? elevation4 = Vector3D.Distance(closestSurfacePointGlobal3, NewPosition3);

                                                                    if (elevation4 < MinElevation)
                                                                    {
                                                                        PasteTo.Position = OriginalPosition;
                                                                        ob.PositionAndOrientation = PasteTo;

                                                                        PasteTo.Position = OriginalPosition + PastePositioBackForward;
                                                                        ob.PositionAndOrientation = PasteTo;
                                                                        Vector3D NewPosition4 = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                                        Vector3D closestSurfacePointGlobal4 = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition4);
                                                                        double? elevation5 = Vector3D.Distance(closestSurfacePointGlobal4, NewPosition4);

                                                                        if (elevation5 < MinElevation)
                                                                        {
                                                                            PasteTo.Position = OriginalPosition;
                                                                            ob.PositionAndOrientation = PasteTo;

                                                                            PasteTo.Position = OriginalPosition - PastePositioRightLeft;
                                                                            ob.PositionAndOrientation = PasteTo;
                                                                            Vector3D NewPosition5 = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                                            Vector3D closestSurfacePointGlobal5 = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition5);
                                                                            double? elevation6 = Vector3D.Distance(closestSurfacePointGlobal5, NewPosition5);

                                                                            if (elevation6 < MinElevation)
                                                                            {
                                                                                PasteTo.Position = OriginalPosition;
                                                                                ob.PositionAndOrientation = PasteTo;

                                                                                PasteTo.Position = OriginalPosition + PastePositioRightLeft;
                                                                                ob.PositionAndOrientation = PasteTo;
                                                                                Vector3D NewPosition6 = objectBuilderList[i].PositionAndOrientation.GetValueOrDefault().Position + Vector3D.Zero;

                                                                                Vector3D closestSurfacePointGlobal6 = closestPlanet.GetClosestSurfacePointGlobal(ref NewPosition6);
                                                                                double? elevation7 = Vector3D.Distance(closestSurfacePointGlobal6, NewPosition6);


                                                                                if (elevation7 < MinElevation)
                                                                                {
                                                                                    PasteTo.Position = OriginalPosition;
                                                                                    ob.PositionAndOrientation = PasteTo;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            MyAPIGateway.Entities.RemapObjectBuilderCollection(objectBuilderList);

                                            foreach (MyObjectBuilder_EntityBase ob in objectBuilderList)
                                            {
                                                _ = MyAPIGateway.Entities.CreateFromObjectBuilderParallel(ob, true, null);
                                            }
                                        }
                                    }
                                    else
                                        GridPhysics?.ClearSpeed();
                                }
                                else
                                {
                                    if (damage.Amount != 0f)
                                    {
                                        if (damage.Amount >= DePatchPlugin.Instance.Config.DamgeToBlocksVoxel)
                                            damage.Amount = DePatchPlugin.Instance.Config.DamgeToBlocksVoxel;
                                    }
                                }
                                return;
                            }

                            if (Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                            { // by grid bump high speed
                                if (damage.IsDeformation) damage.IsDeformation = false;

                                if (damage.Amount != 0f)
                                {
                                    if (DePatchPlugin.Instance.Config.DamgeToBlocksRamming == 0f)
                                        DePatchPlugin.Instance.Config.DamgeToBlocksRamming = 0.5f;

                                    if (damage.Amount >= DePatchPlugin.Instance.Config.DamgeToBlocksRamming)
                                        damage.Amount = DePatchPlugin.Instance.Config.DamgeToBlocksRamming;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
