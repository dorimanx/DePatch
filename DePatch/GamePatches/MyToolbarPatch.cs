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
        public static FieldInfo TerminalBlock { get; private set; }
        public static readonly Type MyToolbarItemTerminalBlockClass = Type.GetType("Sandbox.Game.Screens.Helpers.MyToolbarItemTerminalBlock, Sandbox.Game");

        public static void Patch(PatchContext ctx)
        {
            m_items = typeof(MyToolbar).easyField("m_items");
            TerminalBlock = MyToolbarItemTerminalBlockClass.easyField("m_block");

            ctx.Suffix(typeof(MyToolbar), "SetItemAtIndexInternal", typeof(MyToolbarPatch), "SetItemAtIndexInternalPatch");
        }

        private static void SetItemAtIndexInternalPatch(MyToolbar __instance, ref int i, ref MyToolbarItem item, ref bool initialization, bool gamepad = false)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.FixExploits)
                return;

            if (initialization || __instance is null || item is null || __instance.ToolbarType == MyToolbarType.Character)
                return;

            // deny adding toolbar item if ToolBar Block is owned by nobody = 0L but do allow if toolbar item block is also owned by 0L or shared with all.
            if (__instance.Owner != null && ((MyCubeBlock)__instance.Owner).IDModule != null && ((MyCubeBlock)__instance.Owner).IDModule.Owner == 0L)
            {
                try
                {
                    // item can change from MyToolbarItemTerminalBlock to MyTooMyToolbarItemTerminalGroup
                    var IsItGroup = item.GetObjectBuilder();
                    var steamId = MyEventContext.Current.Sender.Value;
                    var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                    {
                        if (!(item is MyToolbarItemWeapon) && TerminalBlock.GetValue(item) is MyTerminalBlock TerminalItemBlock)
                        {
                            if (TerminalItemBlock != null && TerminalItemBlock.IDModule != null && (TerminalItemBlock.IDModule.Owner == 0 || TerminalItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                return;

                            if (TerminalItemBlock != null && requesterPlayer != null && TerminalItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                return;
                        }
                    }

                    if (((MyToolbarItem[])m_items.GetValue(__instance))[i] != null)
                    {
                        __instance.SetItemAtIndex(i, null, false);
                        __instance.SetItemAtIndex(i, null, true);

                        if (__instance.GetControllerPlayerID() != 0L)
                            MyVisualScriptLogicProvider.ClearToolbarSlot(i, __instance.GetControllerPlayerID());
                        else
                            if (requesterPlayer != null)
                                MyVisualScriptLogicProvider.ClearToolbarSlot(i, requesterPlayer.Identity.IdentityId);
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