using System;
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
        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyShipDrill).GetMethod("UpdateBeforeSimulation10", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).
                Prefixes.Add(typeof(MyShipDrillPatch).GetMethod(nameof(UpdateBeforeSimulation10), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static MethodInfo Receiver_IsPoweredChanged = typeof(MyShipDrill).GetMethod("Receiver_IsPoweredChanged", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingMethodException("missing Receiver_IsPoweredChanged");

        private static PropertyInfo ShakeAmount = typeof(MyShipDrill).GetProperty("ShakeAmount");

        private static FieldInfo m_drillFrameCountdown = typeof(MyShipDrill).GetField("m_drillFrameCountdown", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo m_drillBase = typeof(MyShipDrill).GetField("m_drillBase", BindingFlags.Instance | BindingFlags.NonPublic);

        public static FieldInfo m_wantsToCollect = typeof(MyShipDrill).GetField("m_wantsToCollect", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo InitSubBlocks = typeof(MyCubeBlock).GetMethod("InitSubBlocks", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool UpdateBeforeSimulation10(MyShipDrill __instance)
        {
            if (!DePatchPlugin.Instance.Config.DrillTools)
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
