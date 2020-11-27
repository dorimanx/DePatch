using System;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.ModAPI;
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
                        IMySlimBlock GridBlock = target as IMySlimBlock;
                        VRage.Game.Components.MyPhysicsComponentBase GridPhysics = GridBlock.CubeGrid.Physics;
                        IMyCubeGrid GridCube = GridBlock.CubeGrid;
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
                                if (Grid.BlocksCount > 50)
                                {
                                    if (damage.IsDeformation) damage.IsDeformation = false;
                                    if (damage.Amount != 0f) damage.Amount = 0f;
                                }
                            }
                            else
                            {
                                if (AttackerEntity is MyVoxelBase)
                                { // by voxel on high speed
                                    GridPhysics?.ClearSpeed();

                                    if (damage.IsDeformation) damage.IsDeformation = false;
                                    if (damage.Amount != 0f) damage.Amount = 0f;

                                    GridPhysics.ApplyImpulse(
                                        Grid.PositionComp.GetPosition() -
                                        ((LinearVelocity + AngularVelocity) * Grid.GridSize * 200f),
                                        Grid.PositionComp.GetPosition() + MyUtils.GetRandomVector3D());

                                    GridPhysics?.ClearSpeed();
                                }

                                if (Grid.BlocksCount > 50)
                                { // by grid bump high speed
                                    if (damage.IsDeformation) damage.IsDeformation = false;
                                    if (damage.Amount != 0f) damage.Amount = 0f;
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
