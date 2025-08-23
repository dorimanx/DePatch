using DePatch.CoolDown;
using Sandbox.Game.Entities;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;
using VRage.Game.ObjectBuilders.Components;
using VRageMath;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyPVESafeZoneAction
    {
        public static bool BootTickStarted = true;
        private static bool ServerBootLoopStart = true;

        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MySessionComponentSafeZones), "IsActionAllowed", typeof(MyPVESafeZoneAction), nameof(IsActionAllowedPatch1), new[] { "entity", "action", "sourceEntityId", "user" });
            ctx.Prefix(typeof(MySessionComponentSafeZones), "IsActionAllowed", typeof(MyPVESafeZoneAction), nameof(IsActionAllowedPatch2), new[] { "aabb", "action", "sourceEntityId", "user" });
            ctx.Prefix(typeof(MySessionComponentSafeZones), "IsActionAllowed", typeof(MyPVESafeZoneAction), nameof(IsActionAllowedPatch3), new[] { "point", "action", "sourceEntityId", "user" });
        }

        public static void UpdateBoot()
        {
            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
            {
                if (ServerBootLoopStart)
                {
                    if (DePatchPlugin.Instance.Config.DelayShootingOnBootTime <= 0)
                        DePatchPlugin.Instance.Config.DelayShootingOnBootTime = 1;

                    int LoopCooldown = DePatchPlugin.Instance.Config.DelayShootingOnBootTime * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, LoopCooldown);
                    ServerBootLoopStart = false;
                    BootTickStarted = true;
                }

                if (BootTickStarted)
                {
                    // loop for X sec after boot to block weapons.
                    _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopOnBootRequestID, null, out var remainingSecondsBoot);

                    if (remainingSecondsBoot < 1)
                        BootTickStarted = false;
                }
            }
        }

        private static bool IsActionAllowedPatch1(MyEntity entity, MySafeZoneAction action, long sourceEntityId, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            var IsInPVEZone = false;
            __result = false;

            if (entity is MyCubeGrid grid)
            {
                switch (action)
                {
                    case MySafeZoneAction.Shooting:
                        {
                            if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
                            {
                                if (BootTickStarted)
                                {
                                    // block weapons
                                    return false;
                                }
                            }

                            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                                return true;

                            if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                                return true;

                            if (PVE.CheckEntityInZone(entity, ref IsInPVEZone))
                                return false;

                            return true;
                        }
                }
            }
            return true;
        }

        public static bool IsActionAllowedPatch2(BoundingBoxD aabb, MySafeZoneAction action, long sourceEntityId, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            __result = false;

            switch (action)
            {
                case MySafeZoneAction.Shooting:
                    {
                        if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
                        {
                            if (BootTickStarted)
                            {
                                // block weapons
                                return false;
                            }
                        }

                        if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                            return true;

                        if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                            return true;

                        BoundingBoxD Zone1 = PVE.PVESphere.GetBoundingBox();
                        BoundingBoxD Zone2 = PVE.PVESphere2.GetBoundingBox();

                        if (Zone1.Intersects(aabb) || (DePatchPlugin.Instance.Config.PveZoneEnabled2 && Zone2.Intersects(aabb)))
                            return false;

                        return true;
                    }
            }
            return true;
        }

        public static bool IsActionAllowedPatch3(Vector3D point, MySafeZoneAction action, long sourceEntityId, ulong user, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            __result = false;

            switch (action)
            {
                case MySafeZoneAction.Shooting:
                    {
                        if (DePatchPlugin.Instance.Config.DelayShootingOnBoot)
                        {
                            if (BootTickStarted)
                            {
                                // block weapons
                                return false;
                            }
                        }

                        if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                            return true;

                        if (DePatchPlugin.Instance.Config.AllowToShootNPCinZone)
                            return true;

                        BoundingBoxD Zone1 = PVE.PVESphere.GetBoundingBox();
                        BoundingBoxD Zone2 = PVE.PVESphere2.GetBoundingBox();
                        BoundingBoxD boundingBox = new BoundingBoxD(point - 1.0, point + 1f);

                        if (Zone1.Intersects(boundingBox) || (DePatchPlugin.Instance.Config.PveZoneEnabled2 && Zone2.Intersects(boundingBox)))
                            return false;

                        return true;
                    }
            }
            return true;
        }
    }
}
