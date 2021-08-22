using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Network;
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
                _init = true;

                if (MyAPIGateway.Session != null)
                    MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, HandleGridDamage);
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

            if (damage.Type != MyDamageType.Fall)
            {
                if (damage.Type != MyDamageType.Deformation)
                    return;
            }

            if (!(target is MySlimBlock GridBlock))
                return;

            var GridCube = GridBlock?.CubeGrid;
            var GridPhysics = GridBlock?.CubeGrid.Physics;

            if (GridBlock == null || GridPhysics == null)
                return;

            if (GridCube.GridSizeEnum == MyCubeSize.Large && GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize)
                return;
            if (GridCube.GridSizeEnum == MyCubeSize.Small && GridCube.BlocksCount > DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize)
                return;

            _ = MyEntities.TryGetEntityById(damage.AttackerId, out var AttackerEntity, allowClosed: true);

            if (GridPhysics.LinearVelocity.Length() < DePatchPlugin.Instance.Config.MinProtectSpeed &&
                GridPhysics.AngularVelocity.Length() < DePatchPlugin.Instance.Config.MinProtectSpeed)
            {
                // detected grid on low speed.
                damage.IsDeformation = false;

                // no damage from voxels.
                if (AttackerEntity is MyVoxelBase || AttackerEntity is MyVoxelMap)
                {
                    damage.IsDeformation = false;
                    damage.Amount = 0f;
                    return;
                }

                // bump to other grid on low speed, prevent stuck in other blocks
                if (AttackerEntity is MyCubeBlock || AttackerEntity is MyCubeGrid)
                {
                    damage.IsDeformation = false;
                    if (GridCube.IsStatic)
                        damage.Amount = 0f;
                    else
                        damage.Amount = 0.02f;
                    return;
                }

                if ((AttackerEntity is MyLargeTurretBase || AttackerEntity is MyUserControllableGun) && damage.Type == MyDamageType.Deformation)
                    return;

                if (damage.AttackerId == 0L && damage.Amount == 0f && damage.Type == MyDamageType.Deformation)
                    return;

                // fully ignore damage if attacker is unknown.
                damage.IsDeformation = false;
                damage.Amount = 0f;
            }
            else
            {
                if (AttackerEntity is MyVoxelBase || AttackerEntity is MyVoxelMap)
                { // by voxel on high speed
                    damage.IsDeformation = false;

                    if (DePatchPlugin.Instance.Config.DamageToBlocksVoxel < 0 || DePatchPlugin.Instance.Config.DamageToBlocksVoxel > 1f)
                        DePatchPlugin.Instance.Config.DamageToBlocksVoxel = 0.05f;

                    if (damage.Amount != 0f && damage.Amount > DePatchPlugin.Instance.Config.DamageToBlocksVoxel)
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
                                if (damage.Amount != 0f)
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

                if (damage.Amount == 0f && GridCube.IsStatic)
                    damage.Amount = 0f;

                if ((AttackerEntity is MyLargeTurretBase || AttackerEntity is MyUserControllableGun) && damage.Type == MyDamageType.Deformation)
                    return;

                if (damage.AttackerId == 0L && damage.Amount == 0f && damage.Type == MyDamageType.Deformation)
                    return;

                // if damage is 0 then we are in PVE Zone. prevent blocks stuck in others grids and SAVE will fail.
                if (damage.Amount == 0)
                {
                    damage.IsDeformation = false;
                    damage.Amount = 0.01f;
                    return;
                }

                if (DePatchPlugin.Instance.Config.DamageToBlocksRamming <= 0f || DePatchPlugin.Instance.Config.DamageToBlocksRamming > 1f)
                    DePatchPlugin.Instance.Config.DamageToBlocksRamming = 0.05f;

                // bump to other grid on high speed, prevent stuck in other blocks
                if (AttackerEntity is MyCubeBlock || AttackerEntity is MyCubeGrid)
                {
                    damage.IsDeformation = false;
                    if (damage.Amount > DePatchPlugin.Instance.Config.DamageToBlocksRamming)
                        damage.Amount = DePatchPlugin.Instance.Config.DamageToBlocksRamming;
                    if (GridCube.IsStatic)
                        damage.Amount = 0f;

                    return;
                }

                // any other unknown damage set low damage.
                if (GridCube.IsStatic)
                    damage.Amount = 0f;
                else
                    damage.Amount = 0.02f;
            }
        }
    }
}
