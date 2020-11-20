﻿using System;
using Sandbox.Game.Entities;
using VRage.Game;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;

namespace DePatch
{
    public class MyGridDeformationPatch
    {
        private static bool _init;
        public static void Init()
        {
            if (!_init)
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(1, new BeforeDamageApplied(HandleGridDamage));
                _init = true;
            }
        }
        private static void HandleGridDamage(object target, ref MyDamageInformation damage)
        {
            try
            {
                if (DePatchPlugin.Instance.Config.ProtectGrid)
                {
                    if ((damage.Type == MyDamageType.Deformation || damage.Type == MyDamageType.Fall) &&
                        damage.Type != MyDamageType.Environment && damage.Type != MyDamageType.Bullet && damage.Type != MyDamageType.Rocket &&
                        damage.Type != MyDamageType.Grind && damage.Type != MyDamageType.Weapon && damage.Type != MyDamageType.Weld &&
                        damage.Type != MyDamageType.Explosion)
                    {
                        MyCubeGrid grid;
                        try
                        {
                            grid = (MyCubeGrid)(target as IMySlimBlock).CubeGrid;
                        }
                        catch
                        {
                            return;
                        }

                        float speed = DePatchPlugin.Instance.Config.MinProtectSpeed;
                        float Lineargridspeed = grid.Physics.LinearVelocity.Length();
                        float Angulargridspeed = grid.Physics.AngularVelocity.Length();
                        var attacker = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
                        if (grid != null && grid.Physics != null && grid.GridSizeEnum == MyCubeSize.Small && grid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedSmallGridSize)
                        {
                            if (attacker == null && Lineargridspeed < speed && Angulargridspeed < speed)
                            { //by voxel
                                damage.Amount = 0f;
                                damage.IsDeformation = false;
                            }
                            else
                            {
                                if (Lineargridspeed > speed || Angulargridspeed > speed)
                                { // by player or Voxel
                                    if (grid.BlocksCount > 50)
                                    {
                                        Vector3D position = grid.PositionComp.GetPosition();
                                        damage.Amount = 0f;
                                        damage.IsDeformation = false;
                                        grid.Physics.ApplyImpulse(position - ((grid.Physics.LinearVelocity + grid.Physics.AngularVelocity) * grid.Mass / 4.0f),
                                            position + grid.Physics.AngularVelocity);

                                        grid.Physics?.ClearSpeed();
                                    }
                                }
                            }
                            return;
                        }

                        if (grid != null && grid.Physics != null && grid.GridSizeEnum == MyCubeSize.Large && grid.BlocksCount < DePatchPlugin.Instance.Config.MaxProtectedLargeGridSize)
                        {
                            if (attacker == null && Lineargridspeed < speed && Angulargridspeed < speed)
                            { //by voxel
                                damage.Amount = 0f;
                                damage.IsDeformation = false;
                                if (Lineargridspeed > 30 || Angulargridspeed > 30)
                                {
                                    Vector3D position = grid.PositionComp.GetPosition();
                                    grid.Physics.LinearVelocity = Vector3D.Backward;
                                    grid.Physics.LinearVelocity = Vector3D.Up;
                                    grid.Physics.ApplyImpulse(position - ((grid.Physics.LinearVelocity + grid.Physics.AngularVelocity) * grid.Mass / 4.0f),
                                        position + grid.Physics.AngularVelocity);

                                    grid.Physics?.ClearSpeed();
                                }
                            }
                            else
                            {
                                if (Lineargridspeed > speed || Angulargridspeed > speed)
                                { // by player or Voxel
                                    if (grid.BlocksCount > 50)
                                    {
                                        Vector3D position = grid.PositionComp.GetPosition();
                                        damage.Amount = 0f;
                                        damage.IsDeformation = false;
                                        grid.Physics.LinearVelocity = Vector3D.Backward;
                                        grid.Physics.LinearVelocity = Vector3D.Up;
                                        grid.Physics.ApplyImpulse(position - ((grid.Physics.LinearVelocity + grid.Physics.AngularVelocity) * grid.Mass / 4.0f),
                                                position + grid.Physics.AngularVelocity);

                                        grid.Physics?.ClearSpeed();
                                    }
                                }
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { DePatchPlugin.Log.Warn(ex); }
        }
    }
}
