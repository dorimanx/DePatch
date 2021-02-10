using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Entities.Planet;
using Sandbox.Game.GameSystems;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Groups;
using VRage.ModAPI;
using VRage.Network;
using VRage.ObjectBuilders;
using VRageMath;

namespace DePatch.VoxelProtection
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
            var groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();
            Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group =>
            {
                foreach (var groupNodes in group.Nodes)
                {
                    var grid = groupNodes.NodeData;

                    if (grid.Physics == null)
                        continue;

                    if (!grid.DisplayName.Equals(gridname) && grid.EntityId + "" != gridname)
                        continue;

                    groups.Add(group);
                }
            });

            return groups;
        }

        private static bool CheckAdminGrid(object target)
        {
            if ((target as IMySlimBlock) == null)
                return false;

            var mySlimBlock = target as IMySlimBlock;
            if (mySlimBlock.CubeGrid == null)
                return false;

            List<IMySlimBlock> blocks = new List<IMySlimBlock>();

            mySlimBlock.CubeGrid.GetBlocks(blocks, (x) => (x.FatBlock is IMyTerminalBlock) && x.FatBlock.BlockDefinition.SubtypeId.Contains("AdminGrid"));
            return blocks.Count > 0;
        }

        private static void HandleGridDamage(object target, ref MyDamageInformation damage)
        {
            if (DePatchPlugin.Instance.Config.AdminGrid && target is IMySlimBlock)
            {
                if (CheckAdminGrid(target))
                {
                    damage.Amount = 0;
                    damage.IsDeformation = false;
                    return;
                }
            }

            // if disabled, return
            if (!DePatchPlugin.Instance.Config.ProtectGrid || !DePatchPlugin.Instance.Config.Enabled) return;

            if (damage.Type != MyDamageType.Deformation && damage.Type != MyDamageType.Fall) return;

            MyEntities.TryGetEntityById(damage.AttackerId, out var AttackerEntity, allowClosed: true);

            var GridBlock = target as IMySlimBlock;
            var GridPhysics = GridBlock?.CubeGrid.Physics;
            var GridCube = GridBlock?.CubeGrid;
            var Grid = (MyCubeGrid)GridCube;

            if (GridBlock == null || GridPhysics == null ||
                (GridCube.GridSizeEnum != MyCubeSize.Large || Grid.BlocksCount >=
                    DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize) &&
                (GridCube.GridSizeEnum != MyCubeSize.Small || Grid.BlocksCount >=
                    DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize)) return;

            var speed = DePatchPlugin.Instance.Config.MinProtectSpeed;
            var LinearVelocity = GridPhysics.LinearVelocity;
            var AngularVelocity = GridPhysics.AngularVelocity;

            if (LinearVelocity.Length() < speed && AngularVelocity.Length() < speed)
            {
                //by voxel or grid on low speed
                if (AttackerEntity is MyVoxelBase)
                {
                    if (damage.IsDeformation) damage.IsDeformation = false;
                    if (damage.Amount == 0f) return;
                    damage.Amount = 0f;
                    return;
                }

                if (Grid.BlocksCount <= DePatchPlugin.Instance.Config.MaxBlocksDoDamage ||
                    AttackerEntity is MyCubeBlock block &&
                    block.CubeGrid.BlocksCount <= DePatchPlugin.Instance.Config.MaxBlocksDoDamage) return;

                if (damage.IsDeformation) damage.IsDeformation = false;
                if (damage.Amount > 0f) damage.Amount = 0f;
            }
            else
            {
                if (AttackerEntity is MyVoxelBase)
                { // by voxel on high speed
                    if (damage.IsDeformation) damage.IsDeformation = false;

                    if (damage.Amount != 0f)
                    {
                        if (damage.Amount >= DePatchPlugin.Instance.Config.DamageToBlocksVoxel)
                            damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksVoxel;
                    }

                    _ = MyGravityProviderSystem.CalculateNaturalGravityInPoint(Grid.PositionComp.GetPosition(), out var ingravitynow);
                    if (ingravitynow <= 20 && ingravitynow >= 0.2)
                    {
                        if (DePatchPlugin.Instance.Config.ConvertToStatic &&
                            Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxGridSizeToConvert &&
                            (LinearVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed || AngularVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed))
                        {
                            var worldAABB = Grid.PositionComp.WorldAABB;
                            var closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
                            var elevation = double.PositiveInfinity;
                            if (closestPlanet != null)
                            {
                                var centerOfMassWorld = GridPhysics.CenterOfMassWorld;
                                var closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref centerOfMassWorld);
                                elevation = Vector3D.Distance(closestSurfacePointGlobal, centerOfMassWorld);
                            }
                            else
                            {
                                elevation = double.PositiveInfinity;
                            }

                            if (elevation < 200 && elevation != double.PositiveInfinity &&
                                Grid.GetFatBlockCount<MyMotorSuspension>() < 4 &&
                                Grid.GetFatBlockCount<MyThrust>() >= 6)
                            {
                                if (damage.Amount != 0f)
                                    damage.Amount = 0f;

                                GridPhysics?.ClearSpeed();

                                var pilots = new List<MyCharacter>();
                                foreach (var a in Grid.GetFatBlocks<MyCockpit>())
                                {
                                    if (a != null && a.Pilot != null)
                                    {
                                        pilots.Add(a.Pilot);
                                        a.RemovePilot();
                                    }
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
                                var grids = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(Grid);
                                grids.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
                                grids.Reverse();
                                grids.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                                var biggestGrid = grids.First();
                                var oldPosition = biggestGrid.PositionComp.GetPosition();
                                MyMultiplayer.RaiseEvent(biggestGrid, (MyCubeGrid x) => new Action(x.ConvertToStatic), default(EndpointId));

                                /* This part of code belong to LordTylus great plugin dev! FixShip after converting to static */
                                var gridWithSubGrids = FindGridGroup(Grid.DisplayName);
                                var objectBuilderList = new List<MyObjectBuilder_EntityBase>();
                                var gridsList = new List<MyCubeGrid>();
                                var box = BoundingBox.CreateInvalid();

                                foreach (var item in gridWithSubGrids)
                                {
                                    foreach (var groupNodes in item.Nodes)
                                    {
                                        var grid = groupNodes.NodeData;
                                        gridsList.Add(grid);

                                        var ob = grid.GetObjectBuilder(true);

                                        if (!objectBuilderList.Contains(ob))
                                        {
                                            if (ob is MyObjectBuilder_CubeGrid gridBuilder)
                                            {
                                                box.Include(gridBuilder.CalculateBoundingBox());
                                                foreach (var cubeBlock in gridBuilder.CubeBlocks)
                                                {
                                                    if (cubeBlock is MyObjectBuilder_OxygenTank o2Tank)
                                                        o2Tank.AutoRefill = false;
                                                }
                                            }
                                            objectBuilderList.Add(ob);
                                        }
                                    }
                                }

                                foreach (var grid in gridsList)
                                {
                                    if (grid == null || ((IMyEntity)grid).MarkedForClose || ((IMyEntity)grid).Closed)
                                        continue;

                                    ((IMyEntity)grid).Close();
                                }

                                Vector3D? vector3D3;
                                var boundingSphere = BoundingSphere.CreateFromBoundingBox(box);

                                var spawnInfo = new SpawnInfo
                                {
                                    CollisionRadius = boundingSphere.Radius,
                                    Planet = closestPlanet,
                                    PlanetDeployAltitude = boundingSphere.Radius * 1.2f
                                };
                                vector3D3 = MyRespawnComponentBase.FindPositionAbovePlanet(oldPosition,
                                    ref spawnInfo, true, 10, 50, 70);

                                if (vector3D3 == default)
                                {
                                    vector3D3 = oldPosition;
                                    pilots.Select(b => b.GetIdentity().IdentityId).ForEach(b => MyVisualScriptLogicProvider.ShowNotification("Your grid will be stuck in voxel! Good digging xD", 15000, MyFontEnum.Red, b));

                                    MyAPIGateway.Entities.RemapObjectBuilderCollection(objectBuilderList);

                                    foreach (var ob in objectBuilderList)
                                    {
                                        MyAPIGateway.Entities.CreateFromObjectBuilderParallel(ob, true, null);
                                    }
                                }
                                else
                                {
                                    var vector3D2 = oldPosition - closestPlanet.PositionComp.GetPosition();
                                    vector3D2.Normalize();
                                    var vector3D = Vector3D.CalculatePerpendicularVector(vector3D2);
                                    var matrix = MatrixD.CreateWorld(vector3D3.Value, vector3D, vector3D2);
                                    var vector3D4 = Vector3D.TransformNormal(boundingSphere.Center, matrix);
                                    var position = vector3D3.Value - vector3D4;

                                    var gridPos = objectBuilderList.First();
                                    var pos = gridPos.PositionAndOrientation.GetValueOrDefault();
                                    pos.Position = position;
                                    gridPos.PositionAndOrientation = pos;
                                    var newMatrix = gridPos.PositionAndOrientation.Value.GetMatrix() * FindRotationMatrix((MyObjectBuilder_CubeGrid)gridPos);
                                    gridPos.PositionAndOrientation = new MyPositionAndOrientation(newMatrix);

                                    MyAPIGateway.Entities.RemapObjectBuilderCollection(objectBuilderList);

                                    foreach (var ob in objectBuilderList)
                                    {
                                        MyAPIGateway.Entities.CreateFromObjectBuilderParallel(ob, true, null);
                                    }
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
                            if (damage.Amount >= DePatchPlugin.Instance.Config.DamageToBlocksVoxel)
                                damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksVoxel;
                        }
                    }
                    return;
                }

                // check if it's torpedo on high speed.
                if (Grid.BlocksCount <= DePatchPlugin.Instance.Config.MaxBlocksDoDamage ||
                        AttackerEntity is MyCubeBlock block &&
                        block.CubeGrid.BlocksCount <= DePatchPlugin.Instance.Config.MaxBlocksDoDamage) return;

                if (Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                { // by grid bump high speed
                    if (damage.IsDeformation) damage.IsDeformation = false;

                    if (damage.Amount != 0f)
                    {
                        if (DePatchPlugin.Instance.Config.DamageToBlocksRamming == 0f)
                            DePatchPlugin.Instance.Config.DamageToBlocksRamming = 0.5f;

                        if (damage.Amount >= DePatchPlugin.Instance.Config.DamageToBlocksRamming)
                            damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksRamming;
                    }
                }
            }
        }

        private static MatrixD FindRotationMatrix(MyObjectBuilder_CubeGrid cubeGrid)
        {
            MatrixD matrixD = MatrixD.Identity;
            List<MyObjectBuilder_Cockpit> list = (from blk in cubeGrid.CubeBlocks.OfType<MyObjectBuilder_Cockpit>()
                                                  where !(blk is MyObjectBuilder_CryoChamber) && blk.SubtypeName.IndexOf("bathroom", StringComparison.InvariantCultureIgnoreCase) == -1
                                                  select blk).ToList();
            MyObjectBuilder_CubeBlock myObjectBuilder_CubeBlock = list.Find((MyObjectBuilder_Cockpit blk) => blk.IsMainCockpit) ?? list.FirstOrDefault();
            if (myObjectBuilder_CubeBlock == null)
            {
                List<MyObjectBuilder_RemoteControl> list2 = cubeGrid.CubeBlocks.OfType<MyObjectBuilder_RemoteControl>().ToList();
                myObjectBuilder_CubeBlock = (list2.Find((MyObjectBuilder_RemoteControl blk) => blk.IsMainCockpit) ?? list2.FirstOrDefault());
            }
            if (myObjectBuilder_CubeBlock == null)
            {
                myObjectBuilder_CubeBlock = cubeGrid.CubeBlocks.OfType<MyObjectBuilder_LandingGear>().FirstOrDefault();
            }
            if (myObjectBuilder_CubeBlock != null)
            {
                if (myObjectBuilder_CubeBlock.BlockOrientation.Up == Base6Directions.Direction.Right)
                {
                    matrixD *= MatrixD.CreateFromAxisAngle(Vector3D.Forward, MathHelper.ToRadians(-90f));
                }
                else
                {
                    if (myObjectBuilder_CubeBlock.BlockOrientation.Up == Base6Directions.Direction.Left)
                    {
                        matrixD *= MatrixD.CreateFromAxisAngle(Vector3D.Forward, MathHelper.ToRadians(90f));
                    }
                    else
                    {
                        if (myObjectBuilder_CubeBlock.BlockOrientation.Up == Base6Directions.Direction.Down)
                        {
                            matrixD *= MatrixD.CreateFromAxisAngle(Vector3D.Forward, MathHelper.ToRadians(180f));
                        }
                        else
                        {
                            if (myObjectBuilder_CubeBlock.BlockOrientation.Up == Base6Directions.Direction.Forward)
                            {
                                matrixD *= MatrixD.CreateFromAxisAngle(Vector3D.Left, MathHelper.ToRadians(-90f));
                            }
                            else
                            {
                                if (myObjectBuilder_CubeBlock.BlockOrientation.Up == Base6Directions.Direction.Backward)
                                {
                                    matrixD *= MatrixD.CreateFromAxisAngle(Vector3D.Left, MathHelper.ToRadians(90f));
                                }
                            }
                        }
                    }
                }
            }
            return matrixD;
        }
    }
}
