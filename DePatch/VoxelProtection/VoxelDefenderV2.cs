using HarmonyLib;
using Sandbox.Game.Entities;
using VRageMath;

namespace DePatch
{
    [HarmonyPatch(typeof(MyCubeGrid), "PerformCutouts")]
    internal class VoxelDefenderV2
    {
        private static bool Prefix(MyCubeGrid __instance)
        {
            if (!DePatchPlugin.Instance.Config.ProtectVoxels)
                return true;

            if (__instance != null && __instance.Physics != null)
            {
                if (!DePatchPlugin.Instance.Config.ProtectGrid && (__instance.Physics.LinearVelocity.Length() > DePatchPlugin.Instance.Config.MinProtectSpeed ||
                    __instance.Physics.AngularVelocity.Length() > DePatchPlugin.Instance.Config.MinProtectSpeed))
                {
                    Vector3D position = __instance.PositionComp.GetPosition();
                    __instance.Physics.ApplyImpulse(position - ((__instance.Physics.LinearVelocity + __instance.Physics.AngularVelocity) * __instance.Mass / 4.0f),
                        position + __instance.Physics.AngularVelocity);

                    __instance.Physics.LinearVelocity = Vector3D.Backward;
                    __instance.Physics.LinearVelocity = Vector3D.Up;
                    __instance.Physics?.ClearSpeed();
                }
                return false;
            }

            return false;
        }
    }
}
