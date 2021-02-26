using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
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
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, HandleGridDamage);
                _init = true;
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

            if (damage.Type != MyDamageType.Deformation && damage.Type != MyDamageType.Fall && damage.Type != MyDamageType.Destruction) return;

            MyEntities.TryGetEntityById(damage.AttackerId, out var AttackerEntity, allowClosed: true);

            if (!(target is MySlimBlock GridBlock)) return;
            var GridCube = GridBlock?.CubeGrid;
            var GridPhysics = GridBlock?.CubeGrid.Physics;

            if (GridBlock == null || GridPhysics == null) return;

            if (GridCube.GridSizeEnum == MyCubeSize.Large && GridCube.BlocksCount >= DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize) return;
            if (GridCube.GridSizeEnum == MyCubeSize.Small && GridCube.BlocksCount >= DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize) return;

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

                TorpedoHit(GridCube);

                if (TorpedoDamageSystem.ContainsKey(GridCube.EntityId) && TorpedoDamageSystem[GridCube.EntityId].ApplyDamage)
                    return;

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

                    _ = MyGravityProviderSystem.CalculateNaturalGravityInPoint(GridCube.PositionComp.GetPosition(), out var ingravitynow);
                    if (ingravitynow <= 20 && ingravitynow >= 0.2)
                    {
                        if (DePatchPlugin.Instance.Config.ConvertToStatic &&
                            GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxGridSizeToConvert &&
                            (LinearVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed || AngularVelocity.Length() >= DePatchPlugin.Instance.Config.StaticConvertSpeed))
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
                            else
                            {
                                elevation = double.PositiveInfinity;
                            }

                            if (elevation < 200 && elevation != double.PositiveInfinity &&
                                GridCube.GetFatBlockCount<MyMotorSuspension>() < 4 &&
                                GridCube.GetFatBlockCount<MyThrust>() >= 6)
                            {
                                if (damage.Amount != 0f)
                                    damage.Amount = 0f;

                                GridPhysics?.ClearSpeed();

                                foreach (var a in GridCube.GetFatBlocks<MyCockpit>())
                                {
                                    if (a != null && a.Pilot != null)
                                    {
                                        a.RemovePilot();
                                    }
                                }

                                foreach (var projector in GridCube.GetFatBlocks<MyProjectorBase>())
                                {
                                    if (projector.ProjectedGrid == null) continue;

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

                                MyMultiplayer.RaiseEvent(grids.First(), (MyCubeGrid x) => new Action(x.ConvertToStatic), default(EndpointId));

                                /* This part of code belong to LordTylus great plugin dev! FixShip after converting to static */
                                ReloadShip.FixShip(GridCube.DisplayName);
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
                TorpedoHit(GridCube);

                if (TorpedoDamageSystem.ContainsKey(GridCube.EntityId) && TorpedoDamageSystem[GridCube.EntityId].ApplyDamage)
                    return;

                if (GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
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
    }
}
