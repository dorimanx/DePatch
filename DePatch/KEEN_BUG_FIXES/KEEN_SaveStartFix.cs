using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using Sandbox.Game.World;
using Sandbox.Game.Screens.Helpers;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class KEEN_SaveStartFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_callbackOnFinished;
        private static MethodInfo PushInProgress;
        private static MethodInfo OnSnapshotDone;

        public static void Patch(PatchContext ctx)
        {
            m_callbackOnFinished = typeof(MyAsyncSaving).easyField("m_callbackOnFinished");
            PushInProgress = typeof(MyAsyncSaving).easyMethod("PushInProgress");
            OnSnapshotDone = typeof(MyAsyncSaving).easyMethod("OnSnapshotDone");

            ctx.Prefix(typeof(MyAsyncSaving), typeof(KEEN_SaveStartFix), nameof(Start));
        }

        public static bool Start(Action callbackOnFinished = null, string customName = null)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            try
            {
                PushInProgress.Invoke(null, new object[] {});

                var m_callbackOnFinishedLocal = m_callbackOnFinished.GetValue(null) as Action;
                m_callbackOnFinishedLocal = callbackOnFinished;

                OnSnapshotDone.Invoke(null, new object[] { MySession.Static.Save(out MySessionSnapshot snapshot, customName), snapshot });

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Game Save Start Function! Crash Avoided");
            }
            return false;
        }
    }
}