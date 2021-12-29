using Sandbox.Game.Entities.Cube;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyProductionBlockPatch
    {
        internal static MethodInfo OnRemoveQueueItem;

        public static void Patch(PatchContext ctx)
        {
            OnRemoveQueueItem = typeof(MyProductionBlock).EasyMethod("OnRemoveQueueItem");
            ctx.Prefix(typeof(MyProductionBlock), "RemoveFirstQueueItemAnnounce", typeof(MyProductionBlockPatch), nameof(RemoveFirstQueueItemAnnouncePatch));
        }

        public static bool RemoveFirstQueueItemAnnouncePatch(MyProductionBlock __instance, MyFixedPoint amount, float progress = 0f)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            if (__instance is MyRefinery)
            {
                // no need to send queue remove to client for refinery, it's has no queue. only assemblers have.
                OnRemoveQueueItem.Invoke(__instance, new ValueType[] { 0, amount, progress });
                return false;
            }

            return true;
        }
    }
}