using Havok;
using Sandbox;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;
using VRageMath;
using VRage.ModAPI;
using NLog;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_RemoveEntityPhantomPathFix
    {
        // Code by Buddhist#3825 (Discord)
        // This patch fixing Keen Crash/total Hang during reading/writing to same dictionary that not even needed. and can crash on nulls, so we added missing null checks."
        // Crash observed when ship with subgrids enter/leave active safezone, or many ships enter same zone at same time.

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static MethodInfo entity_OnClose = typeof(MySafeZone).GetMethod("entity_OnClose", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Patch(PatchContext ctx)
        {
            MethodInfo _target = typeof(MySafeZone).GetMethod("RemoveEntityPhantom", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo _patch = typeof(KEEN_RemoveEntityPhantomPathFix).GetMethod(nameof(RemoveEntityPhantomPath), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(_target).Prefixes.Add(_patch);
        }

        private static bool RemoveEntityPhantomPath(MySafeZone __instance, HkRigidBody body, IMyEntity entity)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || MySandboxGame.Static.SimulationFrameCounter < 1000)
                return true;

            if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
            {
                if (__instance is null || body is null || entity is null)
                    return false;

                try
                {
                    MyEntity topEntity = (MyEntity)entity.GetTopMostParent();
                    if (topEntity.Physics is null || topEntity.Physics.ShapeChangeInProgress || topEntity != entity)
                        return false;

                    bool addedOrRemoved = MySessionComponentSafeZones.IsRecentlyAddedOrRemoved(topEntity) || !entity.InScene;

                    Vector3D position1 = entity.Physics.ClusterToWorld(body.Position);
                    Quaternion rotation1 = Quaternion.CreateFromRotationMatrix(body.GetRigidBodyMatrix());
                    MySandboxGame.Static.Invoke(action: () =>
                    {
                        try
                        {
                            if (__instance.Physics is null)
                                return;

                            if (entity.MarkedForClose)
                            {
                                if ((bool)ReflectionUtils.InvokeInstanceMethod(typeof(MySafeZone), __instance, "RemoveEntityInternal", new object[] { topEntity, addedOrRemoved }))
                                    ReflectionUtils.InvokeInstanceMethod(typeof(MySafeZone), __instance, "SendRemovedEntity", new object[] { topEntity.EntityId, addedOrRemoved });

                                return;
                            }

                            bool flag = ((entity as MyCharacter) != null && (entity as MyCharacter).IsDead) || body.IsDisposed || !entity.Physics.IsInWorld;

                            if (entity.Physics != null && !flag)
                            {
                                position1 = entity.Physics.ClusterToWorld(body.Position);
                                rotation1 = Quaternion.CreateFromRotationMatrix(body.GetRigidBodyMatrix());
                            }

                            Vector3D position = __instance.PositionComp.GetPosition();
                            MatrixD matrix = __instance.PositionComp.GetOrientation();
                            Quaternion fromRotationMatrix = Quaternion.CreateFromRotationMatrix(in matrix);
                            HkShape shape = HkShape.Empty;

                            if (entity.Physics != null)
                            {
                                if (entity.Physics.RigidBody != null)
                                    shape = entity.Physics.RigidBody.GetShape();
                                else if (entity.Physics is MyPhysicsBody physics && (entity as MyCharacter != null) && physics.CharacterProxy != null)
                                    shape = physics.CharacterProxy.GetHitRigidBody().GetShape();
                            }

                            if (flag || !shape.IsValid || !MyPhysics.IsPenetratingShapeShape(shape, ref position1, ref rotation1, __instance.Physics.RigidBody.GetShape(), ref position, ref fromRotationMatrix))
                            {
                                if ((bool)ReflectionUtils.InvokeInstanceMethod(typeof(MySafeZone), __instance, "RemoveEntityInternal", new object[] { topEntity, addedOrRemoved }))
                                {
                                    _ = ReflectionUtils.InvokeInstanceMethod(typeof(MySafeZone), __instance, "SendRemovedEntity", new object[] { topEntity.EntityId, addedOrRemoved });

                                    if (topEntity is MyCubeGrid myCubeGrid)
                                    {
                                        foreach (MyShipController myShipController in myCubeGrid.GetFatBlocks<MyShipController>())
                                        {
                                            if (!(myShipController is MyRemoteControl) && myShipController.Pilot != null && myShipController.Pilot != topEntity &&
                                                 (bool)ReflectionUtils.InvokeInstanceMethod(
                                                     typeof(MySafeZone), __instance, "RemoveEntityInternal", new object[] { myShipController.Pilot, addedOrRemoved }))
                                            {
                                                ReflectionUtils.InvokeInstanceMethod(
                                                    typeof(MySafeZone), __instance, "SendRemovedEntity", new object[] { myShipController.Pilot.EntityId, addedOrRemoved });
                                            }
                                        }
                                    }
                                }
                                topEntity.OnClose -= (Action<MyEntity>)entity_OnClose.Invoke(__instance, new object[] { __instance });
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("Phantom leave exception " + e);
                        }
                    }, invokerName: "Phantom leave", -1, 0);
                }
                catch (Exception e)
                {
                    Log.Error("MyRemoveEntityPhantomPatched error " + e);
                }
                return false;
            }
            return true;
        }
    }
}