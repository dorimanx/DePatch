using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.Entity.EntityComponents.Interfaces;
using VRage.ModAPI;
using VRage.Collections;

namespace DePatch.KEEN_BUG_FIXES
{
    public static class KEEN_UpdateOnceBeforeFrameFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_componentsForUpdateOnce;

        public static void Patch(PatchContext ctx)
        {
            m_componentsForUpdateOnce = typeof(MyGameLogic).EasyField("m_componentsForUpdateOnce");
            ctx.Prefix(typeof(MyGameLogic), typeof(KEEN_UpdateOnceBeforeFrameFix), nameof(UpdateOnceBeforeFrame));
        }

        public static bool UpdateOnceBeforeFrame()
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.UpdateOnceBeforeFrameFix)
                return true;

            try
            {
                var m_componentsForUpdateOnceList = (CachingList<MyGameLogicComponent>)m_componentsForUpdateOnce.GetValue(null);
                if (m_componentsForUpdateOnceList == null)
                    return true;

                m_componentsForUpdateOnceList.ApplyChanges();

                foreach (MyGameLogicComponent myGameLogicComponent in m_componentsForUpdateOnceList)
                {
                    if (myGameLogicComponent == null)
                        continue;

                    myGameLogicComponent.NeedsUpdate &= ~MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

                    if (!myGameLogicComponent.MarkedForClose && !myGameLogicComponent.Closed)
                        ((IMyGameLogicComponent)myGameLogicComponent).UpdateOnceBeforeFrame(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during UpdateOnceBeforeFrame Function! Crash Avoided");
            }

            return false;
        }
    }
}