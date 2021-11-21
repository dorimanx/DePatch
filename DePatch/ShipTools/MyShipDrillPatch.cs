using System.Linq;
using System.Reflection;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;

namespace DePatch.ShipTools
{
    [PatchShim]

    internal static class MyShipDrillPatch
    {
        private static PropertyInfo ShakeAmount = typeof(MyShipDrill).GetProperty("ShakeAmount");
        private static MethodInfo Receiver_IsPoweredChanged;
        private static FieldInfo m_drillFrameCountdown;
        private static FieldInfo m_drillBase;
        public static FieldInfo m_wantsToCollect;
        private static MethodInfo InitSubBlocks;

        public static void Patch(PatchContext ctx)
        {
            Receiver_IsPoweredChanged = typeof(MyShipDrill).easyMethod("Receiver_IsPoweredChanged");
            m_drillFrameCountdown = typeof(MyShipDrill).easyField("m_drillFrameCountdown");
            m_drillBase = typeof(MyShipDrill).easyField("m_drillBase");
            m_wantsToCollect = typeof(MyShipDrill).easyField("m_wantsToCollect");
            InitSubBlocks = typeof(MyCubeBlock).easyMethod("InitSubBlocks");

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
                m_drillFrameCountdown.SetValue(drill, (int)(float)((int)m_drillFrameCountdown.GetValue(drill) + drillSettings.TickRate));

            if (!drill.CanShoot(MyShootActionEnum.PrimaryAction, drill.OwnerId, out var _))
            {
                ShakeAmount.SetValue(drill, 0f);
                return;
            }

            var collectOre = (bool)m_wantsToCollect.GetValue(drill);

            if (DePatchPlugin.Instance.Config.DrillIgnoreSubtypes)
                collectOre = ((bool)m_wantsToCollect.GetValue(drill) || DePatchPlugin.Instance.Config.DrillDisableRightClick);
            else if (drillSettings.DisableRightClick)
                collectOre = ((bool)m_wantsToCollect.GetValue(drill) || drillSettings.DisableRightClick);
            else
                collectOre = (bool)m_wantsToCollect.GetValue(drill);

            var myDrillBase = (MyDrillBase)m_drillBase.GetValue(drill);
            if (myDrillBase.Drill(drill.Enabled || collectOre, true, false, 0.1f))
            {
                ShakeAmount.SetValue(drill, 1f);
                return;
            }

            ShakeAmount.SetValue(drill, 0.5f);
        }
    }
}
