using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Network;
using VRageMath;

namespace DePatch.VoxelProtection
{
    public class MyGridDeformationPatch
    {
        private struct GridDamageCheck
        {
            public bool ApplyDamage;
            public int Timer;

            public GridDamageCheck(bool ShouldApplyDamage)
            {
                ApplyDamage = ShouldApplyDamage;
                Timer = 0;
            }

            public void UpdateTimer()
            {
                Timer++;
            }
        }

        private static bool _init;
        private static ConcurrentDictionary<long, GridDamageCheck> TorpedoDamageSystem = new ConcurrentDictionary<long, GridDamageCheck>();
        private static Timer ItemTimer = new Timer(400.0);

        public static void Init()
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (!_init)
            {
                ItemTimer.Elapsed += ItemTimer_Elapsed;
                ItemTimer.Start();

                _init = true;

                if (MyAPIGateway.Session != null)
                    MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, HandleGridDamage);
            }
        }

        private static void ItemTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<long, GridDamageCheck> item in TorpedoDamageSystem)
            {
                if (item.Value.Timer > 2)
                    TorpedoDamageSystem.Remove(item.Key);

                item.Value.UpdateTimer();
            }
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

        private static void TorpedoHit(MyCubeGrid TargetGrid)
        {
            var GridBox = new MyOrientedBoundingBoxD(TargetGrid.PositionComp.LocalAABB, TargetGrid.WorldMatrix);
            var Entities = new List<MyEntity>();
            MyGamePruningStructure.GetAllEntitiesInOBB(ref GridBox, Entities);
            var BiggestAttacker = Entities.OfType<MyCubeGrid>().Aggregate((MyCubeGrid Grid1, MyCubeGrid Grid2) => (Grid1.BlocksCount > Grid2.BlocksCount) ? Grid1 : Grid2);
            if (BiggestAttacker != null && BiggestAttacker.EntityId != TargetGrid.EntityId)
            {
                bool ShouldApplyDamageA = false;
                bool ShouldApplyDamageB = false;

                if (TargetGrid.BlocksCount < DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                {
                    ShouldApplyDamageA = true;
                }

                if (BiggestAttacker.BlocksCount < DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                {
                    ShouldApplyDamageB = true;
                }

                if (!ShouldApplyDamageA && !ShouldApplyDamageB)
                {
                    TorpedoDamageSystem.TryAdd(TargetGrid.EntityId, new GridDamageCheck(ShouldApplyDamage: false));
                }
                else
                {
                    TorpedoDamageSystem.TryAdd(TargetGrid.EntityId, new GridDamageCheck(ShouldApplyDamage: true));
                }
            }
        }

        private static void HandleGridDamage(object target, ref MyDamageInformation damage)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

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
            if (!DePatchPlugin.Instance.Config.ProtectGrid) return;

            if (damage.Type != MyDamageType.Deformation && damage.Type != MyDamageType.Fall && damage.Type != MyDamageType.Destruction) return;

            if (!(target is MySlimBlock GridBlock)) return;

            if (damage.Amount == 0)
                return;

            var GridCube = GridBlock?.CubeGrid;
            var GridPhysics = GridBlock?.CubeGrid.Physics;

            if (GridBlock == null || GridPhysics == null) return;

            if (GridCube.GridSizeEnum == MyCubeSize.Large && GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize) return;
            if (GridCube.GridSizeEnum == MyCubeSize.Small && GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize) return;


            _ = MyEntities.TryGetEntityById(damage.AttackerId, out var AttackerEntity, allowClosed: true);
            if (AttackerEntity == null)
                return;

            if (GridPhysics.LinearVelocity.Length() < DePatchPlugin.Instance.Config.MinProtectSpeed && GridPhysics.AngularVelocity.Length() < DePatchPlugin.Instance.Config.MinProtectSpeed)
            {
                //by voxel or grid on low speed
                if (AttackerEntity is MyVoxelBase || AttackerEntity is MyVoxelMap)
                {
                    damage.Amount = 0f;
                    return;
                }
                damage.IsDeformation = false;
            }
            else
            {
                if (AttackerEntity is MyVoxelBase || AttackerEntity is MyVoxelMap)
                { // by voxel on high speed
                    damage.IsDeformation = false;

                    if (DePatchPlugin.Instance.Config.DamageToBlocksVoxel < 0)
                        DePatchPlugin.Instance.Config.DamageToBlocksVoxel = 0.1f;

                    if (damage.Amount > DePatchPlugin.Instance.Config.DamageToBlocksVoxel)
                        damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksVoxel;

                    _ = MyGravityProviderSystem.CalculateNaturalGravityInPoint(GridCube.PositionComp.GetPosition(), out var ingravitynow);
                    if (ingravitynow <= 20 && ingravitynow >= 0.2)
                    {
                        if (DePatchPlugin.Instance.Config.ConvertToStatic &&
                             GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxGridSizeToConvert &&
                            (GridPhysics.LinearVelocity.Length() > DePatchPlugin.Instance.Config.StaticConvertSpeed ||
                             GridPhysics.AngularVelocity.Length() > DePatchPlugin.Instance.Config.StaticConvertSpeed))
                        {
                            var worldAABB = GridCube.PositionComp.WorldAABB;
                            var closestPlanet = MyGamePruningStructure.GetClosestPlanet(ref worldAABB);
                            var elevation = double.PositiveInfinity;
                            if (closestPlanet != null)
                            {
                                var centerOfMassWorld = GridPhysics.CenterOfMassWorld;
                                var closestSurfacePointGlobal = closestPlanet.GetClosestSurfacePointGlobal(ref centerOfMassWorld);
                                elevation = Vector3D.Distance(closestSurfacePointGlobal, centerOfMassWorld);
                            }

                            if (elevation < 250 && elevation != double.PositiveInfinity &&
                                GridCube.GetFatBlockCount<MyMotorSuspension>() < 4 &&
                                GridCube.GetFatBlockCount<MyThrust>() >= 6)
                            {
                                damage.Amount = 0f;

                                GridPhysics?.ClearSpeed();

                                foreach (var Cockpit in GridCube.GetFatBlocks<MyCockpit>())
                                {
                                    if (Cockpit != null && Cockpit.Pilot != null)
                                        Cockpit.RemovePilot();
                                }
                                foreach (var Cryo in GridCube.GetFatBlocks<MyCryoChamber>())
                                {
                                    if (Cryo != null && Cryo.Pilot != null)
                                        Cryo.RemovePilot();
                                }

                                foreach (var projector in GridCube.GetFatBlocks<MyProjectorBase>())
                                {
                                    if (projector != null && projector.ProjectedGrid != null)
                                        projector.Enabled = false;
                                }

                                foreach (var drills in GridCube.GetFatBlocks<MyShipDrill>())
                                {
                                    if (drills != null && drills.Enabled)
                                        drills.Enabled = false;
                                }

                                /* This part of code belong to Foogs great plugin dev! */
                                var grids = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(GridCube);
                                grids.SortNoAlloc((x, y) => x.BlocksCount.CompareTo(y.BlocksCount));
                                grids.Reverse();
                                grids.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                                MyMultiplayer.RaiseEvent(grids.First(), (MyCubeGrid x) => new Action(x.ConvertToStatic), MyEventContext.Current.Sender);

                                /* This part of code belong to LordTylus great plugin dev! FixShip after converting to static */
                                ReloadShip.FixShip(GridCube.EntityId);

                                foreach (var ReloadedGrids in grids)
                                {
                                    if (ReloadedGrids.Physics == null)
                                        continue;
                                    ReloadedGrids.Physics?.ClearSpeed();
                                }
                            }
                        }
                    }
                    return;
                }

                if (GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                { // by grid bump high speed
                    damage.IsDeformation = false;

                    if (DePatchPlugin.Instance.Config.DamageToBlocksRamming <= 0f)
                        DePatchPlugin.Instance.Config.DamageToBlocksRamming = 0.5f;

                    if (damage.Amount > DePatchPlugin.Instance.Config.DamageToBlocksRamming)
                        damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksRamming;
                }
            }
        }
    }
}
