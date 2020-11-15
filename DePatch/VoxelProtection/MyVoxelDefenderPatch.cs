using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Havok;
using Sandbox.Engine.Physics;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Entities.Debris;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.WorldEnvironment;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRage.ModAPI;

namespace DePatch
{
    internal class MyVoxelDefenderPatch
    {
        private static FieldInfo m_grid = ReflectionUtils.GetField<MyGridPhysics>(nameof(m_grid), true);

        private static bool Prefix(
          MyGridPhysics __instance,
          HkBreakOffLogicResult __result,
          HkRigidBody otherBody,
          uint shapeKey,
          ref float maxImpulse)
        {
            __result = MyVoxelDefenderPatch.Logic(__instance, otherBody, shapeKey, ref maxImpulse);
            return false;
        }

        private static HkBreakOffLogicResult Logic(
          MyGridPhysics __instance,
          HkRigidBody otherBody,
          uint shapeKey,
          ref float maxImpulse)
        {
            if ((double)maxImpulse == 0.0)
                maxImpulse = __instance.Shape.BreakImpulse;

            ulong user = 0;
            IMyEntity entity1 = otherBody.GetEntity(0U);
            if (entity1 is MyVoxelBase)
                return HkBreakOffLogicResult.DoNotBreakOff;

            MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entity1 as MyEntity);
            if (controllingPlayer != null)
                user = controllingPlayer.Id.SteamId;

            if (!MySessionComponentSafeZones.IsActionAllowed((MyEntity)MyVoxelDefenderPatch.m_grid.GetValue((object)__instance), MySafeZoneAction.Damage, 0L, user) || MySession.Static.Settings.EnableVoxelDestruction && entity1 is MyVoxelBase)
                return HkBreakOffLogicResult.DoNotBreakOff;

            HkBreakOffLogicResult breakOffLogicResult = HkBreakOffLogicResult.UseLimit;
            if (!Sync.IsServer)
                breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;

            else if ((HkReferenceObject)__instance.RigidBody == (HkReferenceObject)null || __instance.Entity.MarkedForClose || (HkReferenceObject)otherBody == (HkReferenceObject)null)
            {
                breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
            }
            else
            {
                IMyEntity entity2 = otherBody.GetEntity(0U);
                if (entity2 == null)
                {
                    return HkBreakOffLogicResult.DoNotBreakOff;
                }
                if (entity2 is MyEnvironmentSector || entity2 is MyFloatingObject || entity2 is MyDebrisBase)
                {
                    breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (entity2 is MyCharacter)
                {
                    breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (entity2.GetTopMostParent((System.Type)null) == __instance.Entity)
                {
                    breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else
                {
                    MyCubeGrid nodeB = entity2 as MyCubeGrid;
                    if (!MySession.Static.Settings.EnableSubgridDamage && nodeB != null && MyCubeGridGroups.Static.Physical.HasSameGroup((MyCubeGrid)MyVoxelDefenderPatch.m_grid.GetValue((object)__instance), nodeB))
                    {
                        breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                    }
                    else if (__instance.Entity is MyCubeGrid || nodeB != null)
                    {
                        breakOffLogicResult = HkBreakOffLogicResult.UseLimit;
                    }
                }
#pragma warning disable CS0612 // Тип или член устарел
                if (__instance.WeldInfo.Children.Count > 0)
#pragma warning restore CS0612 // Тип или член устарел
                    __instance.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable((HkEntity)__instance.RigidBody, __instance.Shape.BreakImpulse);
            }
            return breakOffLogicResult;
        }
    }
}
