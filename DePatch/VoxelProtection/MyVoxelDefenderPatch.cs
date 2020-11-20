using System.Reflection;
using Havok;
using Sandbox.Engine.Physics;
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
        private static FieldInfo m_grid = ReflectionUtils.GetField<MyGridPhysics>(nameof(Grid), true);

        public static FieldInfo Grid { get => m_grid; set => m_grid = value; }

        private static bool Prefix(
            MyGridPhysics __instance,
            HkBreakOffLogicResult __result,
            HkRigidBody otherBody,
            uint shapeKey,
            ref float maxImpulse)
        {
            _ = Logic(__instance, otherBody, shapeKey, ref maxImpulse);
            return false;
        }

        private static HkBreakOffLogicResult Logic(
          MyGridPhysics __instance,
          HkRigidBody otherBody,
          uint shapeKey,
          ref float maxImpulse)
        {
            if (maxImpulse == 0.0)
                maxImpulse = __instance.Shape.BreakImpulse;

            ulong user = 0;
            IMyEntity entity1 = otherBody.GetEntity(0U);
            if (entity1 is MyVoxelBase)
                return HkBreakOffLogicResult.DoNotBreakOff;

            MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entity1 as MyEntity);
            if (controllingPlayer != null)
                user = controllingPlayer.Id.SteamId;

            if (!MySessionComponentSafeZones.IsActionAllowed((MyEntity)Grid.GetValue(__instance), MySafeZoneAction.Damage, 0L, user) || (MySession.Static.Settings.EnableVoxelDestruction && entity1 is MyVoxelBase))
            {
                return HkBreakOffLogicResult.DoNotBreakOff;
            }

            HkBreakOffLogicResult breakOffLogicResult = HkBreakOffLogicResult.UseLimit;
            if (!Sync.IsServer || __instance.RigidBody == null || __instance.Entity.MarkedForClose || otherBody == null)
                breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
            else
            {
                if (otherBody.GetEntity(0U) == null)
                {
                    return HkBreakOffLogicResult.DoNotBreakOff;
                }
                if (otherBody.GetEntity(0U) is MyEnvironmentSector ||
                    otherBody.GetEntity(0U) is MyFloatingObject ||
                    otherBody.GetEntity(0U) is MyDebrisBase ||
                    otherBody.GetEntity(0U) is MyCharacter ||
                    otherBody.GetEntity(0U).GetTopMostParent(null) == __instance.Entity)
                {
                    breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                }
                else
                {
                    if (!MySession.Static.Settings.EnableSubgridDamage &&
                        otherBody.GetEntity(0U) as MyCubeGrid != null &&
                        MyCubeGridGroups.Static.Physical.HasSameGroup((MyCubeGrid)Grid.GetValue(__instance), otherBody.GetEntity(0U) as MyCubeGrid))
                    {
                        breakOffLogicResult = HkBreakOffLogicResult.DoNotBreakOff;
                    }
                    else if (__instance.Entity is MyCubeGrid || otherBody.GetEntity(0U) as MyCubeGrid != null)
                    {
                        breakOffLogicResult = HkBreakOffLogicResult.UseLimit;
                    }
                }
#pragma warning disable CS0612 // Тип или член устарел
                if (__instance.WeldInfo.Children.Count > 0)
#pragma warning restore CS0612 // Тип или член устарел
                    __instance.HavokWorld.BreakOffPartsUtil.MarkEntityBreakable(__instance.RigidBody, __instance.Shape.BreakImpulse);
            }
            return breakOffLogicResult;
        }
    }
}
