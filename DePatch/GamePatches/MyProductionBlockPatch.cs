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
        internal readonly static MethodInfo OnRemoveQueueItem = typeof(MyProductionBlock).GetMethod("OnRemoveQueueItem", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyProductionBlock).GetMethod("RemoveFirstQueueItemAnnounce", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).
                Prefixes.Add(typeof(MyProductionBlockPatch).GetMethod(nameof(RemoveFirstQueueItemAnnouncePatch), BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
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