using System;
using System.Reflection;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Torch;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Network;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyToolbarPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_items;
        public static FieldInfo Block { get; private set; }
        public static readonly Type MyToolbarItemTerminalBlockClass = Type.GetType("Sandbox.Game.Screens.Helpers.MyToolbarItemTerminalBlock, Sandbox.Game");
 
        internal static readonly MethodInfo MyToolbarSetItemAtIndexInternal = typeof(MyToolbar).GetMethod("SetItemAtIndexInternal", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo SendToolbarItemPatch = typeof(MyToolbarPatch).GetMethod(nameof(SetItemAtIndexInternalPatch), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            m_items = typeof(MyToolbar).GetField("m_items", BindingFlags.NonPublic | BindingFlags.Instance);
            Block = MyToolbarItemTerminalBlockClass.GetField("m_block", BindingFlags.Instance | BindingFlags.NonPublic);

            ctx.GetPattern(MyToolbarSetItemAtIndexInternal).Suffixes.Add(SendToolbarItemPatch);
        }

        private static void SetItemAtIndexInternalPatch(MyToolbar __instance, int i, MyToolbarItem item, ref bool initialization, bool gamepad = false)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.FixExploits)
                return;

            if (initialization)
                return;

            if (__instance is null || item is null || __instance.ToolbarType == MyToolbarType.Character)
                return;

            // deny adding toolbar item if ToolBar Block is owned by nobody = 0L but do allow if toolbar item block is also owned by 0L or shared with all.
            if (__instance.Owner != null && ((MyCubeBlock)__instance.Owner).IDModule != null && ((MyCubeBlock)__instance.Owner).IDModule.Owner == 0L)
            {
                try
                {
                    // item can change from MyToolbarItemTerminalBlock to MyTooMyToolbarItemTerminalGroup
                    var IsItGroup = item.GetObjectBuilder();

                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                    {
                        var ItemBlock = (MyTerminalBlock)Block.GetValue(item);
                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                            return;
                    }

                    if (((MyToolbarItem[])m_items.GetValue(__instance))[i] != null)
                    {
                        __instance.SetItemAtIndex(i, null, false);
                        __instance.SetItemAtIndex(i, null, true);

                        if (__instance.GetControllerPlayerID() != 0L)
                            MyVisualScriptLogicProvider.ClearToolbarSlot(i, __instance.GetControllerPlayerID());
                        else
                        {
                            var steamId = MyEventContext.Current.Sender.Value;
                            var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

                            if (requesterPlayer != null)
                                MyVisualScriptLogicProvider.ClearToolbarSlot(i, requesterPlayer.Identity.IdentityId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during SetItemAtIndexInternalPatch For ToolBars! Crash Avoided");
                }
            }
        }
    }
}