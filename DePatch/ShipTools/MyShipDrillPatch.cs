using System.Linq;
using System.Reflection;
using Sandbox.Game.Entities;
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

            DrillUpdate(__instance);

            return false;
        }

        private static void DrillUpdate(MyShipDrill drill)
        {
            Receiver_IsPoweredChanged.Invoke(drill, new object[0]);
            InitSubBlocks.Invoke(drill, new object[0]);

            if (drill.Parent?.Physics == null)
                return;

            m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) - 10);
            if ((int)m_drillFrameCountdown.GetValue(drill) > 0)
                return;

            DrillSettings drillSettings = DrillSettings.drills.ToList().Find((DrillSettings b) => b.Subtype == drill.DefinitionId.SubtypeName);
            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
                m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) + DePatchPlugin.Instance.Config.DrillUpdateRate);
            else
                m_drillFrameCountdown.SetValue(drill, (int)m_drillFrameCountdown.GetValue(drill) + drillSettings.TickRate);

            if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out MyGunStatusEnum myGunStatusEnum))
            {
                ShakeAmount.SetValue(drill, 0f);
                return;
            }

            var wantsToCollect = (bool)m_wantsToCollect.GetValue(drill);

            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
                wantsToCollect = (bool)m_wantsToCollect.GetValue(drill) || DePatchPlugin.Instance.Config.DrillDisableRightClick;
            else if (drillSettings.DisableRightClick)
                wantsToCollect = (bool)m_wantsToCollect.GetValue(drill) || drillSettings.DisableRightClick;
            else
                wantsToCollect = (bool)m_wantsToCollect.GetValue(drill);

            var myDrillBase = (MyDrillBase)m_drillBase.GetValue(drill);
            if (myDrillBase.Drill(drill.Enabled || wantsToCollect, true, false, 0.1f))
            {
                ShakeAmount.SetValue(drill, 1f);
                return;
            }

            ShakeAmount.SetValue(drill, 0.5f);
        }
    }
}
