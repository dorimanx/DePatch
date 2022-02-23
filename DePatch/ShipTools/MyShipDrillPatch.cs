using System.Linq;
using System.Reflection;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI;

namespace DePatch.ShipTools
{
    [PatchShim]

    internal static class MyShipDrillPatch
    {
        private static readonly PropertyInfo ShakeAmount = typeof(MyShipDrill).GetProperty("ShakeAmount");
        private static MethodInfo Receiver_IsPoweredChanged;
        private static FieldInfo m_drillFrameCountdown;
        private static FieldInfo m_drillBase;
        public static FieldInfo m_wantsToCollect;
        private static MethodInfo InitSubBlocks;

        public static void Patch(PatchContext ctx)
        {
            Receiver_IsPoweredChanged = typeof(MyShipDrill).EasyMethod("Receiver_IsPoweredChanged");
            m_drillFrameCountdown = typeof(MyShipDrill).EasyField("m_drillFrameCountdown");
            m_drillBase = typeof(MyShipDrill).EasyField("m_drillBase");
            m_wantsToCollect = typeof(MyShipDrill).EasyField("m_wantsToCollect");
            InitSubBlocks = typeof(MyCubeBlock).EasyMethod("InitSubBlocks");

            ctx.Prefix(typeof(MyShipDrill), typeof(MyShipDrillPatch), nameof(UpdateBeforeSimulation10));
        }

        private static bool UpdateBeforeSimulation10(MyShipDrill __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.DrillTools)
                return true;

            if (DePatchPlugin.Instance.Config.ParallelDrill != DrillingMode.Keen)
                DePatchPlugin.Instance.Config.ParallelDrill = DrillingMode.Keen;

            if (__instance.BlockDefinition.Id.SubtypeName.Contains("NanobotDrillSystem"))
                return true;

            Receiver_IsPoweredChanged.Invoke(__instance, new object[0]);
            InitSubBlocks.Invoke(__instance, new object[0]);

            if (__instance.Parent?.Physics == null)
                return false;

            m_drillFrameCountdown.SetValue(__instance, (int)m_drillFrameCountdown.GetValue(__instance) - 10);
            if ((int)m_drillFrameCountdown.GetValue(__instance) > 0)
                return false;

            DrillSettings drillSettings = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == __instance.DefinitionId.SubtypeName);

            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
                m_drillFrameCountdown.SetValue(__instance, (int)m_drillFrameCountdown.GetValue(__instance) + DePatchPlugin.Instance.Config.DrillUpdateRate);
            else
                m_drillFrameCountdown.SetValue(__instance, (int)m_drillFrameCountdown.GetValue(__instance) + drillSettings.TickRate);

            if (__instance.CanShoot(MyShootActionEnum.PrimaryAction, __instance.OwnerId, out MyGunStatusEnum _))
            {
                var wantsToCollect = (bool)m_wantsToCollect.GetValue(__instance);

                if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
                    wantsToCollect = (bool)m_wantsToCollect.GetValue(__instance) || DePatchPlugin.Instance.Config.DrillDisableRightClick;
                else if (drillSettings.DisableRightClick)
                    wantsToCollect = (bool)m_wantsToCollect.GetValue(__instance) || drillSettings.DisableRightClick;
                else
                    wantsToCollect = (bool)m_wantsToCollect.GetValue(__instance);

                var myDrillBase = (MyDrillBase)m_drillBase.GetValue(__instance);

                if (myDrillBase.Drill(__instance.Enabled || wantsToCollect, speedMultiplier: 0.1f))
                    ShakeAmount.SetValue(__instance, 1f);
                else
                    ShakeAmount.SetValue(__instance, 0.5f);
            }
            else
                ShakeAmount.SetValue(__instance, 0.0f);

            return false;
        }
    }
}
