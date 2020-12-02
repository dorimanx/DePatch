using System;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using Sandbox.ModAPI;
using VRage.Utils;
using VRage.Game.Entity;

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
        private static void HandleGridDamage(object target, ref MyDamageInformation damage)
        {
            if (DePatchPlugin.Instance.Config.ProtectGrid && DePatchPlugin.Instance.Config.Enabled)
            {
                if (damage.Type == MyDamageType.Deformation || damage.Type == MyDamageType.Fall)
                {
                    MyEntities.TryGetEntityById(damage.AttackerId, out MyEntity AttackerEntity, allowClosed: true);

                    try
                    {
                        IMySlimBlock GridBlock;
                        MyPhysicsComponentBase GridPhysics;
                        IMyCubeGrid GridCube;
                        MyCubeGrid Grid;
                        try
                        {
                            GridBlock = target as IMySlimBlock;
                            GridPhysics = GridBlock.CubeGrid.Physics;
                            GridCube = GridBlock.CubeGrid;
                            Grid = (MyCubeGrid)GridCube;
                        }
                        catch
                        {
                            return;
                        }

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
                                    GridPhysics?.ClearSpeed();

                                    if (damage.IsDeformation) damage.IsDeformation = false;

                                    float damagehit = damage.Amount;
                                    if (damagehit != 0f)
                                    {
                                        if (damagehit > DePatchPlugin.Instance.Config.DamgeToBlocksVoxel)
                                            damagehit = DePatchPlugin.Instance.Config.DamgeToBlocksVoxel;
                                        else
                                            damage.Amount = damagehit;
                                    }

                                    GridPhysics.ApplyImpulse(
                                        Grid.PositionComp.GetPosition() -
                                        ((LinearVelocity + AngularVelocity) * Grid.GridSize * 200f),
                                        Grid.PositionComp.GetPosition() + MyUtils.GetRandomVector3D());

                                    GridPhysics?.ClearSpeed();
                                }

                                if (Grid.BlocksCount > DePatchPlugin.Instance.Config.MaxBlocksDoDamage)
                                { // by grid bump high speed
                                    if (damage.IsDeformation) damage.IsDeformation = false;

                                    float damagehit = damage.Amount;
                                    if (damagehit != 0f)
                                    {
                                        if (damagehit > DePatchPlugin.Instance.Config.DamgeToBlocksRamming)
                                            damagehit = DePatchPlugin.Instance.Config.DamgeToBlocksRamming;
                                        else
                                            damage.Amount = damagehit;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { DePatchPlugin.Log.Warn(ex); }
                }
            }
        }
    }
}
