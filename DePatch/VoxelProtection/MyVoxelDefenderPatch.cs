using System.Reflection;
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
        private static FieldInfo m_grid = ReflectionUtils.GetField<MyGridPhysics>("m_grid", isPrivate: true);

        private static bool Prefix(
            MyGridPhysics __instance,
            HkBreakOffLogicResult __result,
            HkRigidBody otherBody,
            uint shapeKey,
            ref float maxImpulse)
        {
            __result = Logic(__instance, otherBody, shapeKey, ref maxImpulse);
            return false;
        }

        private static HkBreakOffLogicResult Logic(
          MyGridPhysics __instance,
          HkRigidBody otherBody,
          uint shapeKey,
          ref float maxImpulse)
        {
            if (maxImpulse == 0f)
                maxImpulse = __instance.Shape.BreakImpulse;

            ulong user = 0uL;
            IMyEntity entity = otherBody.GetEntity(0u);
            if (entity is MyVoxelBase)
                return HkBreakOffLogicResult.DoNotBreakOff;

            MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entity as MyEntity);
            if (controllingPlayer != null)
                user = controllingPlayer.Id.SteamId;

            if (!MySessionComponentSafeZones.IsActionAllowed((MyEntity)m_grid.GetValue(__instance), MySafeZoneAction.Damage, 0L, user) ||
                (MySession.Static.Settings.EnableVoxelDestruction && entity is MyVoxelBase))
            {
                return HkBreakOffLogicResult.DoNotBreakOff;
            }
            HkBreakOffLogicResult result = HkBreakOffLogicResult.UseLimit;
            if (!Sync.IsServer)
            {
                result = HkBreakOffLogicResult.DoNotBreakOff;
            }
            else if (__instance.RigidBody == null || __instance.Entity.MarkedForClose || otherBody == null)
            {
                result = HkBreakOffLogicResult.DoNotBreakOff;
            }
            else
            {
                IMyEntity entity2 = otherBody.GetEntity(0u);
                if (entity2 == null)
                {
                    return HkBreakOffLogicResult.DoNotBreakOff;
                }
                if (entity2 is MyEnvironmentSector || entity2 is MyFloatingObject || entity2 is MyDebrisBase)
                {
                    result = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (entity2 is MyCharacter)
                {
                    result = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else if (entity2.GetTopMostParent() == __instance.Entity)
                {
                    result = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else
                {
                    if (!MySession.Static.Settings.EnableSubgridDamage && (entity2 as MyCubeGrid) != null &&
                        MyCubeGridGroups.Static.Physical.HasSameGroup((MyCubeGrid)m_grid.GetValue(__instance), entity2 as MyCubeGrid))
                    {
                        result = HkBreakOffLogicResult.DoNotBreakOff;
                    }
                    else if (__instance.Entity is MyCubeGrid || entity2 as MyCubeGrid != null)
                    {
                        result = HkBreakOffLogicResult.UseLimit;
                    }
                }
#pragma warning disable CS0612 // Тип или член устарел
                if (__instance.WeldInfo.Children.Count > 0)
#pragma warning restore CS0612 // Тип или член устарел
                    __instance.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(__instance.RigidBody, __instance.Shape.BreakImpulse);
            }
            _ = MyFakes.DEFORMATION_LOGGING;
            return result;
        }
    }
}
